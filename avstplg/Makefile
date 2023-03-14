#Solution configuration, use either "Debug" or "Release"
CONFIGURATION	= Debug
ASSEMBLY_NAME	= avstplg
SOLUTION_NAME  := $(ASSEMBLY_NAME)_NETFramework.sln
SRCDIR	       := src
OUTDIR	       := $(SRCDIR)/bin/$(CONFIGURATION)
MSBUILD_FLAGS	= /m
#Build command, either "msbuild" or "dotnet build"
BUILDCMD	= msbuild

all: build

build:
	$(BUILDCMD) /property:Configuration=$(CONFIGURATION) /t:Build $(MSBUILD_FLAGS) $(SOLUTION_NAME)
ifeq ($(BUILDCMD),msbuild)
	@chmod +x "./$(OUTDIR)/$(ASSEMBLY_NAME).exe"
endif

rebuild:
	$(BUILDCMD) /property:Configuration=$(CONFIGURATION) /t:Rebuild $(MSBUILD_FLAGS) $(SOLUTION_NAME)
ifeq ($(BUILDCMD),msbuild)
	@chmod +x "./$(OUTDIR)/$(ASSEMBLY_NAME).exe"
endif

clean:
	$(BUILDCMD) /property:Configuration=$(CONFIGURATION) /t:Clean $(MSBUILD_FLAGS) $(SOLUTION_NAME)

install: build
	@mkdir -p $(DESTDIR)/opt/avstplg
ifeq ($(BUILDCMD),msbuild)
	@cp -r $(OUTDIR)/* $(DESTDIR)/opt/avstplg
else
	@cp -r $(OUTDIR)/net6.0/* $(DESTDIR)/opt/avstplg
endif

uninstall:
	@rm -rf /opt/avstplg
