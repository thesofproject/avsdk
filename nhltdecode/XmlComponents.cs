using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace nhltdecode
{
    public class AcpiDescriptionHeaderXml
    {
        public string Signature;
        public byte Revision;
        public string OemId;
        public string OemIdTableId;
        public uint OemRevision;
        [XmlElement(DataType = "hexBinary")]
        public byte[] CreatorId;
        public uint CreatorRevision;

        public static AcpiDescriptionHeaderXml FromNative(AcpiDescriptionHeader hdr)
        {
            var xhdr = new AcpiDescriptionHeaderXml();

            xhdr.Signature = Encoding.ASCII.GetString(hdr.Signature);
            xhdr.Revision = hdr.Revision;
            xhdr.OemId = Encoding.ASCII.GetString(hdr.OemId);
            xhdr.OemIdTableId = Encoding.ASCII.GetString(hdr.OemIdTableId);
            xhdr.OemRevision = hdr.OemRevision;
            xhdr.CreatorId = hdr.CreatorId;
            xhdr.CreatorRevision = hdr.CreatorRevision;

            return xhdr;
        }

        public AcpiDescriptionHeader ToNative()
        {
            var hdr = new AcpiDescriptionHeader();
            Encoding srcEncoding = Encoding.Unicode;

            hdr.Signature = Encoding.Convert(srcEncoding, Encoding.ASCII, srcEncoding.GetBytes(Signature));
            hdr.Revision = Revision;
            hdr.OemId = Encoding.Convert(srcEncoding, Encoding.ASCII, srcEncoding.GetBytes(OemId));
            hdr.OemIdTableId = Encoding.Convert(srcEncoding, Encoding.ASCII, srcEncoding.GetBytes(OemIdTableId));
            hdr.OemRevision = OemRevision;
            hdr.CreatorId = CreatorId;
            hdr.CreatorRevision = CreatorRevision;

            return hdr;
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
            xcfg.FormatConfiguration = new FormatConfigurationXml
            {
                Blob = cfg.Config.Capabilities
            };

            return xcfg;
        }

        public FormatConfig ToNative()
        {
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
        [XmlElement(DataType = "hexBinary")]
        public byte[] InstanceId = new byte[1];
        [XmlElement(DataType = "hexBinary")]
        public byte[] VendorId = new byte[2];
        [XmlElement(DataType = "hexBinary")]
        public byte[] DeviceId = new byte[2];
        [XmlElement(DataType = "hexBinary")]
        public byte[] RevisionId = new byte[2];
        [XmlElement(DataType = "hexBinary")]
        public byte[] SubsystemId = new byte[4];

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
            xdesc.VendorId = desc.VendorId;
            xdesc.DeviceId = desc.DeviceId;
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
            desc.VendorId = VendorId;
            desc.DeviceId = DeviceId;
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
            desc.DevicesInfo.Count = (byte)Devices.Length;
            desc.DevicesInfo.Devices = Devices;

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
}
