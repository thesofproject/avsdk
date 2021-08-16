#Solution configuration, use either "Debug" or "Release"
CONFIGURATION = Debug
ASSEMBLY_NAME = itt
OUTPUT_PATH   = bin/$(CONFIGURATION)

all: build

build:
	msbuild /property:Configuration=$(CONFIGURATION) /t:Build
	@chmod +x "./$(OUTPUT_PATH)/$(ASSEMBLY_NAME).exe"

rebuild:
	msbuild /property:Configuration=$(CONFIGURATION) /t:Rebuild
	@chmod +x "./$(OUTPUT_PATH)/$(ASSEMBLY_NAME).exe"

clean:
	msbuild /property:Configuration=$(CONFIGURATION) /t:Clean
