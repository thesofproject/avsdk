#if defined(_WIN32) || defined (__CYGWIN__)

#include <boost/filesystem/path.hpp>
#include <string>
#include "fileupdate_listener_win.hpp"

// size must be DWORD aligned as per ReadDirectoryChangesW spec
#define AVS_NOTIFY_BUFFER_SIZE (32 * 1024)

fileupdate_listener_win::fileupdate_listener_win()
{
	hFile = INVALID_HANDLE_VALUE;
	hEvent = nullptr;
	buffer = new unsigned char[AVS_NOTIFY_BUFFER_SIZE];
	// hEvent is unused by the system if lpCompletionRoutine is provided
	// in ReadDirectoryChangesW
	overlapped.hEvent = this;
}

fileupdate_listener_win::~fileupdate_listener_win()
{
	__unsubscribe();
	delete buffer;
}

void CALLBACK fileupdate_listener_win::completion_callback(DWORD dwErrorCode,
							   DWORD dwNumberOfBytesTransfered,
							   LPOVERLAPPED lpOverlapped)
{
	if (dwErrorCode || !dwNumberOfBytesTransfered)
		return;

	fileupdate_listener_win *listener = (fileupdate_listener_win *)lpOverlapped->hEvent;
	PFILE_NOTIFY_INFORMATION info;
	unsigned char *data = listener->buffer;

	do {
		info = (PFILE_NOTIFY_INFORMATION)data;
		if (!listener->filename.compare(info->FileName)) {
			SetEvent(listener->hEvent);
			break;
		}

		data += info->NextEntryOffset;
	} while (info->NextEntryOffset);

	ReadDirectoryChangesW(listener->hFile,
			      listener->buffer, sizeof(listener->buffer),
			      false, FILE_NOTIFY_CHANGE_LAST_WRITE, NULL,
			      &listener->overlapped, completion_callback);
}

bool fileupdate_listener_win::subscribe(const std::string &fullpath)
{
	if (hFile != INVALID_HANDLE_VALUE)
		return false; // nothing to do

	boost::filesystem::path p(fullpath);
	const std::string str = p.filename().string();

	// will be comparing against FILE_NOTIFY_INFORMATION::FileName later on
	// which is an array of WCHARs, so convert name to wstring here
	int count = MultiByteToWideChar(CP_ACP, 0, str.c_str(), (int)str.size(), NULL, 0);
	std::wstring wstr(count, 0);

	MultiByteToWideChar(CP_ACP, 0, str.c_str(), (int)str.size(), &wstr[0], count);
	filename = wstr;

	hFile = CreateFile(p.parent_path().c_str(), FILE_LIST_DIRECTORY,
			   FILE_SHARE_READ | FILE_SHARE_WRITE | FILE_SHARE_DELETE,
			   NULL, OPEN_EXISTING,
			   FILE_FLAG_BACKUP_SEMANTICS | FILE_FLAG_OVERLAPPED, NULL);
	if (hFile == INVALID_HANDLE_VALUE)
		return false;

	hEvent = CreateEvent(NULL, true, false, NULL);
	if (!hEvent) {
		CloseHandle(hFile);
		return false;
	}

	if (!ReadDirectoryChangesW(hFile, buffer, sizeof(buffer),
				   false, FILE_NOTIFY_CHANGE_LAST_WRITE, NULL,
				   &overlapped, completion_callback)) {
		CloseHandle(hEvent);
		CloseHandle(hFile);
		return false;
	}

	return true;
}

void fileupdate_listener_win::__unsubscribe()
{
	if (hFile == INVALID_HANDLE_VALUE)
		return; // nothing to do

	CancelIo(hFile);
	// clear callback
	ReadDirectoryChangesW(hFile, buffer, sizeof(buffer),
			      false, FILE_NOTIFY_CHANGE_LAST_WRITE, NULL,
			      &overlapped, NULL);

	if (!HasOverlappedIoCompleted(&overlapped))
		SleepEx(3, true);

	CloseHandle(hEvent);
	CloseHandle(hFile);
	hFile = INVALID_HANDLE_VALUE;
}

int fileupdate_listener_win::wait_for_signal()
{
	int ret;

	do {
		ret = WaitForSingleObjectEx(hEvent, INFINITE, true);
	} while (ret == WAIT_IO_COMPLETION);

	ResetEvent(hEvent);
	return ret;
}

#endif