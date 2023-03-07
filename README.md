Intel Topology Tool for the skylake sound driver
================================================

 * [Important](#important)
 * [Overview](#overview)
   * [Building](#building)
 * [Contributing](#contributing)

# Important

The skylake-driver as well as all its collaterals is **deprecated in favour of the
[avs-driver](https://git.kernel.org/pub/scm/linux/kernel/git/torvalds/linux.git/tree/sound/soc/intel/avs)**.
It's recommended for end users of newer Linux kernel version to switch to the latter solution. See
the main branch of this repository for information regarding the
[avstplg](https://github.com/thesofproject/avsdk#avstplg) tool, which an equivalent of itt, but
targets the avs-driver.

# Overview

The Intel Topology Tool (itt) converts topology configuration files written in widely adopted XML
into alsa-lib friendly format i.e.: UseCaseManagement (UCM). The tool is indented for use with the
[skylake-driver](https://git.kernel.org/pub/scm/linux/kernel/git/torvalds/linux.git/tree/sound/soc/intel/skylake)
that is part of the Linux kernel.

The obtained UCM is then expected to be provided as an input to the alsatplg tool of
[alsa-utils](https://github.com/alsa-project/alsa-utils) to create a kernel-friendly binary. Sound
driver loads such topology binary during boot to provide rich user experience.

Branch
[for-skylake-driver](https://github.com/thesofproject/avs-topology-xml/tree/for-skylake-driver)
of avs-topology-xml repository hosts a wide range of topology configuration files which are input
for this tool.

Connected to:
[enable HDAudio+DMIC with skylake-driver](https://gist.github.com/crojewsk/4e6382bfb0dbfaaf60513174211f29cb)
guide.

## Building

itt is a C# and .NET based tool. Either .NET 6.0+ or .NET Framework 4.6+ can be used to build the
component. .NET 6.0 and onward is the recommended option as it's available through package manager
on most major Linux distributions. For additional information visit
[Install .NET on Linux](https://learn.microsoft.com/en-us/dotnet/core/install/linux).\
.NET Framework is supported on Windows natively while Linux support is provided with
[Mono](https://www.mono-project.com/download/stable/).

.NET 6.0+ configurations shall be built using `dotnet build` CLI which invokes `msbuild for .NET`
under the hood. For .NET Framework, a Makefile which is part of every project can be used to compile
using `msbuild for Mono`.

# Contributing

While the tool and its collaterals are shared mainly to help community deal with remaining problems
on the older configurations, any contributions are welcome.

Please see the chapter [Contributing](https://github.com/thesofproject/avsdk#contributing) on the
main branch for the guidelines.
