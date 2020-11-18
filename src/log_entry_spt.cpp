#include <algorithm>
#include <boost/algorithm/string.hpp>
#include <fstream>
#include <string>
#include <vector>
#include "log_entry_spt.hpp"

// Number of fields for struct log_literal1_5
#define LOG_LITERAL_TOKEN_COUNT 10

static void init_literal(struct log_literal1_5 &literal, const std::string &record)
{
	std::vector<std::string> tokens;

	// record is made of at least 10 elements, each separated with ','
	boost::split(tokens, record, boost::is_any_of(","));
	if (tokens.size() < LOG_LITERAL_TOKEN_COUNT)
		throw std::invalid_argument("invalid record: " + record);

	// remove '"' from the front and at the end
	for (auto it = tokens.begin(); it != tokens.end(); ++it) {
		std::string &s = *it;

		s.erase(0, 1);
		s.pop_back();
	}

	literal.file_id = atoi(tokens[0].c_str());
	literal.line_num = atoi(tokens[1].c_str());
	literal.filename = tokens[2];
	literal.provider = tokens[3];
	literal.loglevel = tokens[4];
	literal.message = tokens[5];
	literal.param1 = tokens[6];
	literal.param2 = tokens[7];
	literal.param3 = tokens[8];
	literal.param4 = tokens[9];
}

void build_provider(std::vector<struct log_literal1_5> &provider, const std::string inpath)
{
	std::ifstream csv(inpath);
	std::string line;

	while (std::getline(csv, line)) {
		struct log_literal1_5 literal = {0};

		init_literal(literal, line);
		provider.push_back(literal);
	}

	csv.close();
}

static int print_hex_dwords(char *buf, size_t size, uint32_t *data, int count, int rowsize = 4)
{
	int remaining = count;
	int written = 0;

	for (int i = 0; i < count; i += rowsize) {
		int ret, n = std::min(remaining, rowsize);

		for (int j = n - 1; j; j--) {
			ret = snprintf(buf + written, size - written, " %08X", *data++);
			if (ret < 0)
				return ret;
			written += ret;
		}

		ret = snprintf(buf + written, size - written, " %08X\n", *data++);
		if (ret < 0)
			return ret;
		written += ret;
		remaining -= n;
	}

	return written;
}

int write_entry(std::ostream &out, struct log_literal1_5 *literal,
		const log_entry_spt &entry, uint32_t *data)
{
	static char buf[512];
	int ret;

	ret = snprintf(buf, sizeof(buf), "%lld: %s(%d): res[%x,%d] %s %s\n",
		       (unsigned long long)entry.data->timestamp,
		       literal->filename.c_str(), literal->line_num,
		       entry.data->module.id, entry.data->instance_id,
		       literal->loglevel.c_str(), literal->message.c_str());
	if (ret < 0)
		return ret;
	out << buf;

	ret = snprintf(buf, sizeof(buf), "%lld: Payload (\"%s\", \"%s\", \"%s\", \"%s\"):\n",
		       (unsigned long long)entry.data->timestamp,
		       literal->param1.c_str(), literal->param2.c_str(),
		       literal->param3.c_str(), literal->param4.c_str());
	if (ret < 0)
		return ret;
	out << buf;

	ret = print_hex_dwords(buf, sizeof(buf), data, entry.data->entry_length + 1);
	if (ret < 0)
		return ret;;
	out << buf;

	return 0;
}
