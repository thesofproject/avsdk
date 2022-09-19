#include <boost/program_options.hpp>
#include <iostream>
#include <fstream>
#include <map>
#include <regex>
#include <string>
#include <vector>
#include "fileupdate_listener.hpp"
#include "log_entry_spt.hpp"
#include "log_entry_icl.hpp"

using namespace boost::program_options;

class detailed_path {
public:
	detailed_path(const std::string &p, int id)
		: path(p), lib_id(id)
	{
	}

	detailed_path()
		: detailed_path("", 0)
	{
	}

	std::string path;
	int lib_id;
};

static void validate(boost::any& v,
		     const std::vector<std::string>& values,
		     detailed_path*, int)
{
	static std::regex r("(.+):(0x[0-9a-fA-F]+|[0-9]+)$"); // <path>:<lib_id>

	// Make sure no previous assignment to 'v' was made.
	validators::check_first_occurrence(v);
	// Extract the first string from 'values'. If there is more than
	// one string, it's an error, and exception will be thrown.
	const std::string& s = validators::get_single_string(values);

	std::smatch match;
	if (!std::regex_match(s, match, r))
		throw validation_error(validation_error::invalid_option_value);

	v = boost::any(detailed_path(match[1], boost::lexical_cast<int>(match[2])));
}

static void conflicting_options(const variables_map& vm,
				const char* opt1, const char* opt2)
{
	if (vm.count(opt1) && !vm[opt1].defaulted() &&
	    vm.count(opt2) && !vm[opt2].defaulted())
		throw std::logic_error(std::string("Conflicting options '") +
			               opt1 + "' and '" + opt2 + "'.");
}

template <typename LiteralT, class EntryT>
void process_logdump(std::istream &in, std::ostream &out,
		     std::map<int, std::map<uint64_t, LiteralT>> &dict)
{
	static_assert(std::is_convertible<EntryT *, ilog_entry *>::value,
		      "EntryT must be a derivate of ilog_entry");

	EntryT entry;
	std::string strbuf(entry.max_size(), ' ');
	char *buf = const_cast<char *>(strbuf.data());
	int prevpos;

	// cache for optimized access to entry information
	entry.assign_ptr(buf);
	uint32_t *data = (uint32_t *)(buf + entry.hdr_size());

	while (in.good()) {
		typename std::map<uint64_t, LiteralT>::iterator found;
		std::map<uint64_t, LiteralT> *cache;
		size_t size = entry.size(in.peek());

		prevpos = in.tellg();

		in.read(buf, size);
		if (in.fail() | in.eof())
			break;

		if (!entry.is_valid()) {
			in.seekg(prevpos + sizeof(uint32_t));
			continue;
		}

		auto mit = dict.find(entry.lib_id());
		if (mit == dict.end())
			goto skipover;

		cache = &mit->second;
		found = cache->find(entry.key());
		if (found != cache->end()) {
			if (write_entry(out, &found->second, entry, data) < 0)
				return;
			continue;
		}

	skipover:
		out << "Unknown record at position: " << prevpos << std::endl;
		// skip over bogus data (DWORD-aligned) and re-attempt parsing
		in.seekg(prevpos + sizeof(uint32_t));
	}
}

template <typename LiteralT, class EntryT>
static void do_work(std::vector<detailed_path> &paths,
		    const std::string &inpath, std::ostream &out,
		    bool follow)
{
	std::map<int, std::map<uint64_t, LiteralT>> dict;
	std::map<uint64_t, LiteralT> provider;

	for (auto it = paths.begin(); it != paths.end(); it++) {
		build_provider(provider, it->path);
		dict[it->lib_id] = provider;
	}

	std::ifstream infile(inpath, std::fstream::binary);

	if (!follow) {
		process_logdump<LiteralT, EntryT>(infile, out, dict);
		infile.close();
		return;
	}

	fileupdate_listener listener;
	listener.subscribe(inpath);

	while (1) {
		int prevpos;

		process_logdump<LiteralT, EntryT>(infile, out, dict);
		prevpos = infile.tellg();
		out.flush();

		if (!infile.eof())
			break;

		int ret = listener.wait_for_signal();
		if (ret) {
			std::cout << "wait for signal failed: " << ret << std::endl;
			break;
		}

		infile.clear();
		infile.seekg(prevpos, std::ios_base::beg);
		if (infile.tellg() == -1)
			break;
	}

	listener.unsubscribe();
	infile.close();
}

int main(int argc, char* argv[])
{
	options_description desc("Program options");
	variables_map vm;

	desc.add_options()
		("help", "produce a help screen")
		("version,v", "print the version number")
		("input,i", value<std::string>(),
			"fw trace binary file to parse")
		("output,o", value<std::string>(),
			"file to dump parsed text into")
		("csv", value< std::vector<detailed_path>>(),
			"csv symbol cache (in <path>:<lib_id> format)")
		("elf", value< std::vector<detailed_path>>(),
			"elf symbol cache (in <path>:<lib_id> format)")
		("follow,f", "monitor the input file")
		;

	try {
		store(parse_command_line(argc, argv, desc), vm);

		if (!vm.count("csv") && !vm.count("elf")) {
			std::cout << "Usage: avsparse_fwlog [options]\n";
			std::cout << desc;
			return 0;
		}

		if (!vm.count("input")) {
			std::cout << "Usage: avsparse_fwlog [options]\n";
			std::cout << desc;
			return 0;
		}

		conflicting_options(vm, "csv", "elf");

		std::ofstream outfile;
		std::ostream *out;
		std::string inpath = vm["input"].as<std::string>();
		bool follow = vm.count("follow");

		if (vm.count("output")) {
			outfile.exceptions(std::ifstream::failbit | std::ifstream::badbit);
			outfile.open(vm["output"].as<std::string>());
			out = &outfile;
		} else {
			out = &std::cout;
		}

		std::vector<detailed_path> symbols;
		if (vm.count("csv")) {
			symbols = vm["csv"].as<std::vector<detailed_path>>();
			do_work<struct log_literal1_5, log_entry_spt>(symbols, inpath, *out,
								      follow);
		} else {
			symbols = vm["elf"].as<std::vector<detailed_path>>();
			do_work<struct log_literal2_0, log_entry_icl>(symbols, inpath, *out,
								      follow);
		}

		if (outfile.is_open())
			outfile.close();

	} catch (std::exception &e) {
		std::cout << e.what() << "\n";
	}

	return 0;
}
