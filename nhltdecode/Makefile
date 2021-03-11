#Solution configuration, use either "Debug" or "Release"
CONFIGURATION=Debug

all: build

build:
	msbuild /property:Configuration=$(CONFIGURATION) /t:Build

rebuild:
	msbuild /property:Configuration=$(CONFIGURATION) /t:Rebuild

clean:
	msbuild /property:Configuration=$(CONFIGURATION) /t:Clean
