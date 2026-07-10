# Solution configuration, use either "Debug" or "Release".
CONFIGURATION		= Debug
ASSEMBLY_NAME	       := nhltdecode
OUTDIR		       := $(realpath $(dir $(lastword $(MAKEFILE_LIST))))/build
MSBUILD_FLAGS		= -m -v:m -p:Configuration=$(CONFIGURATION)

.PHONY: all build clean

all: build

# MSBuild for Mono, enable with 'make MONO=1'. Publishing not supported.
ifeq ("$(origin MONO)", "command line")
SOLUTION_NAME	       := $(ASSEMBLY_NAME)_NETFramework.sln
MSBUILD		       := msbuild

# Mark output as executable as MSBuild for Mono does not do that.
build: buildsolution
	@chmod +x "${OUTDIR}/$(CONFIGURATION)/net4.8/bin/$(ASSEMBLY_NAME).exe"

# MSBuild for .NET, the default. Publish when building for portability.
else
SOLUTION_NAME	       := $(ASSEMBLY_NAME).sln
MSBUILD		       := dotnet build

build: publishsolution
endif

%solution:
	$(MSBUILD) $(SOLUTION_NAME) -t:$(subst solution,,$@) $(MSBUILD_FLAGS)

clean: cleansolution
