#if defined(__linux__)

#include <boost/filesystem/path.hpp>
#include <iostream>
#include <memory>
#include <string>

#include <errno.h>
#include <fcntl.h>
#include <sys/inotify.h>
#include <unistd.h>
#include "fileupdate_listener_linux.hpp"

// buffer should be large enough to hold a range of
// inotify_event instances returned to read()
#define BUF_LEN ((sizeof(struct inotify_event) + FILENAME_MAX) * 1024)

fileupdate_listener_linux::fileupdate_listener_linux()
{
	fd = inotify_init();
	if (fd < 0)
		std::cerr << "inotify_init failed: " << errno << std::endl;

	FD_ZERO(&readfds);
	wd = EINVAL;
}

fileupdate_listener_linux::~fileupdate_listener_linux()
{
	__unsubscribe();

	int ret = close(fd);
	if (ret)
		std::cerr << "close failed: " << errno << std::endl;
}

bool fileupdate_listener_linux::subscribe(const std::string &fullpath)
{
	if (wd >= 0)
		return false; // nothing to do

	boost::filesystem::path p(fullpath);
	filename = p.filename().string();

	wd = inotify_add_watch(fd, p.parent_path().c_str(), IN_MODIFY);
	if (wd < 0) {
		std::cerr << "inotify_add_watch failed: " << errno << std::endl;
		return false;
	}

	return true;
}

void fileupdate_listener_linux::__unsubscribe()
{
	if (wd < 0)
		return; // nothing to do
	if (inotify_rm_watch(fd, wd))
		std::cerr << "inotify_rm_watch failed: " << errno << std::endl;
	wd = EINVAL;
}

int fileupdate_listener_linux::wait_for_signal()
{
	FD_SET(fd, &readfds);

again:
	int ret = select(fd + 1, &readfds, NULL, NULL, NULL);
	if (ret < 0) {
		std::cerr << "select failed: " << errno << std::endl;
		return errno;
	}

	if (!FD_ISSET(fd, &readfds))
		goto again; // no events to process

	std::unique_ptr<char[]> bufptr(new char[BUF_LEN]);
	char *buf = bufptr.get();
	ssize_t len, n = 0;

	len = read(fd, buf, BUF_LEN);

	while (n < len) {
		struct inotify_event *event = (struct inotify_event *)&buf[n];

		// test if we can stop blocking
		if (!filename.compare(event->name))
			return 0;
		n += sizeof(*event) + event->len;
	}

	goto again;
}

#endif