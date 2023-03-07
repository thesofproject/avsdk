#Solution configuration, use either "Debug" or "Release"
CONFIGURATION	= Debug
ASSEMBLY_NAME	= itt
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

#
# Address NuGet errors such as below by removing assets.json:
#
# Your project does not reference ".NETFramework,Version=v4.6.2" framework. Add a reference
# to ".NETFramework,Version=v4.6.2" in the "TargetFrameworks" property of your project file
# and then re-run NuGet restore.
#
clean:
	msbuild /property:Configuration=$(CONFIGURATION) /t:Clean $(MSBUILD_FLAGS) $(SOLUTION_NAME)
	rm -f $(SRCDIR)/obj/project.assets.json
