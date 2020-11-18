#ifndef AVS_ILOG_ENTRY_HPP
#define AVS_ILOG_ENTRY_HPP

#include <boost/cstdint.hpp>

class ilog_entry {
public:
	virtual ~ilog_entry()
	{
	};

	virtual void assign_ptr(char *buf) = 0;
	virtual size_t hdr_size() const = 0;
	virtual size_t size(unsigned int dwords = 0) const = 0;
	virtual size_t max_size() const = 0;
	virtual bool is_valid() const = 0;
	virtual uint32_t lib_id() const = 0;
};

#endif