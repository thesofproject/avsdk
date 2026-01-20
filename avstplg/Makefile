# Solution configuration, use either "Debug" or "Release".
CONFIGURATION	= Debug
ASSEMBLY_NAME  := avstplg
OUTDIR	       := $(realpath $(dir $(lastword $(MAKEFILE_LIST))))/build
MSBUILD_FLAGS	= -m -v:m -p:Configuration=$(CONFIGURATION)

.PHONY: all build clean

all: build

# MSBuild for Mono. Publishing not supported.
ifeq ("$(origin MONO)", "command line")
SOLUTION_NAME  := $(ASSEMBLY_NAME)_NETFramework.sln
MSBUILD	       := msbuild

# MSBuild for Mono does not mark output as an executable.
build: buildsolution
	@chmod +x "${OUTDIR}/$(CONFIGURATION)/net4.5/bin/$(ASSEMBLY_NAME).exe"

# MSBuild for .NET. Publish when building for portability.
else
SOLUTION_NAME  := $(ASSEMBLY_NAME).sln
MSBUILD	       := dotnet build

build: publishsolution
endif

%solution:
	$(MSBUILD) $(SOLUTION_NAME) -t:$(subst solution,,$@) $(MSBUILD_FLAGS)

clean: cleansolution
