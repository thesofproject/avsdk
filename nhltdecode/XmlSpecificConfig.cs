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
}
