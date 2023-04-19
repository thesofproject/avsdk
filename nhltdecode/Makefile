# Solution configuration, use either "Debug" or "Release".
CONFIGURATION	= Debug
FRAMEWORK	= net6.0
ASSEMBLY_NAME	= nhltdecode
SOLUTION_NAME  := $(ASSEMBLY_NAME).sln
OUTDIR	       := $(realpath $(dir $(lastword $(MAKEFILE_LIST))))/build
MSBUILD_FLAGS	= -m -p:TargetFramework=$(FRAMEWORK)
MONO		=

.PHONY: all build clean

all: build

# MSBuild for Mono. Publishing not supported.
ifneq ($(MONO),)
BINDIR	       := $(OUTDIR)/mono/bin/$(CONFIGURATION)
OBJDIR	       := $(OUTDIR)/mono/obj/
MSBUILD	       := msbuild

# MSBuild for Mono does not mark output as an executable.
build: restoresolution buildsolution
	@chmod +x "$(BINDIR)/$(ASSEMBLY_NAME).exe"

# MSBuild for .NET. Publish when building for portability.
else
BINDIR	       := $(OUTDIR)/bin/$(CONFIGURATION)/$(FRAMEWORK)
OBJDIR	       := $(OUTDIR)/obj/
MSBUILD	       := dotnet build

# RuntimeIdentifier is a must when publishing to a single file.
# At the same time, ImportByWildcardBeforeSolution=false bypasses limitation
# of specifying RID being supported only on project-level, not solution-level.
MSBUILD_FLAGS  += --use-current-runtime -p:PublishSingleFile=true --no-self-contained \
		  -p:ImportByWildcardBeforeSolution=false \
		  -p:PublishDir=$(BINDIR)/publish

build: publishsolution
endif

# The BaseIntermediateOutputPath must end with a trailing slash.
%solution:
	$(MSBUILD) -p:Configuration=$(CONFIGURATION) \
		   -p:BaseIntermediateOutputPath=$(OBJDIR) \
		   -p:OutputPath=$(BINDIR) \
		   -t:$(subst solution,,$@) $(MSBUILD_FLAGS) $(SOLUTION_NAME)

clean: cleansolution
