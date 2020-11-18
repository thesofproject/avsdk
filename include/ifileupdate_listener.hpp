#ifndef AVS_IFILEUPDATE_LISTENER_HPP
#define AVS_IFILEUPDATE_LISTENER_HPP

#include <string>

class ifileupdate_listener {
public:
	virtual ~ifileupdate_listener()
	{
	}

	virtual bool subscribe(const std::string &fullpath) = 0;
	virtual void unsubscribe() = 0;
	virtual int wait_for_signal() = 0;
};

#endif