using System;
using System.Runtime.InteropServices;

namespace nhltdecode
{
    public enum CONF_TYPE : byte
    {
        GENERIC = 0,
        MIC_ARRAY = 1,
        RENDER_WITH_LOOPBACK = 2, // not supported in Windows
        RENDER_FEEDBACK = 3, // in case of endpoint capture direction means feedback for render
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DeviceSpecificConfig
    {
        public byte VirtualSlot;
        public CONF_TYPE ConfigType;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MicArrayDeviceSpecificConfig
    {
        public DeviceSpecificConfig DeviceConfig;
        public byte ArrayTypeEx;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MicSnrSensitivityExtension
    {
        public int SNR;
        public int Sensitivity;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct VendorMicConfig
    {
        public byte Type;
        public byte Panel;
        public ushort SpeakerPositionDistance;
        public ushort HorizontalOffset;
        public ushort VerticalOffset;
        public byte FrequencyLowBand;
        public byte FrequencyHighBand;
        public short DirectionAngle;
        public short ElevationAngle;
        public short WorkVerticalAngleBegin;
        public short WorkVerticalAngleEnd;
        public short WorkHorizontalAngleBegin;
        public short WorkHorizontalAngleEnd;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RenderFeedbackConfig
    {
        public byte FeedbackVirtualSlot;
        public ushort FeedbackChannels;
        public ushort FeedbackValidBitsPerSample;
    }
}
