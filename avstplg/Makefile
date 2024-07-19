# Solution configuration, use either "Debug" or "Release".
CONFIGURATION	= Debug
FRAMEWORK	= net6.0
ASSEMBLY_NAME  := avstplg
OUTDIR	       := $(realpath $(dir $(lastword $(MAKEFILE_LIST))))/build
MSBUILD_FLAGS	= -m
MONO		=

# The BaseIntermediateOutputPath must end with a trailing slash.
override MSBUILD_FLAGS += -p:Configuration=$(CONFIGURATION)
override MSBUILD_FLAGS += -p:OutputPath=$(BINDIR)
override MSBUILD_FLAGS += -p:BaseIntermediateOutputPath=$(OBJDIR)

.PHONY: all build clean

all: build

# MSBuild for Mono. Publishing not supported.
ifneq ($(MONO),)
SOLUTION_NAME  := $(ASSEMBLY_NAME)_NETFramework.sln
BINDIR	       := $(OUTDIR)/mono/bin/$(CONFIGURATION)
OBJDIR	       := $(OUTDIR)/mono/obj/
MSBUILD	       := msbuild

# MSBuild for Mono does not mark output as an executable.
build: buildsolution
	@chmod +x "$(BINDIR)/$(ASSEMBLY_NAME).exe"

# MSBuild for .NET. Publish when building for portability.
else
SOLUTION_NAME  := $(ASSEMBLY_NAME).sln
BINDIR	       := $(OUTDIR)/bin/$(CONFIGURATION)/$(FRAMEWORK)
OBJDIR	       := $(OUTDIR)/obj/
MSBUILD	       := dotnet build

# RuntimeIdentifier (RID) is a must when publishing to a single file. Set it
# based on the current machine with --use-current-runtime.
# Due to certain limitations, specifying RID is supported only on project-level,
# not solution-level. ImportByWildcardBeforeSolution=false bypasses that.

override MSBUILD_FLAGS += --use-current-runtime --no-self-contained
override MSBUILD_FLAGS += -p:ImportByWildcardBeforeSolution=false
override MSBUILD_FLAGS += -p:PublishSingleFile=true
override MSBUILD_FLAGS += -p:PublishDir=$(BINDIR)/publish

build: publishsolution
endif

%solution:
	$(MSBUILD) $(SOLUTION_NAME) -t:$(subst solution,,$@) $(MSBUILD_FLAGS)

clean: cleansolution
