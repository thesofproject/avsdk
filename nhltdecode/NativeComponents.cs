using System.IO;
using System.Runtime.InteropServices;
using System.Xml.Serialization;

namespace nhltdecode
{
    public interface IBinaryConvertible<T>
    {
        int SizeOf();
        void ReadFromBinary(BinaryReader reader);
        void WriteToBinary(BinaryWriter writer);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AcpiDescriptionHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] Signature;
        public uint Length;
        public byte Revision;
        public byte Checksum;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] OemId;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] OemIdTableId;
        public uint OemRevision;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] CreatorId; // AslCompilerId
        public uint CreatorRevision; // AslCompilerRevision
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WaveFormatExtensible
    {
        [XmlElement(DataType = "hexBinary")]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] FormatTag;
        public ushort Channels;
        public uint SamplesPerSec;
        public uint AvgBytesPerSec;
        public ushort BlockAlign;
        public ushort BitsPerSample;
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

        public void ReadFromBinary(BinaryReader reader)
        {
            Format = MarshalHelper.FromBinaryReader<WaveFormatExtensible>(reader);
            Config.ReadFromBinary(reader);
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

        public void ReadFromBinary(BinaryReader reader)
        {
            FormatsCount = reader.ReadByte();
            FormatsConfiguration = new FormatConfig[FormatsCount];
            for (int i = 0; i < FormatsCount; i++)
                FormatsConfiguration[i].ReadFromBinary(reader);
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

        public void ReadFromBinary(BinaryReader reader)
        {
            CapabilitiesSize = reader.ReadUInt32();
            Capabilities = reader.ReadBytes((int)CapabilitiesSize);
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

        public void ReadFromBinary(BinaryReader reader)
        {
            Count = reader.ReadByte();
            Devices = new DeviceInfo[Count];
            for (int i = 0; i < Count; i++)
                Devices[i] = MarshalHelper.FromBinaryReader<DeviceInfo>(reader);
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

        public void ReadFromBinary(BinaryReader reader)
        {
            long startPos = reader.BaseStream.Position;

            EndpointDescriptorLength = reader.ReadUInt32();
            LinkType = (LINK_TYPE)reader.ReadByte();
            InstanceId = reader.ReadBytes(1);
            VendorId = reader.ReadBytes(2);
            DeviceId = reader.ReadBytes(2);
            RevisionId = reader.ReadBytes(2);
            SubsystemId = reader.ReadBytes(4);
            DeviceType = reader.ReadByte();
            Direction = reader.ReadByte();
            VirtualBusId = reader.ReadByte();
            EndpointConfig.ReadFromBinary(reader);
            FormatsConfig.ReadFromBinary(reader);

            // Check if there is DevicesInfo in EndpointDescriptor
            if (EndpointDescriptorLength > (reader.BaseStream.Position - startPos))
                DevicesInfo.ReadFromBinary(reader);
            // Some NHLTs have redundant bytes at the end of EndpointDescriptor,
            // they should be ommited
            reader.ReadBytes((int)(EndpointDescriptorLength - (reader.BaseStream.Position - startPos)));
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

        public void ReadFromBinary(BinaryReader reader)
        {
            Header = MarshalHelper.FromBinaryReader<AcpiDescriptionHeader>(reader);
            DescriptorCount = reader.ReadByte();
            Descriptors = new EndpointDescriptor[DescriptorCount];
            for (int i = 0; i < DescriptorCount; i++)
                Descriptors[i].ReadFromBinary(reader);
            OEDConfig.ReadFromBinary(reader);
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
