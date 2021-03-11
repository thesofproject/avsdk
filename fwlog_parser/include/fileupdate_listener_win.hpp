#if defined(_WIN32) || defined (__CYGWIN__)

#ifndef AVS_FILEUPDATE_LISTENER_WIN_HPP
#define AVS_FILEUPDATE_LISTENER_WIN_HPP

#ifndef UNICODE
#define UNICODE
#endif
#define NOMINMAX // fix min/max redefinition from windows.h
#include <windows.h>
#undef NOMINMAX

#include <string>
#include "ifileupdate_listener.hpp"

class fileupdate_listener_win : public ifileupdate_listener {
public:
	fileupdate_listener_win();
	virtual ~fileupdate_listener_win();

	virtual bool subscribe(const std::string &fullpath) override;
	virtual void unsubscribe() override;
	virtual int wait_for_signal() override;

private:
	static void CALLBACK completion_callback(DWORD dwErrorCode,
						 DWORD dwNumberOfBytesTransfered,
						 LPOVERLAPPED lpOverlapped);

	void *hFile;
	void *hEvent;
	std::wstring filename;
	unsigned char *buffer;
	OVERLAPPED overlapped;
};

#endif // AVS_FILEUPDATE_LISTENER_WIN_HPP

#endif // _WIN32