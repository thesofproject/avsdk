CC=g++
CFLAGS=-I./include -I/usr/local/include -L/usr/local/lib/boost/
ODIR=src
LIBS=-lboost_program_options -lboost_filesystem

_OBJ = main.cpp fileupdate_listener_linux.cpp fileupdate_listener_win.cpp \
       log_entry_icl.cpp log_entry_spt.cpp
OBJ = $(patsubst %,$(ODIR)/%,$(_OBJ))

avsfwlog_parse: $(OBJ)
	$(CC) -o $@ $^ $(CFLAGS) $(LIBS)

.PHONY: clean

clean:
	rm -f $(ODIR)/*.o avsfwlog_parse
