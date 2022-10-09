SDK for Intel audio software solutions
========================

 * [Overview](#overview)
   * [Building](#)
 * [Tools](#tools)
   * [NUcmSerializer](#nucmserializer)
   * [avstplg](#avstplg)
   * [fwlog_parser](#fwlog_parser)
   * [nhltdecode](#nhltdecode)
   * [probe2wav](#probe2wav)

# Overview

Set of tools designed to help develop and debug software and firmware on Intel platforms with
AudioDSP onboard.

Related to [alsa-utils](https://github.com/alsa-project/alsa-utils) which is also set of utilities
but targets AdvancedLinuxSoundArchitecture (ALSA) audience in more general fashion.

## Building

Majority of the tools found here are C# and .NET based. Either .NET 6.0+ or .NET Framework 4.6+ can
be used to build the components. .NET 6.0 and onward is the recommended option as it's available
through package manager on most major Linux distributions. For additional information visit
[Install .NET on Linux](https://learn.microsoft.com/en-us/dotnet/core/install/linux).\
.NET Framework is supported on Windows natively while Linux support is provided with
[Mono](https://www.mono-project.com/download/stable/).

.NET 6.0+ configurations shall be built using `dotnet build` CLI which invokes `msbuild for .NET`
under the hood. For .NET Framework, a Makefile which is part of every project can be used to compile
using `msbuild for Mono`.

For C++ [fwlog_parser](#fwlog_parser), standard gcc toolchain and boost libraries
(program_options and filesystem) are required - widely available on both Windows and Linux
platforms.

# Tools

## NUcmSerializer

A UseCaseManagement (UCM) serializer library for .NET. Concentrates on topology side of UCM syntax
and hosts wide range of Section objects.

Behavior is similar to native to .NET XmlSerializer. Both serialization and deserialization are
supported. [avstplg](#avstplg) is an example its utilization.

## avstplg

Converts topology configuration files from a more friendly XML format into UCM i.e.: alsa-lib
friendly format.

[avs-topology-xml](https://github.com/thesofproject/avs-topology-xml) hosts a wide range of topology
configuration files which are input for this tool.

## fwlog_parser

Parses the binary AudioDSP log file into a human-friendly text equivalent given the symbol
dictionary. The symbol dictionaries are not part of the repository yet. References will be provided
in the future revision.

## nhltdecode

NonHDAudioLinkTable (NHLT) decoder and encoder. The NHLT is part of ACPICA and consists of hardware
related information for I2S and Digital Microphone (DMIC) devices. The specification is available on
[01.org](https://01.org/sites/default/files/595976_intel_sst_nhlt.pdf).

The tool allows for conversion of NHLT binary into a XML document file and vice versa.

## probe2wav

Straightforward extractor of wave data from the AudioDSP probe dumps. The data-probing functionality
is a debug feature which is part of AudioDSP firmware and allows for verifying the impact of
module's data processing on the stream as a whole by extracting or injecting data directly from or
to given module.

As the information is not gathered in real-time, the data comes in form of packets. The tool
simplifies the process of its extraction. Obtained wave file can be then verified.
