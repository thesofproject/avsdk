/*
 * Copyright (c) 2020-2022, Intel Corporation. All rights reserved.
 *
 * Author: Cezary Rojewski <cezary.rojewski@intel.com>
 *
 * SPDX-License-Identifier: Apache-2.0
 */

#ifndef AVS_IFILEUPDATE_LISTENER_HPP
#define AVS_IFILEUPDATE_LISTENER_HPP

#include <string>

class ifileupdate_listener {
public:
	ifileupdate_listener(const ifileupdate_listener &l) = delete;
	ifileupdate_listener &operator=(ifileupdate_listener &l) = delete;

	ifileupdate_listener()
	{
	}

	virtual ~ifileupdate_listener()
	{
	}

	virtual bool subscribe(const std::string &fullpath) = 0;
	virtual void unsubscribe() = 0;
	virtual int wait_for_signal() = 0;
};

#endif