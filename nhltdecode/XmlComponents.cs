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
    }

    [XmlType("FormatConfig")]
    public class FormatConfigXml
    {
        [XmlAttribute("idx")]
        public int Idx;
        public WaveFormatExtensible Format;
        [XmlElement(DataType = "hexBinary")]
        public byte[] FormatConfiguration;

        public static FormatConfigXml FromNative(FormatConfig cfg)
        {
            var xcfg = new FormatConfigXml();
            xcfg.Format = cfg.Format;
            xcfg.FormatConfiguration = cfg.Config.Capabilities;

            return xcfg;
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
    }
}
