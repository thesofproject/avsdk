#ifndef AVS_FILEUPDATE_LISTENER_HPP
#define AVS_FILEUPDATE_LISTENER_HPP

#if defined(__linux__)
#include "fileupdate_listener_linux.hpp"
typedef fileupdate_listener_linux fileupdate_listener;
#elif defined(_WIN32) || defined (__CYGWIN__)
#include "fileupdate_listener_win.hpp"
typedef fileupdate_listener_win fileupdate_listener;
#endif

#endif