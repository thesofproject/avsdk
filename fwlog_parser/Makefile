SRCDIR	       := src
OUTDIR	       := build

CXX		= g++
CXXFLAGS	= -std=c++11 -Wall
CPPFLAGS	=
DEPFLAGS	= -MT $@ -MMD -MP -MF $(OUTDIR)/$*.o.d
# Boost headers path is expected to be part of CPLUS_INCLUDE_PATH
INCLUDES       := -I ./include
# Boost libraries path is expected to be part of LIBRARY_PATH
LIBS	       := -lboost_program_options -lboost_filesystem

SRCFILES       := $(wildcard $(SRCDIR)/*.cpp)
OBJFILES       := $(patsubst $(SRCDIR)/%.cpp,$(OUTDIR)/%.o,$(SRCFILES))
DEPFILES       := $(SRCFILES:$(SRCDIR)/%.cpp=$(OUTDIR)/%.o.d)

.PHONY: all clean

all: avsfwlog_parse

avsfwlog_parse: $(OBJFILES)
	$(CXX) $(CXXFLAGS) $(CPPFLAGS) -o $(OUTDIR)/$@ $^ $(LIBS)

$(OUTDIR)/%.o: $(SRCDIR)/%.cpp $(OUTDIR)/%.o.d | $(OUTDIR)
	$(CXX) $(DEPFLAGS) $(CXXFLAGS) $(CPPFLAGS) $(INCLUDES) -c -o $@ $<

$(OUTDIR): ; mkdir -p $@

clean:
	rm -rf $(OUTDIR)/*

# Declare .d files as explicit targets so make does not consider them as
# intermediate files and does not delete them. Prevents thier unnecessary
# recreation if nothing has changed.
$(DEPFILES):
include $(wildcard $(DEPFILES))
