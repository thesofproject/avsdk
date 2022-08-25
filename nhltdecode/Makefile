#Solution configuration, use either "Debug" or "Release"
CONFIGURATION	= Debug
ASSEMBLY_NAME	= nhltdecode
SRCDIR	       := src
OUTDIR	       := $(SRCDIR)/bin/$(CONFIGURATION)
MSBUILD_FLAGS	= /m

all: build

build:
	msbuild /property:Configuration=$(CONFIGURATION) /t:Build $(MSBUILD_FLAGS)
	@chmod +x "./$(OUTDIR)/$(ASSEMBLY_NAME).exe"

rebuild:
	msbuild /property:Configuration=$(CONFIGURATION) /t:Rebuild $(MSBUILD_FLAGS)
	@chmod +x "./$(OUTDIR)/$(ASSEMBLY_NAME).exe"

clean:
	msbuild /property:Configuration=$(CONFIGURATION) /t:Clean $(MSBUILD_FLAGS)
