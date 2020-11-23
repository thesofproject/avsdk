using System.IO;
using System.Xml.Serialization;

namespace nhltdecode
{
    public struct MicArrayConfigXml
    {
        public byte ArrayTypeEx;
        public VendorMicConfig[] VendorMicConfig;
        public MicSnrSensitivityExtension? MicSnrSensitivityExtension;

        public bool ShouldSerializeMicSnrSensitivityExtension()
        {
            return MicSnrSensitivityExtension.HasValue;
        }

        public byte[] ToBytes()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            writer.Write(ArrayTypeEx);
            if (VendorMicConfig != null)
            {
                writer.Write((byte)VendorMicConfig.Length);
                foreach (var vendorMic in VendorMicConfig)
                    writer.Write(MarshalHelper.StructureToBytes(vendorMic));
            }

            if (MicSnrSensitivityExtension.HasValue)
                writer.Write(MarshalHelper.StructureToBytes(MicSnrSensitivityExtension.Value));

            var bytes = stream.ToArray();
            writer.Close();
            return bytes;
        }
    }

    public class EndpointSpecificConfigXml
    {
        public byte? VirtualSlot;
        public byte? ConfigType;
        public MicArrayConfigXml? MicArrayConfig;
        public RenderFeedbackConfig? RenderFeedbackConfig;
        [XmlElement(DataType = "hexBinary")]
        public byte[] Blob;

        public static EndpointSpecificConfigXml FromNative(SpecificConfig cfg)
        {
            var reader = new BinaryReader(new MemoryStream(cfg.Capabilities));
            var xcfg = new EndpointSpecificConfigXml();

            if (cfg.CapabilitiesSize == 0)
                goto exit;
            xcfg.VirtualSlot = reader.ReadByte();
            if (cfg.CapabilitiesSize == 1)
                goto exit;
            xcfg.ConfigType = reader.ReadByte();

            switch ((CONF_TYPE)xcfg.ConfigType)
            {
                case CONF_TYPE.GENERIC:
                case CONF_TYPE.RENDER_WITH_LOOPBACK:
                    xcfg.Blob = reader.ReadBytes((int)cfg.CapabilitiesSize - 2);
                    break;

                case CONF_TYPE.MIC_ARRAY:
                    var xmicCfg = new MicArrayConfigXml();

                    xmicCfg.ArrayTypeEx = reader.ReadByte();
                    int arrayExtension = ((xmicCfg.ArrayTypeEx & 0b11110000) >> 4);
                    int arrayType = (xmicCfg.ArrayTypeEx & 0b00001111);

                    if (arrayType == 0xF)
                    {
                        byte numMics = reader.ReadByte();
                        if (numMics > 0)
                        {
                            xmicCfg.VendorMicConfig = new VendorMicConfig[numMics];
                            for (int i = 0; i < numMics; i++)
                                xmicCfg.VendorMicConfig[i] =
                                    MarshalHelper.FromBinaryReader<VendorMicConfig>(reader);
                        }
                    }

                    if (arrayExtension == 0x1)
                        xmicCfg.MicSnrSensitivityExtension =
                            MarshalHelper.FromBinaryReader<MicSnrSensitivityExtension>(reader);

                    xcfg.MicArrayConfig = xmicCfg;
                    break;

                case CONF_TYPE.RENDER_FEEDBACK:
                    xcfg.RenderFeedbackConfig =
                            MarshalHelper.FromBinaryReader<RenderFeedbackConfig>(reader);
                    break;
            }

        exit:
            reader.Close();
            return xcfg;
        }

        public SpecificConfig ToNative()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            if (VirtualSlot.HasValue)
                writer.Write(VirtualSlot.Value);
            if (ConfigType.HasValue)
                writer.Write(ConfigType.Value);
            if (MicArrayConfig.HasValue)
                writer.Write(MicArrayConfig.Value.ToBytes());
            if (RenderFeedbackConfig.HasValue)
                writer.Write(MarshalHelper.StructureToBytes(RenderFeedbackConfig.Value));
            if (Blob != null)
                writer.Write(Blob);

            var cfg = new SpecificConfig();
            cfg.Capabilities = stream.ToArray();
            cfg.CapabilitiesSize = (uint)cfg.Capabilities.Length;

            writer.Close();
            return cfg;
        }

        public bool ShouldSerializeVirtualSlot()
        {
            return VirtualSlot.HasValue;
        }
        public bool ShouldSerializeConfigType()
        {
            return ConfigType.HasValue;
        }
        public bool ShouldSerializeMicArrayConfig()
        {
            return MicArrayConfig.HasValue;
        }
        public bool ShouldSerializeRenderFeedbackConfig()
        {
            return RenderFeedbackConfig.HasValue;
        }
    }

    public class FormatConfigurationXml
    {
        [XmlElement(DataType = "hexBinary")]
        public byte[] Blob;
        public uint? GatewayAttributes;
        public I2sConfigurationBlob? I2sBlob;
        public I2sConfigurationBlobLegacy? I2sBlobLegacy;
        public DmicConfigurationBlob? DmicBlob;

        public void ParseBlob(LINK_TYPE type)
        {
            if (Blob == null || Blob.Length == 0)
                return;

            switch (type)
            {
                case LINK_TYPE.SSP:
                case LINK_TYPE.PDM:
                    break;
                default:
                    return; // nothing to do for others
            }

            var reader = new BinaryReader(new MemoryStream(Blob));

            if (type == LINK_TYPE.SSP)
            {
                GatewayAttributes = reader.ReadUInt32();

                long pos = reader.BaseStream.Position;
                byte header = reader.ReadByte();
                reader.BaseStream.Position = pos; // mimics 'peek'

                if (header == 0xEE)
                {
                    var i2sBlob = new I2sConfigurationBlob();
                    i2sBlob.ReadFromBinary(reader);
                    I2sBlob = i2sBlob;
                }
                else
                {
                    I2sBlobLegacy = MarshalHelper.FromBinaryReader<I2sConfigurationBlobLegacy>(reader);
                }
            }
            else // LINK_TYPE.PDM
            {
                GatewayAttributes = reader.ReadUInt32();

                var dmicBlob = new DmicConfigurationBlob();
                dmicBlob.ReadFromBinary(reader);
                DmicBlob = dmicBlob;
            }

            reader.Close();
        }

        public static FormatConfigurationXml FromNative(SpecificConfig cfg)
        {
            return new FormatConfigurationXml() { Blob = cfg.Capabilities };
        }

        public SpecificConfig ToNative()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            if (Blob != null)
                writer.Write(Blob);
            if (GatewayAttributes.HasValue)
                writer.Write(GatewayAttributes.Value);
            if (I2sBlob.HasValue)
                I2sBlob.Value.WriteToBinary(writer);
            if (I2sBlobLegacy.HasValue)
                writer.Write(MarshalHelper.StructureToBytes(I2sBlobLegacy.Value));
            if (DmicBlob.HasValue)
                DmicBlob.Value.WriteToBinary(writer);

            var cfg = new SpecificConfig();
            cfg.Capabilities = stream.ToArray();
            cfg.CapabilitiesSize = (uint)cfg.Capabilities.Length;

            writer.Close();
            return cfg;
        }

        public bool ShouldSerializeBlob()
        {
            return !GatewayAttributes.HasValue;
        }
        public bool ShouldSerializeGatewayAttributes()
        {
            return GatewayAttributes.HasValue;
        }
        public bool ShouldSerializeI2sBlob()
        {
            return I2sBlob.HasValue;
        }
        public bool ShouldSerializeI2sBlobLegacy()
        {
            return I2sBlobLegacy.HasValue;
        }
        public bool ShouldSerializeDmicBlob()
        {
            return DmicBlob.HasValue;
        }
    }
}
