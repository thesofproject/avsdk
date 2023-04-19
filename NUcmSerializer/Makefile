# Solution configuration, use either "Debug" or "Release".
CONFIGURATION	= Debug
FRAMEWORK	= net6.0
ASSEMBLY_NAME	= NUcmSerializer
OUTDIR	       := $(realpath $(dir $(lastword $(MAKEFILE_LIST))))/build
MSBUILD_FLAGS	= -m
MONO		=

.PHONY: all build clean

all: build

# MSBuild for Mono.
ifneq ($(MONO),)
SOLUTION_NAME  := $(ASSEMBLY_NAME)_NETFramework.sln
BINDIR	       := $(OUTDIR)/mono/bin/$(CONFIGURATION)
OBJDIR	       := $(OUTDIR)/mono/obj/
MSBUILD	       := msbuild

# MSBuild for .NET.
else
SOLUTION_NAME  := $(ASSEMBLY_NAME).sln
BINDIR	       := $(OUTDIR)/bin/$(CONFIGURATION)/$(FRAMEWORK)
OBJDIR	       := $(OUTDIR)/obj/
MSBUILD	       := dotnet build
endif

# The BaseIntermediateOutputPath must end with a trailing slash.
%solution:
	$(MSBUILD) -p:Configuration=$(CONFIGURATION) \
		   -p:BaseIntermediateOutputPath=$(OBJDIR) \
		   -p:OutputPath=$(BINDIR) \
		   -t:$(subst solution,,$@) $(MSBUILD_FLAGS) $(SOLUTION_NAME)

build: buildsolution
clean: cleansolution
