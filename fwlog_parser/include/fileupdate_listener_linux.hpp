#if defined(__linux__)

#ifndef AVS_FILEUPDATE_LISTENER_LINUX_HPP
#define AVS_FILEUPDATE_LISTENER_LINUX_HPP

#include <string>
#include "ifileupdate_listener.hpp"

class fileupdate_listener_linux : public ifileupdate_listener {
public:
	fileupdate_listener_linux();
	virtual ~fileupdate_listener_linux();

	virtual bool subscribe(const std::string &fullpath) override;
	virtual void unsubscribe() override
	{
		__unsubscribe();
	}

	virtual int wait_for_signal() override;

private:
	void __unsubscribe();

	int fd;
	int wd;
	fd_set readfds;
	std::string filename;
};

#endif // AVS_FILEUPDATE_LISTENER_LINUX_HPP

#endif // __linux__
