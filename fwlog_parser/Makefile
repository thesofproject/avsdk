CXX ?= g++
CXXFLAGS = -I./include -I/usr/local/include -L/usr/local/lib/boost/
SRCDIR = src
LIBS = -lboost_program_options -lboost_filesystem

SRCFILES = main.cpp fileupdate_listener_linux.cpp fileupdate_listener_win.cpp \
	   log_entry_icl.cpp log_entry_spt.cpp
OBJFILES = $(patsubst %,$(SRCDIR)/%,$(SRCFILES))

avsfwlog_parse: $(OBJFILES)
	$(CXX) -o $@ $^ $(CXXFLAGS) $(LIBS)

.PHONY: clean

clean:
	rm -f $(SRCDIR)/*.o avsfwlog_parse
