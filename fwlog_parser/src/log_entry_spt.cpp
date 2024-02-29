/*
 * Copyright (c) 2020-2022, Intel Corporation. All rights reserved.
 *
 * Author: Cezary Rojewski <cezary.rojewski@intel.com>
 *
 * SPDX-License-Identifier: Apache-2.0
 */

#include <algorithm>
#include <boost/algorithm/string.hpp>
#include <fstream>
#include <map>
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
		throw std::invalid_argument("invalid record: \"" + record + "\"");

	// if 'message' token contains ',', its chunks need to be rejoined
	if (tokens.size() > LOG_LITERAL_TOKEN_COUNT) {
		// 'message' token is the 5th element and spans till 'param1'
		auto start = tokens.begin() + 5;
		auto end = tokens.end() - 4;

		tokens[5] = boost::algorithm::join(boost::make_iterator_range(start, end), ",");
		tokens.erase(start + 1, end);
	}

	// remove '"'s from the front and at the end, and trim
	for (auto it = tokens.begin(); it != tokens.end(); ++it) {
		std::string &s = *it;

		boost::trim(s);
		if (s.length() < 2)
			continue;
		while (s.front() == '\"' && s.back() == '\"') {
			s.erase(0, 1);
			s.pop_back();
			boost::trim(s);
		}
	}

	literal.key.file_id = atoi(tokens[0].c_str());
	literal.key.line_num = atoi(tokens[1].c_str());
	literal.filename = tokens[2];
	literal.provider = tokens[3];
	literal.loglevel = tokens[4];
	literal.message = tokens[5] + "\n";
	literal.param1 = tokens[6];
	literal.param2 = tokens[7];
	literal.param3 = tokens[8];
	literal.param4 = tokens[9];
}

void build_provider(std::map<uint64_t, struct log_literal1_5> &provider,
		    const std::string &inpath)
{
	std::ifstream csv(inpath);
	std::string line;

	while (std::getline(csv, line)) {
		struct log_literal1_5 literal = {0};

		init_literal(literal, line);
		provider.emplace(literal.key.entry_id, literal);
	}
}

int write_entry(std::ostream &out, struct log_literal1_5 *literal,
		const log_entry_spt &entry, uint32_t *data)
{
	static char buf[512];
	int ret;

	ret = snprintf(buf, sizeof(buf), "%llu: %u %u,%u %s(%u): %s ",
		       (unsigned long long)entry.data->timestamp, entry.data->core_id,
		       entry.data->module.type, entry.data->instance_id, literal->filename.c_str(),
		       literal->key.line_num, literal->loglevel.c_str());
	if (ret < 0)
		return ret;

	switch (entry.data->entry_length) {
	case 0:
		ret = snprintf(buf + ret, sizeof(buf) - ret, literal->message.c_str(), data[0]);
		break;
	case 1:
		ret = snprintf(buf + ret, sizeof(buf) - ret, literal->message.c_str(), data[0],
			       data[1]);
		break;
	case 2:
		ret = snprintf(buf + ret, sizeof(buf) - ret, literal->message.c_str(), data[0],
			       data[1], data[2]);
		break;
	case 3:
		ret = snprintf(buf + ret, sizeof(buf) - ret, literal->message.c_str(), data[0],
			       data[1], data[2], data[3]);
		break;
	default:
		throw std::invalid_argument("Unexpected number of payload dwords");
	}

	if (ret < 0)
		return ret;
	out << buf;

	return 0;
}
