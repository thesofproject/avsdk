#if defined(__linux__)
#include <elf.h>
#elif defined(_WIN32) || defined (__CYGWIN__)
#include "elf.h"
#endif

#include <cstring>
#include <fstream>
#include <map>
#include <string>
#include <vector>
#include "log_entry_icl.hpp"

static int elf_find_section(std::vector<Elf32_Shdr> &sections,
			    const std::string &strings, const char *name)
{
	// searching for complete name so calculating its lenght is valid
	size_t len = strlen(name);

	for (int i = 0; i < sections.size(); i++)
		if (!strings.compare(sections[i].name, len, name))	
			return i;
	return -1;
}

static void elf_read_header(std::istream &elf, Elf32_Ehdr *ehdr)
{
	elf.read((char *)ehdr, sizeof(*ehdr));
	if (!IS_ELF(*ehdr))
		throw std::invalid_argument("Not a 32 bits ELF-LE file");
}

static void elf_read_sections(std::istream &elf, Elf32_Ehdr *ehdr,
			      std::vector<Elf32_Shdr> &sections)
{
	sections.resize(ehdr->shnum);
	elf.seekg(ehdr->shoff, std::ios_base::beg);
	elf.read((char *)sections.data(), sizeof(Elf32_Shdr) * ehdr->shnum);
}

static void elf_read_strings(std::istream &elf, Elf32_Shdr *strsection,
			     std::string &strings)
{
	strings.resize(strsection->size);
	elf.seekg(strsection->off, std::ios_base::beg);
	elf.read((char *)strings.data(), strsection->size);
}

static void elf_read_symbols(std::istream &elf, Elf32_Shdr *symsection,
			     std::vector<Elf32_Sym> &symbols)
{
	symbols.resize(symsection->size / sizeof(Elf32_Sym));
	elf.seekg(symsection->off, std::ios_base::beg);
	elf.read((char *)symbols.data(), symsection->size);
}

static void elf_init_literal(std::istream &elf, struct log_literal2_0 &literal,
			     Elf32_Sym *sym, Elf32_Shdr *shdr, Elf32_Shdr *funcstrs)
{
	elf.seekg(shdr->off + (sym->value - shdr->vaddr));
	elf.read((char *)&literal.hdr, sizeof(literal.hdr));

	literal.text.resize(literal.hdr.text_len);
	elf.read(const_cast<char *>(literal.text.data()), literal.hdr.text_len);

	if (literal.hdr.file >= funcstrs->vaddr &&
	    literal.hdr.file <= (funcstrs->vaddr + funcstrs->size)) {
		uint64_t offset = literal.hdr.file - funcstrs->vaddr;

		literal.filename.resize(FILENAME_MAX);
		elf.seekg(funcstrs->off + offset, std::ios_base::beg);
		elf.read(const_cast<char *>(literal.filename.data()), FILENAME_MAX);
	} else {
		literal.filename.assign("invalid_filename");
	}

	literal.entry_id = sym->value >> 7;
}

void build_provider(std::map<uint64_t, struct log_literal2_0> &provider,
		    const std::string inpath)
{
	std::ifstream elf(inpath, std::fstream::binary);

	std::vector<Elf32_Shdr> sections;
	std::vector<Elf32_Sym> symbols;
	Elf32_Shdr funcstrs;
	Elf32_Ehdr ehdr;
	std::string strings;

	elf_read_header(elf, &ehdr);
	elf_read_sections(elf, &ehdr, sections);
	elf_read_strings(elf, &sections[ehdr.shstrndx], strings);

	int idx = elf_find_section(sections, strings, ".symtab");
	if (idx == -1)
		throw std::invalid_argument("No symtab");

	elf_read_symbols(elf, &sections[idx], symbols);

	idx = elf_find_section(sections, strings, ".function_strings");
	if (idx == -1)
		throw std::invalid_argument("No functions_strings");

	funcstrs = sections[idx];

	for (auto it = symbols.begin(); it != symbols.end(); it++) {
		struct log_literal2_0 literal = {0};
		Elf32_Shdr *shdr;

		if (it->shndx >= ehdr.shnum)
			continue;
		shdr = &sections[it->shndx];
		// use strstr() instead of string::find() as it honors '\0'
		// which separate each entry
		if (!strstr(strings.data() + shdr->name, "log_entries"))
			continue;

		elf_init_literal(elf, literal, &*it, shdr, &funcstrs);
		provider.insert({literal.entry_id, literal});
	}

	elf.close();
}

int write_entry(std::ostream &out, struct log_literal2_0 *literal,
		const log_entry_icl &entry, uint32_t *data)
{
	std::string format;
	char buf[512];
	int ret;

	ret = snprintf(buf, sizeof(buf), "%lld: %s(%d):\n",
		       (unsigned long long)entry.data->timestamp,
		       literal->filename.data(), literal->hdr.line);
	if (ret < 0)
		return ret;
	out << buf;

	format = "%lld: " + literal->text;

	ret = snprintf(buf, sizeof(buf), format.data(),
		       entry.data->timestamp,
		       data[0], data[1], data[2], data[3],
		       data[4], data[5], data[6]);
	if (ret < 0)
		return ret;
	out << buf << std::endl;

	return 0;
}
