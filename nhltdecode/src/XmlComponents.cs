using System;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace nhltdecode
{
    [XmlType("AcpiDescriptionHeader")]
    public struct AcpiDescriptionHeaderXml
    {
        public byte Revision;
        public string OemId;
        public string OemIdTableId;
        public uint OemRevision;
        public string CreatorId; // AslCompilerId
        public uint CreatorRevision; // AslCompilerRevision

        public static AcpiDescriptionHeaderXml FromNative(AcpiDescriptionHeader hdr)
        {
            return new AcpiDescriptionHeaderXml
            {
                Revision = hdr.Revision,
                OemRevision = hdr.OemRevision,
                CreatorRevision = hdr.CreatorRevision,
                OemId = PrintID(hdr.OemId),
                OemIdTableId = PrintID(hdr.OemIdTableId),
                CreatorId = PrintID(hdr.CreatorId)
            };
        }

        public AcpiDescriptionHeader ToNative()
        {
            return new AcpiDescriptionHeader
            {
                // Length and checksum are populated by NhltXml.ToNative()
                Signature = Encoding.ASCII.GetBytes("NHLT"),
                Revision = Revision,
                OemRevision = OemRevision,
                CreatorRevision = CreatorRevision,
                OemId = IdToBytes(OemId, 6),
                OemIdTableId = IdToBytes(OemIdTableId, 8),
                CreatorId = IdToBytes(CreatorId, 4)
            };
        }

        private static string PrintID(byte[] id)
        {
            string print = string.Empty;

            foreach (var letter in id)
            {
                // Ignore non printable ASCII characters
                if (letter >= ' ' && letter <= '~')
                    print += Encoding.ASCII.GetString(new byte[] { letter });
            }
            return print;
        }

        private static byte[] IdToBytes(string printed, int size)
        {
            byte[] id = Encoding.ASCII.GetBytes(printed);
            Array.Resize(ref id, size);
            return id;
        }
    }

    [XmlType("FormatConfig")]
    public class FormatConfigXml
    {
        [XmlAttribute("idx")]
        public int Idx;
        public WaveFormatExtensible Format;
        public FormatConfigurationXml FormatConfiguration;

        public static FormatConfigXml FromNative(FormatConfig cfg)
        {
            var xcfg = new FormatConfigXml();
            xcfg.Format = cfg.Format;
            xcfg.FormatConfiguration = FormatConfigurationXml.FromNative(cfg.Config);

            return xcfg;
        }

        public FormatConfig ToNative()
        {
            //Regenerate data that was omitted in xml
            //Constant values from dicumentation
            Format.FormatTag = 0xFFFE;
            Format.Size = 22;
            //Values calculated from other fields
            Format.BlockAlign = (ushort)(Format.Channels * Format.BitsPerSample / 8);
            Format.AvgBytesPerSec = Format.BlockAlign * Format.SamplesPerSec;

            var cfg = new FormatConfig();
            cfg.Format = Format;
            cfg.Config = FormatConfiguration.ToNative();

            return cfg;
        }
    }

    [XmlType("EndpointDescriptor")]
    public class EndpointDescriptorXml
    {
        [XmlAttribute("idx")]
        public int Idx;
        public byte LinkType; // 4 bits only
        public byte InstanceId;
        public string VendorId;
        public string DeviceId;
        public ushort RevisionId;
        public uint SubsystemId;

        public byte DeviceType;
        public byte Direction;
        public byte VirtualBusId;
        public EndpointSpecificConfigXml SpecificConfig;
        public FormatConfigXml[] FormatsConfiguration;
        public DeviceInfo[] Devices;

        public static EndpointDescriptorXml FromNative(EndpointDescriptor desc)
        {
            var xdesc = new EndpointDescriptorXml();

            xdesc.LinkType = (byte)desc.LinkType;
            xdesc.InstanceId = desc.InstanceId;
            xdesc.VendorId = string.Format("0x{0:X4}", desc.VendorId);
            xdesc.DeviceId = string.Format("0x{0:X4}", desc.DeviceId);
            xdesc.RevisionId = desc.RevisionId;
            xdesc.SubsystemId = desc.SubsystemId;
            xdesc.DeviceType = desc.DeviceType;
            xdesc.Direction = desc.Direction;
            xdesc.VirtualBusId = desc.VirtualBusId;

            xdesc.SpecificConfig = EndpointSpecificConfigXml.FromNative(desc.EndpointConfig);

            FormatConfig[] cfgs = desc.FormatsConfig.FormatsConfiguration;
            xdesc.FormatsConfiguration = new FormatConfigXml[cfgs.Length];
            for (var i = 0; i < cfgs.Length; i++)
            {
                xdesc.FormatsConfiguration[i] = FormatConfigXml.FromNative(cfgs[i]);
                xdesc.FormatsConfiguration[i].Idx = i;
            }

            xdesc.Devices = desc.DevicesInfo.Devices;

            return xdesc;
        }

        public EndpointDescriptor ToNative()
        {
            var desc = new EndpointDescriptor();

            desc.LinkType = (LINK_TYPE)LinkType;
            desc.InstanceId = InstanceId;
            desc.VendorId = VendorId.ToUInt16();
            desc.DeviceId = DeviceId.ToUInt16();
            desc.RevisionId = RevisionId;
            desc.SubsystemId = SubsystemId;
            desc.DeviceType = DeviceType;
            desc.Direction = Direction;
            desc.VirtualBusId = VirtualBusId;
            desc.EndpointConfig = SpecificConfig.ToNative();

            desc.FormatsConfig = new FormatsConfig();
            desc.FormatsConfig.FormatsCount = (byte)FormatsConfiguration.Length;
            desc.FormatsConfig.FormatsConfiguration = new FormatConfig[FormatsConfiguration.Length];
            for (byte i = 0; i < desc.FormatsConfig.FormatsCount; i++)
                desc.FormatsConfig.FormatsConfiguration[i] = FormatsConfiguration[i].ToNative();

            desc.DevicesInfo = new DevicesInfo();
            if (Devices != null)
            {
                desc.DevicesInfo.Count = (byte)Devices.Length;
                desc.DevicesInfo.Devices = Devices;
            }

            desc.EndpointDescriptorLength = (uint)desc.SizeOf();

            return desc;
        }
    }

    [XmlType("Nhlt")]
    public class NhltXml
    {
        public AcpiDescriptionHeaderXml EfiAcpiDescriptionHeader;
        public EndpointDescriptorXml[] EndpointDescriptors;
        [XmlElement(DataType = "hexBinary")]
        public byte[] OedSpecificConfig;

        public static NhltXml FromNative(NHLT table)
        {
            var xdescs = new EndpointDescriptorXml[table.DescriptorCount];
            for (var i = 0; i < table.DescriptorCount; i++)
            {
                xdescs[i] = EndpointDescriptorXml.FromNative(table.Descriptors[i]);
                xdescs[i].Idx = i;
            }

            var xtable = new NhltXml();
            xtable.EfiAcpiDescriptionHeader = AcpiDescriptionHeaderXml.FromNative(table.Header);
            xtable.EndpointDescriptors = xdescs;
            xtable.OedSpecificConfig = table.OEDConfig.Capabilities;

            return xtable;
        }

        public NHLT ToNative()
        {
            var table = new NHLT();

            table.Header = EfiAcpiDescriptionHeader.ToNative();
            table.DescriptorCount = (byte)EndpointDescriptors.Length;
            table.Descriptors = new EndpointDescriptor[table.DescriptorCount];
            for (byte i = 0; i < table.DescriptorCount; i++)
                table.Descriptors[i] = EndpointDescriptors[i].ToNative();

            table.OEDConfig.Capabilities = OedSpecificConfig;
            table.OEDConfig.CapabilitiesSize = (uint)OedSpecificConfig.Length;
            table.Header.Length = (uint)table.SizeOf();
            table.Header.Checksum = table.CalculateChecksum();

            return table;
        }
    }

    public struct WaveFormatExtensibleXml
    {
        public ushort Channels;
        public uint SamplesPerSec;
        public ushort BitsPerSample;
        public ushort ValidBitsPerSample;
        public string ChannelMask;
        public Guid Subformat;

        public static WaveFormatExtensibleXml FromNative(WaveFormatExtensible format)
        {
            return new WaveFormatExtensibleXml
            {
                Channels = format.Channels,
                SamplesPerSec = format.SamplesPerSec,
                BitsPerSample = format.BitsPerSample,
                ValidBitsPerSample = format.ValidBitsPerSample,
                ChannelMask = string.Format("0x{0:X8}", format.ChannelMask),
                Subformat = new Guid(format.Subformat),
            };
        }

        public WaveFormatExtensible ToNative()
        {
            WaveFormatExtensible format = new WaveFormatExtensible();

            format.Channels = Channels;
            format.SamplesPerSec = SamplesPerSec;
            format.BitsPerSample = BitsPerSample;
            format.ValidBitsPerSample = ValidBitsPerSample;
            format.Subformat = Subformat.ToByteArray();
            format.ChannelMask = ChannelMask.ToUInt32();

            // As per specification, below values ar constant or calculated
            format.FormatTag = 0xFFFE;
            format.Size = 22;
            format.BlockAlign = (ushort)(Channels * BitsPerSample / 8);
            format.AvgBytesPerSec = format.BlockAlign * format.SamplesPerSec;

            return format;
        }
    }

    public struct DeviceInfoXml
    {
        public Guid DeviceId;
        public byte DeviceInstanceId;
        public byte DevicePortId;

        public static DeviceInfoXml FromNative(DeviceInfo device)
        {
            return new DeviceInfoXml
            {
                DeviceId = new Guid(device.DeviceId),
                DeviceInstanceId = device.DeviceInstanceId,
                DevicePortId = device.DevicePortId
            };
        }
        public DeviceInfo ToNative()
        {
            return new DeviceInfo
            {
                DeviceId = DeviceId.ToByteArray(),
                DeviceInstanceId = DeviceInstanceId,
                DevicePortId = DevicePortId
            };
        }
    }
}
