#Solution configuration, use either "Debug" or "Release"
CONFIGURATION	= Debug
ASSEMBLY_NAME	= avstplg
SRCDIR	       := src
OUTDIR	       := $(SRCDIR)/bin/$(CONFIGURATION)

all: build

build:
	msbuild /property:Configuration=$(CONFIGURATION) /t:Build
	@chmod +x "./$(OUTDIR)/$(ASSEMBLY_NAME).exe"

rebuild:
	msbuild /property:Configuration=$(CONFIGURATION) /t:Rebuild
	@chmod +x "./$(OUTDIR)/$(ASSEMBLY_NAME).exe"

clean:
	msbuild /property:Configuration=$(CONFIGURATION) /t:Clean
