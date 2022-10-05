#Solution configuration, use either "Debug" or "Release"
CONFIGURATION	= Debug
ASSEMBLY_NAME	= probe2wav
SOLUTION_NAME  := $(ASSEMBLY_NAME)_NETFramework.sln
SRCDIR	       := src
OUTDIR	       := $(SRCDIR)/bin/$(CONFIGURATION)
MSBUILD_FLAGS	= /m

all: build

build:
	msbuild /property:Configuration=$(CONFIGURATION) /t:Build $(MSBUILD_FLAGS) $(SOLUTION_NAME)
	@chmod +x "./$(OUTDIR)/$(ASSEMBLY_NAME).exe"

rebuild:
	msbuild /property:Configuration=$(CONFIGURATION) /t:Rebuild $(MSBUILD_FLAGS) $(SOLUTION_NAME)
	@chmod +x "./$(OUTDIR)/$(ASSEMBLY_NAME).exe"

clean:
	msbuild /property:Configuration=$(CONFIGURATION) /t:Clean $(MSBUILD_FLAGS) $(SOLUTION_NAME)
