using System.IO;
using System.Runtime.InteropServices;
using System.Xml.Serialization;

namespace nhltdecode
{
    public interface IBinaryConvertible<T>
    {
        int SizeOf();
        void WriteToBinary(BinaryWriter writer);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AcpiDescriptionHeader
    {
        [XmlElement(DataType = "hexBinary")]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] Signature;
        [XmlIgnore]
        public uint Length;
        public byte Revision;
        [XmlIgnore]
        public byte Checksum;
        [XmlElement(DataType = "hexBinary")]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] OemId;
        [XmlElement(DataType = "hexBinary")]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] OemIdTableId;
        public uint OemRevision;
        [XmlElement(DataType = "hexBinary")]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] CreatorId; // AslCompilerId
        public uint CreatorRevision; // AslCompilerRevision
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WaveFormatExtensible
    {
        [XmlIgnore]
        public ushort FormatTag;
        public ushort Channels;
        public uint SamplesPerSec;
        [XmlIgnore]
        public uint AvgBytesPerSec;
        [XmlIgnore]
        public ushort BlockAlign;
        public ushort BitsPerSample;
        [XmlIgnore]
        public ushort Size;
        public ushort ValidBitsPerSample;
        [XmlElement(DataType = "hexBinary")]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] ChannelMask;
        [XmlElement(DataType = "hexBinary")]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] Subformat;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FormatConfig : IBinaryConvertible<SpecificConfig>
    {
        public WaveFormatExtensible Format;
        public SpecificConfig Config;

        public static FormatConfig ReadFromBinary(BinaryReader reader)
        {
            var cfg = new FormatConfig();

            cfg.Format = MarshalHelper.FromBinaryReader<WaveFormatExtensible>(reader);
            cfg.Config = SpecificConfig.ReadFromBinary(reader);

            return cfg;
        }

        public int SizeOf()
        {
            return Marshal.SizeOf(Format) + Config.SizeOf();
        }

        public void WriteToBinary(BinaryWriter writer)
        {
            writer.Write(MarshalHelper.StructureToBytes(Format));
            Config.WriteToBinary(writer);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FormatsConfig : IBinaryConvertible<FormatsConfig>
    {
        public byte FormatsCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)] // fake size
        public FormatConfig[] FormatsConfiguration;

        public static FormatsConfig ReadFromBinary(BinaryReader reader)
        {
            var cfg = new FormatsConfig();

            cfg.FormatsCount = reader.ReadByte();
            cfg.FormatsConfiguration = new FormatConfig[cfg.FormatsCount];
            for (int i = 0; i < cfg.FormatsCount; i++)
                cfg.FormatsConfiguration[i] = FormatConfig.ReadFromBinary(reader);

            return cfg;
        }

        public int SizeOf()
        {
            int cfgSize = Marshal.SizeOf(typeof(FormatConfig));
            int size = Marshal.SizeOf(this);

            size -= cfgSize; // fake size
            if (FormatsConfiguration != null)
                foreach (var cfg in FormatsConfiguration)
                    size += cfg.SizeOf();
            return size;
        }

        public void WriteToBinary(BinaryWriter writer)
        {
            writer.Write(FormatsCount);
            if (FormatsConfiguration != null)
                foreach (var cfg in FormatsConfiguration)
                    cfg.WriteToBinary(writer);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SpecificConfig : IBinaryConvertible<SpecificConfig>
    {
        public uint CapabilitiesSize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)] // fake size
        public byte[] Capabilities;

        public static SpecificConfig ReadFromBinary(BinaryReader reader)
        {
            var cfg = new SpecificConfig();

            cfg.CapabilitiesSize = reader.ReadUInt32();
            cfg.Capabilities = reader.ReadBytes((int)cfg.CapabilitiesSize);

            return cfg;
        }

        public int SizeOf()
        {
            int byteSize = Marshal.SizeOf(typeof(byte));
            int size = Marshal.SizeOf(this);

            size -= byteSize; // fake size
            if (Capabilities != null)
                size += byteSize * Capabilities.Length;
            return size;
        }

        public void WriteToBinary(BinaryWriter writer)
        {
            writer.Write(CapabilitiesSize);
            writer.Write(Capabilities);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DeviceInfo
    {
        [XmlElement(DataType = "hexBinary")]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] DeviceId;
        public byte DeviceInstanceId;
        public byte DevicePortId;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DevicesInfo : IBinaryConvertible<DeviceInfo>
    {
        public byte Count;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)] // fake size
        public DeviceInfo[] Devices;

        public static DevicesInfo ReadFromBinary(BinaryReader reader)
        {
            var info = new DevicesInfo();

            info.Count = reader.ReadByte();
            info.Devices = new DeviceInfo[info.Count];
            for (int i = 0; i < info.Count; i++)
                info.Devices[i] = MarshalHelper.FromBinaryReader<DeviceInfo>(reader);

            return info;
        }

        public int SizeOf()
        {
            int devSize = Marshal.SizeOf(typeof(DeviceInfo));
            int size = Marshal.SizeOf(this);

            size -= devSize; // fake size
            if (Devices != null)
                size += devSize * Devices.Length;
            return size;
        }

        public void WriteToBinary(BinaryWriter writer)
        {
            writer.Write(Count);
            if (Devices != null)
                foreach (var dev in Devices)
                    writer.Write(MarshalHelper.StructureToBytes(dev));
        }
    }

    public enum LINK_TYPE : byte
    {
        HDA = 0,
        DSP = 1,
        PDM = 2,
        SSP = 3,
        SLIMBUS = 4,
        SOUNDWIRE = 5,
        RESERVED1 = 6,
        RESERVED2 = 7
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EndpointDescriptor : IBinaryConvertible<EndpointDescriptor>
    {
        public uint EndpointDescriptorLength;
        public LINK_TYPE LinkType; // 4 bits only
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public byte[] InstanceId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] VendorId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] DeviceId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] RevisionId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] SubsystemId;
        public byte DeviceType;
        public byte Direction;
        public byte VirtualBusId;
        public SpecificConfig EndpointConfig;
        public FormatsConfig FormatsConfig;
        public DevicesInfo DevicesInfo;

        public static EndpointDescriptor ReadFromBinary(BinaryReader reader)
        {
            long pos = reader.BaseStream.Position;
            var desc = new EndpointDescriptor();

            desc.EndpointDescriptorLength = reader.ReadUInt32();
            desc.LinkType = (LINK_TYPE)reader.ReadByte();
            desc.InstanceId = reader.ReadBytes(1);
            desc.VendorId = reader.ReadBytes(2);
            desc.DeviceId = reader.ReadBytes(2);
            desc.RevisionId = reader.ReadBytes(2);
            desc.SubsystemId = reader.ReadBytes(4);
            desc.DeviceType = reader.ReadByte();
            desc.Direction = reader.ReadByte();
            desc.VirtualBusId = reader.ReadByte();
            desc.EndpointConfig = SpecificConfig.ReadFromBinary(reader);
            desc.FormatsConfig = FormatsConfig.ReadFromBinary(reader);

            // Check if there is DevicesInfo in EndpointDescriptor
            if (desc.EndpointDescriptorLength > (reader.BaseStream.Position - pos))
                desc.DevicesInfo = DevicesInfo.ReadFromBinary(reader);

            return desc;
        }

        public int SizeOf()
        {
            int size = Marshal.SizeOf(this);

            // fix EndpointConfig size
            size -= Marshal.SizeOf(EndpointConfig);
            size += EndpointConfig.SizeOf();
            // fix FormatsConfig size
            size -= Marshal.SizeOf(FormatsConfig);
            size += FormatsConfig.SizeOf();
            // fix DevicesInfo size
            size -= Marshal.SizeOf(DevicesInfo);
            size += DevicesInfo.SizeOf();

            return size;
        }

        public void WriteToBinary(BinaryWriter writer)
        {
            writer.Write(EndpointDescriptorLength);
            writer.Write((byte)LinkType);
            writer.Write(InstanceId);
            writer.Write(VendorId);
            writer.Write(DeviceId);
            writer.Write(RevisionId);
            writer.Write(SubsystemId);
            writer.Write(DeviceType);
            writer.Write(Direction);
            writer.Write(VirtualBusId);

            EndpointConfig.WriteToBinary(writer);
            FormatsConfig.WriteToBinary(writer);
            DevicesInfo.WriteToBinary(writer);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NHLT : IBinaryConvertible<NHLT>
    {
        public AcpiDescriptionHeader Header;
        public byte DescriptorCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)] // fake size
        public EndpointDescriptor[] Descriptors;
        public SpecificConfig OEDConfig;

        public static NHLT ReadFromBinary(BinaryReader reader)
        {
            var table = new NHLT();

            table.Header = MarshalHelper.FromBinaryReader<AcpiDescriptionHeader>(reader);
            table.DescriptorCount = reader.ReadByte();
            table.Descriptors = new EndpointDescriptor[table.DescriptorCount];
            for (int i = 0; i < table.DescriptorCount; i++)
            {
                long pos = reader.BaseStream.Position;

                table.Descriptors[i] = EndpointDescriptor.ReadFromBinary(reader);
                // sometimes there are redundant bytes at the end of descriptor, account for them
                reader.BaseStream.Position = pos + table.Descriptors[i].EndpointDescriptorLength;
            }

            table.OEDConfig = SpecificConfig.ReadFromBinary(reader);

            return table;
        }

        public int SizeOf()
        {
            int descSize = Marshal.SizeOf(typeof(EndpointDescriptor));
            int size = Marshal.SizeOf(this);

            size -= descSize; // fake size
            foreach (var descriptor in Descriptors)
                size += descriptor.SizeOf();
            // fix OEDConfig size
            size -= Marshal.SizeOf(OEDConfig);
            size += OEDConfig.SizeOf();

            return size;
        }

        public void WriteToBinary(BinaryWriter writer)
        {
            writer.Write(MarshalHelper.StructureToBytes(Header));
            writer.Write(DescriptorCount);
            if (Descriptors != null)
                foreach (var desc in Descriptors)
                    desc.WriteToBinary(writer);
            OEDConfig.WriteToBinary(writer);
        }
    }
}
