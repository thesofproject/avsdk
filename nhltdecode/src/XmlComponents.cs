using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace nhltdecode
{
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
        public AcpiDescriptionHeader EfiAcpiDescriptionHeader;
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
            xtable.EfiAcpiDescriptionHeader = table.Header;
            xtable.EndpointDescriptors = xdescs;
            xtable.OedSpecificConfig = table.OEDConfig.Capabilities;

            return xtable;
        }

        public NHLT ToNative()
        {
            var table = new NHLT();

            table.Header = EfiAcpiDescriptionHeader;
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
}
