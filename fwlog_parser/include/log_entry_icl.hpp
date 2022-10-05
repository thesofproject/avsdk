/*
 * Copyright (c) 2020-2022, Intel Corporation. All rights reserved.
 *
 * Author: Cezary Rojewski <cezary.rojewski@intel.com>
 *
 * SPDX-License-Identifier: Apache-2.0
 */

#ifndef AVS_LOG_ENTRY_ICL_HPP
#define AVS_LOG_ENTRY_ICL_HPP

#include <boost/cstdint.hpp>
#include <map>
#include <string>
#include <vector>
#include "ilog_entry.hpp"

struct log_literal2_0 {
#pragma pack(push, 4)
	struct {
		uint64_t offset;
		uint32_t level;
		uint32_t log_source;
		uint32_t line;
		uint32_t file;
		uint32_t text_len;
	} hdr;
#pragma pack(pop)
	std::string text;
	std::string filename;
	union entry_key key;
};

#pragma pack(push, 4)
struct log_entry2_0 {
	uint32_t entry_length : 3; // in DWORDs
	uint32_t provider_id : 4;
	uint32_t entry_id : 25;
	uint64_t timestamp;
};
#pragma pack(pop)

#define LOG_ENTRY2_LENGTH_MASK	0x7 // corresponds to entry_length

class log_entry_icl : public ilog_entry {
public:
	log_entry_icl()
		: data(nullptr)
	{
	}

	virtual void assign_ptr(char *buf) override
	{
		data = (struct log_entry2_0 *)buf;
	}

	virtual size_t hdr_size() const override
	{
		return sizeof(*data);
	}

	virtual size_t size(unsigned int dwords = 0) const override
	{
		return (dwords & LOG_ENTRY2_LENGTH_MASK) * sizeof(uint32_t) + sizeof(*data);
	}

	virtual size_t max_size() const override
	{
		return size(LOG_ENTRY2_LENGTH_MASK);
	}

	virtual bool is_valid() const override
	{
		return data->entry_id;
	}

	virtual uint32_t lib_id() const override
	{
		return data->provider_id;
	}

	virtual uint64_t key() const override
	{
		return data->entry_id;
	}

	struct log_entry2_0 *data;
};

void build_provider(std::map<uint64_t, struct log_literal2_0> &provider,
		    const std::string &inpath);

int write_entry(std::ostream &out, struct log_literal2_0 *literal,
		const log_entry_icl &entry, uint32_t *data);

#endif
