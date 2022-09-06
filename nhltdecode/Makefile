#Solution configuration, use either "Debug" or "Release"
CONFIGURATION	= Debug
ASSEMBLY_NAME	= nhltdecode
SOLUTION_NAME  := $(ASSEMBLY_NAME).sln
DOTNET_TARGET	= net45
SRCDIR	       := src
OUTDIR	       := $(SRCDIR)/bin/$(CONFIGURATION)/$(DOTNET_TARGET)
MSBUILD_FLAGS	= /m
MSBUILD_PROPS	= /property:TargetFramework=$(DOTNET_TARGET) \
		  /property:Configuration=$(CONFIGURATION)

all: build

build:
	msbuild $(MSBUILD_PROPS) /t:Restore $(MSBUILD_FLAGS) $(SOLUTION_NAME)
	msbuild $(MSBUILD_PROPS) /t:Build $(MSBUILD_FLAGS) $(SOLUTION_NAME)
	@chmod +x "./$(OUTDIR)/$(ASSEMBLY_NAME).exe"

rebuild:
	msbuild $(MSBUILD_PROPS) /t:Rebuild $(MSBUILD_FLAGS) $(SOLUTION_NAME)
	@chmod +x "./$(OUTDIR)/$(ASSEMBLY_NAME).exe"

clean:
	msbuild $(MSBUILD_PROPS) /t:Clean $(MSBUILD_FLAGS) $(SOLUTION_NAME)
