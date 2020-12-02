#ifndef AVS_LOG_ENTRY_SPT_HPP
#define AVS_LOG_ENTRY_SPT_HPP

#include <boost/cstdint.hpp>
#include <map>
#include <string>
#include <vector>
#include "ilog_entry.hpp"

struct log_literal1_5 {
	unsigned short file_id;
	unsigned short line_num;
	std::string filename;
	std::string provider;
	std::string loglevel;
	std::string message;
	std::string param1;
	std::string param2;
	std::string param3;
	std::string param4;
};

#pragma pack(push, 4)
struct log_entry1_5 {
	uint32_t entry_length : 2; // in DWORDs
	uint32_t line_num : 14;
	uint32_t file_id : 15;
	uint32_t log_type : 1;

	uint16_t instance_id;
	union {
		uint16_t val;
		struct {
			uint16_t lib : 4;
			uint16_t id : 12;
		};
	} module;
	uint64_t timestamp;
};
#pragma pack(pop)

#define LOG_ENTRY_LENGTH_MASK	0x3 // corresponds to entry_length

class log_entry_spt : public ilog_entry {
public:
	log_entry_spt()
		: data(nullptr)
	{
	}

	void assign_ptr(char *buf) override
	{
		data = (struct log_entry1_5 *)buf;
	}

	virtual size_t hdr_size() const override
	{
		return sizeof(*data);
	}

	virtual size_t size(unsigned int dwords = 0) const override
	{
		// there is always at least one DWORD after the header
		return ((dwords & LOG_ENTRY_LENGTH_MASK) + 1) * sizeof(uint32_t) + sizeof(*data);
	}

	virtual size_t max_size() const override
	{
		return size(LOG_ENTRY_LENGTH_MASK);
	}

	virtual bool is_valid() const override
	{
		return data->file_id && data->line_num;
	}

	virtual uint32_t lib_id() const override
	{
		return data->module.lib;
	}

	struct log_entry1_5 *data;
};

// cache key: file_id, line_num
typedef std::pair<int, int> sptkey_t;

void build_provider(std::map<sptkey_t, struct log_literal1_5> &provider,
		    const std::string inpath);

template<>
sptkey_t inline entry_key<sptkey_t, log_entry_spt>(const log_entry_spt &entry)
{
	return std::make_pair<int, int>(entry.data->file_id, entry.data->line_num);
}

bool inline is_entry_matching(const log_entry_spt &entry, struct log_literal1_5 *literal)
{
	return entry.data->file_id == literal->file_id &&
	       entry.data->line_num == literal->line_num;
}

int write_entry(std::ostream &out, struct log_literal1_5 *literal,
		const log_entry_spt &entry, uint32_t *data);

#endif
