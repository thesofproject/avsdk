Intel Topology Tool for the skylake sound driver
================================================

 * [Overview](#overview)
 * [Important](#important)
 * [Contributing](#contributing)

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

# Important

The skylake-driver as well as all its collaterals is **deprecated in favour of the
[avs-driver](https://git.kernel.org/pub/scm/linux/kernel/git/torvalds/linux.git/tree/sound/soc/intel/avs)**.
It's recommended for end users of newer Linux kernel version to switch to the latter solution. See
the main branch of this repository for information regarding the
[avstplg](https://github.com/thesofproject/avsdk#avstplg) tool, which an equivalent of itt, but
targets the avs-driver.

# Contributing

While the tool and its collaterals are shared mainly to help community deal with remaining problems
on the older configurations, any contributions are welcome.

Please see the chapter [Contributing](https://github.com/thesofproject/avsdk#contributing) on the
main branch for the guidelines.
