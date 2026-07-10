//
// Copyright (c) 2026, Intel Corporation. All rights reserved.
//
// Author: Cezary Rojewski <cezary.rojewski@intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System;
using System.Linq;

namespace nhltdecode.tests
{
    public static class Data
    {
        // Prepends Config.CapabilitiesSize before the cfg bytes.
        static byte[] ToConfigBytes(byte[] cfg)
        {
            return BitConverter.GetBytes(cfg.Length).Concat(cfg).ToArray();
        }

        // Prepends an array of objects with byte representing its Count.
        static byte[] ToCountedBytes(byte length, byte[] array)
        {
            return array.Prepend(length).ToArray();
        }

        // Prepends an array of objects with mask representing its Count (through PopCount).
        static byte[] ToMaskedBytes(uint mask, byte[] array)
        {
            return BitConverter.GetBytes(mask).Concat(array).ToArray();
        }

        public static byte[] Config => new byte[] { 0xAA, 0xBB, 0xCC };
        public static byte[] ConfigBytes => new byte[]
        {
            0x03, 0x00, 0x00, 0x00,             // CapabilitiesSize (3)
            0xAA, 0xBB, 0xCC,                   // Capabilities
        };

        public static DeviceConfig DeviceCfg => new DeviceConfig()
        {
            VirtualSlot = 0x1,
            ConfigType = 0x2,
            ArrayTypeEx = DeviceConfig.MICARRAY_TYPE_VENDOR,
            VendorMicsConfig = new Native.VendorMicConfig[]
            {
                new Native.VendorMicConfig(),
            }
        };
        public static byte[] DeviceCfgBytes => new byte[]
        {
            0x1A, 0x00, 0x00, 0x00,             // CapabilitiesSize (26)
            0x01,                               // VirtualSlot
            0x02,                               // ConfigType
            0x0F,                               // ArrayTypeEx
            0x01,                               // MicsCount
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Mics
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
        };
        public static byte[] DeviceCfg0bBytes => ToConfigBytes(Array.Empty<byte>());
        public static DeviceConfig DeviceCfg1b
        {
            get
            {
                var cfg = DeviceCfg;
                cfg.ConfigType = null;
                cfg.ArrayTypeEx = null;
                cfg.VendorMicsConfig = null;
                return cfg;
            }
        }
        public static byte[] DeviceCfg1bBytes
        {
            get
            {
                var bytes = DeviceCfgBytes.Take(5).ToArray();
                bytes[0] = 1;                   // CapabilitiesSize
                return bytes;
            }
        }
        public static DeviceConfig DeviceCfg2b
        {
            get
            {
                var cfg = DeviceCfg;
                cfg.ArrayTypeEx = null;
                cfg.VendorMicsConfig = null;
                return cfg;
            }
        }
        public static byte[] DeviceCfg2bBytes
        {
            get
            {
                var bytes = DeviceCfgBytes.Take(6).ToArray();
                bytes[0] = 2;                   // CapabilitiesSize
                return bytes;
            }
        }
        public static DeviceConfig DeviceCfg3b
        {
            get
            {
                var cfg = DeviceCfg;
                cfg.VendorMicsConfig = null;
                return cfg;
            }
        }
        public static byte[] DeviceCfg3bBytes
        {
            get
            {
                var bytes = DeviceCfgBytes.Take(7).ToArray();
                bytes[0] = 3;                   // CapabilitiesSize
                return bytes;
            }
        }

        public static I2SConfig I2SLegacy => new I2SConfig()
        {
            Version = 0,
            GatewayAttributes = 0x11223344,
            TdmTsGroup = new HexBLOB(new byte[] { 0x01, 0x02, 0x03, 0x04 }),
            Ssc0 = 0x0A,
            Ssc1 = 0x0B,
            Sscto = 0x0C,
            Sspsp = 0x0D,
            Sstsa = new HexUInt32[] { 0x0E },
            Ssrsa = new HexUInt32[] { 0x0F },
            Ssc2 = 0x10,
            Sspsp2 = 0x11,
            Ssc3 = 0x12,
            Ssioc = 0x13,
            MdivCtrl = 0x14,
            MdivR = new HexUInt32[] { 0x15 },
        };
        public static byte[] I2SLegacyBytes => new byte[]
        {
            0x44, 0x33, 0x22, 0x11,             // GatewayAttributes
            0x01, 0x02, 0x03, 0x04, 0x00, 0x00, 0x00, 0x00, // TdmTsGroup (32 bytes)
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x0A, 0x00, 0x00, 0x00,             // SSPConfig.Ssc0
            0x0B, 0x00, 0x00, 0x00,             // Ssc1
            0x0C, 0x00, 0x00, 0x00,             // Sscto
            0x0D, 0x00, 0x00, 0x00,             // Sspsp
            0x0E, 0x00, 0x00, 0x00,             // Sstsa
            0x0F, 0x00, 0x00, 0x00,             // Ssrsa
            0x10, 0x00, 0x00, 0x00,             // Ssc2
            0x11, 0x00, 0x00, 0x00,             // Sspsp2
            0x12, 0x00, 0x00, 0x00,             // Ssc3
            0x13, 0x00, 0x00, 0x00,             // Ssioc
            0x14, 0x00, 0x00, 0x00,             // MclkConfig.MdivCtrl
            0x15, 0x00, 0x00, 0x00,             // MclkConfig.MdivR
        };
        public static byte[] I2SLegacyConfigBytes => ToConfigBytes(I2SLegacyBytes);

        public static I2SConfig I2S15 => new I2SConfig()
        {
            Version = I2SConfig.VERSION1_5,
            GatewayAttributes = 0x11223344,
            TdmTsGroup = new HexBLOB(new byte[] { 0x01, 0x02, 0x03, 0x04 }),
            Ssc0 = 0x0A,
            Ssc1 = 0x0B,
            Sscto = 0x0C,
            Sspsp = 0x0D,
            Sstsa = new HexUInt32[] { 0x0E },
            Ssrsa = new HexUInt32[] { 0x0F },
            Ssc2 = 0x10,
            Sspsp2 = 0x11,
            Ssc3 = 0x12,
            Ssioc = 0x13,
            MdivCtrl = 0x14,
            MdivR = new HexUInt32[] { 0x15 },
        };
        public static byte[] I2S15Bytes => new byte[]
        {
            0x44, 0x33, 0x22, 0x11,             // GatewayAttributes
            0x05, 0x01, 0x00, 0xEE,             // Header: VersionMinor, VersionMajor, Reserved, Signature
            0x5C, 0x00, 0x00, 0x00,             // Header.SizeBytes (92)
            0x01, 0x02, 0x03, 0x04, 0x00, 0x00, 0x00, 0x00, // TdmTsGroup (32 bytes)
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x0A, 0x00, 0x00, 0x00,             // SSPConfig.Ssc0
            0x0B, 0x00, 0x00, 0x00,             // Ssc1
            0x0C, 0x00, 0x00, 0x00,             // Sscto
            0x0D, 0x00, 0x00, 0x00,             // Sspsp
            0x0E, 0x00, 0x00, 0x00,             // Sstsa
            0x0F, 0x00, 0x00, 0x00,             // Ssrsa
            0x10, 0x00, 0x00, 0x00,             // Ssc2
            0x11, 0x00, 0x00, 0x00,             // Sspsp2
            0x12, 0x00, 0x00, 0x00,             // Ssc3
            0x13, 0x00, 0x00, 0x00,             // Ssioc
            0x14, 0x00, 0x00, 0x00,             // MclkConfig15.MdivCtrl
            0x01, 0x00, 0x00, 0x00,             // MclkConfig15.MdivRCount
            0x15, 0x00, 0x00, 0x00,             // MdivR[0]
        };
        public static byte[] I2S15ConfigBytes => ToConfigBytes(I2S15Bytes);

        public static I2SConfig I2S2 => new I2SConfig()
        {
            Version = I2SConfig.VERSION2_0,
            GatewayAttributes = 0x11223344,
            TdmTsGroup = new HexBLOB(new byte[] { 0x01, 0x02, 0x03, 0x04 }),
            Ssc0 = 0x0A,
            Ssc1 = 0x0B,
            Sscto = 0x0C,
            Sspsp = 0x0D,
            Sstsa = new HexUInt32[] { 0x0E },
            Ssrsa = new HexUInt32[] { 0x0F },
            Ssc2 = 0x10,
            Sspsp2 = 0x11,
            Ssc3 = 0x12,
            Ssioc = 0x13,
            MdivCtrl = 0x14,
            MdivR = new HexUInt32[] { 0x15 },
        };
        public static byte[] I2S2Bytes => new byte[]
        {
            0x44, 0x33, 0x22, 0x11,             // GatewayAttributes
            0x00, 0x02, 0x00, 0xEE,             // Header: VersionMinor, VersionMajor, Reserved, Signature
            0x38, 0x01, 0x00, 0x00,             // Header.SizeBytes (312)
            0x01, 0x02, 0x03, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // TdmTsGroup (256 bytes)
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x0A, 0x00, 0x00, 0x00,             // SSPConfig.Ssc0
            0x0B, 0x00, 0x00, 0x00,             // Ssc1
            0x0C, 0x00, 0x00, 0x00,             // Sscto
            0x0D, 0x00, 0x00, 0x00,             // Sspsp
            0x0E, 0x00, 0x00, 0x00,             // Sstsa
            0x0F, 0x00, 0x00, 0x00,             // Ssrsa
            0x10, 0x00, 0x00, 0x00,             // Ssc2
            0x11, 0x00, 0x00, 0x00,             // Sspsp2
            0x12, 0x00, 0x00, 0x00,             // Ssc3
            0x13, 0x00, 0x00, 0x00,             // Ssioc
            0x14, 0x00, 0x00, 0x00,             // MclkConfig.MdivCtrl
            0x15, 0x00, 0x00, 0x00,             // MclkConfig.MdivR
        };
        public static byte[] I2S2ConfigBytes => ToConfigBytes(I2S2Bytes);

        public static I2SConfig I2S3 => new I2SConfig()
        {
            Version = I2SConfig.VERSION3_0,
            GatewayAttributes = 0x11223344,
            TdmTsGroup = new HexBLOB(new byte[] { 0x01, 0x02, 0x03, 0x04 }),
            Ssc0 = 0x0A,
            Ssc1 = 0x0B,
            Sscto = 0x0C,
            Sspsp = 0x0D,
            Ssc2 = 0x10,
            Sspsp2 = 0x11,
            Ssc3 = 0x12,
            Ssioc = 0x13,
            Ssmidytsa = new HexUInt64[8],
            Ssmodytsa = new HexUInt64[8],
            MdivCtrl = 0x14,
            MdivR = new HexUInt32[] { 0x15 },
        };
        public static byte[] I2S3Bytes => new byte[]
        {
            0x44, 0x33, 0x22, 0x11,             // GatewayAttributes
            0x00, 0x03, 0x00, 0xEE,             // Header: VersionMinor, VersionMajor, Reserved, Signature
            0xD4, 0x00, 0x00, 0x00,             // Header.SizeBytes (212)
            0x01, 0x02, 0x03, 0x04, 0x00, 0x00, 0x00, 0x00, // TdmTsGroup (32 bytes)
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x0A, 0x00, 0x00, 0x00,             // SSPConfig3.Ssc0
            0x0B, 0x00, 0x00, 0x00,             // Ssc1
            0x0C, 0x00, 0x00, 0x00,             // Sscto
            0x0D, 0x00, 0x00, 0x00,             // Sspsp
            0x10, 0x00, 0x00, 0x00,             // Ssc2
            0x11, 0x00, 0x00, 0x00,             // Sspsp2
            0x12, 0x00, 0x00, 0x00,             // Ssc3
            0x13, 0x00, 0x00, 0x00,             // Ssioc
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Ssmidytsa (8 x ulong)
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Ssmodytsa (8 x ulong)
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x14, 0x00, 0x00, 0x00,             // MclkConfig15.MdivCtrl
            0x01, 0x00, 0x00, 0x00,             // MclkConfig15.MdivRCount
            0x15, 0x00, 0x00, 0x00,             // MdivR[0]
        };
        public static byte[] I2S3ConfigBytes => ToConfigBytes(I2S3Bytes);

        public static I2SConfig I2SMclk
        {
            get
            {
                var cfg = I2SLegacy;
                cfg.MdivR = null;
                return cfg;
            }
        }
        public static byte[] I2SMclkBytes
        {
            get
            {
                byte[] bytes = I2SLegacyBytes;
                bytes[80] = 0x0; // MdivR
                return bytes;
            }
        }

        public static I2SConfig I2SMclk15
        {
            get
            {
                var cfg = I2S15;
                cfg.MdivR = null;
                return cfg;
            }
        }
        public static byte[] I2SMclk15Bytes
        {
            get
            {
                byte[] bytes = I2S15Bytes.SkipLast(4).ToArray();
                bytes[8] = 0x58; // SizeBytes
                bytes[88] = 0x0; // MdivRCount
                return bytes;
            }
        }
        public static byte[] I2SMclk15ConfigBytes => ToConfigBytes(I2SMclk15Bytes);

        public static I2SConfig I2SDMA
        {
            get
            {
                var cfg = I2SLegacy;
                cfg.DmaControls = new byte[4];
                return cfg;
            }
        }
        public static byte[] I2SDMABytes
        {
            get
            {
                return I2SLegacyBytes.Concat(new byte[4]).ToArray();
            }
        }
        public static byte[] I2SDMAConfigBytes => ToConfigBytes(I2SDMABytes);

        public static FirFilter Filter => new FirFilter()
        {
            FirControl = 0x1,
            FirConfig = 0x2,
            DcOffsetLeft = 0x3,
            DcOffsetRight = 0x4,
            OutGainLeft = 0x5,
            OutGainRight = 0x6,
        };
        public static byte[] FilterBytes => new byte[]
        {
            0x01, 0x00, 0x00, 0x00,             // FirControl
            0x02, 0x00, 0x00, 0x00,             // FirConfig
            0x03, 0x00, 0x00, 0x00,             // DcOffsetLeft
            0x04, 0x00, 0x00, 0x00,             // DcOffsetRight
            0x05, 0x00, 0x00, 0x00,             // OutGainLeft
            0x06, 0x00, 0x00, 0x00,             // OutGainRight
            0x00, 0x00, 0x00, 0x00,             // Rsvd[0]
            0x00, 0x00, 0x00, 0x00,             // Rsvd[1]
        };

        public static PDMCtrlConfig PDM => new PDMCtrlConfig()
        {
            Id = 1,
            CicControl = 0x10,
            CicConfig = 0x20,
            MicControl = 0x30,
            PdmSdwMap = 0x40,
            ReuseFirFromPdm = 0x50,
            FirA = new FirFilter()
            {
                FirControl = 0x1,
                DcOffsetLeft = 0x2,
                DcOffsetRight = 0x3,
                OutGainLeft = 0x4,
                OutGainRight = 0x5,
            },
            FirB = new FirFilter()
            {
                FirControl = 0x6,
                DcOffsetLeft = 0x7,
                DcOffsetRight = 0x8,
                OutGainLeft = 0x9,
                OutGainRight = 0xA,
            },
            FirCoeffs = new HexBLOB(new byte[] { 0xAA, 0xBB, 0xCC, 0xDD }),
        };
        public static byte[] PDMBytes => new byte[]
        {
            0x10, 0x00, 0x00, 0x00,             // CicControl
            0x20, 0x00, 0x00, 0x00,             // CicConfig
            0x00, 0x00, 0x00, 0x00,             // Rsvd0
            0x30, 0x00, 0x00, 0x00,             // MicControl
            0x40, 0x00, 0x00, 0x00,             // PdmSdwMap
            0x50, 0x00, 0x00, 0x00,             // ReuseFirFromPdm
            0x00, 0x00, 0x00, 0x00,             // Rsvd1[0]
            0x00, 0x00, 0x00, 0x00,             // Rsvd1[1]
            0x01, 0x00, 0x00, 0x00,             // FirA.FirControl
            0x00, 0x00, 0x00, 0x00,             // FirA.FirConfig
            0x02, 0x00, 0x00, 0x00,             // FirA.DcOffsetLeft
            0x03, 0x00, 0x00, 0x00,             // FirA.DcOffsetRight
            0x04, 0x00, 0x00, 0x00,             // FirA.OutGainLeft
            0x05, 0x00, 0x00, 0x00,             // FirA.OutGainRight
            0x00, 0x00, 0x00, 0x00,             // FirA.Rsvd[0]
            0x00, 0x00, 0x00, 0x00,             // FirA.Rsvd[1]
            0x06, 0x00, 0x00, 0x00,             // FirB.FirControl
            0x00, 0x00, 0x00, 0x00,             // FirB.FirConfig
            0x07, 0x00, 0x00, 0x00,             // FirB.DcOffsetLeft
            0x08, 0x00, 0x00, 0x00,             // FirB.DcOffsetRight
            0x09, 0x00, 0x00, 0x00,             // FirB.OutGainLeft
            0x0A, 0x00, 0x00, 0x00,             // FirB.OutGainRight
            0x00, 0x00, 0x00, 0x00,             // FirB.Rsvd[0]
            0x00, 0x00, 0x00, 0x00,             // FirB.Rsvd[1]
            0xAA, 0xBB, 0xCC, 0xDD,             // FirCoeffs
        };
        public static byte[] PDMPackedBytes
        {
            get
            {
                // For testing the FIR_COEFFS_PACKED_TO_24_BITS branch.
                byte[] packedFirCoeffs = { 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

                return PDMBytes.Take(96).Concat(packedFirCoeffs).ToArray();
            }
        }
        public static byte[] PDMMaskedBytes => ToMaskedBytes(0x02, PDMBytes);

        public static ChannelConfig Channel => new ChannelConfig()
        {
            Id = 1,
            OutControl = 0xDEADBEEF
        };
        public static byte[] ChannelBytes => new byte[]
        {
            0xEF, 0xBE, 0xAD, 0xDE,             // OutControl
        };
        public static byte[] ChannelMaskedBytes => ToMaskedBytes(0x02, ChannelBytes);

        public static DMICConfig DMIC => new DMICConfig()
        {
            GatewayAttributes = 0xAABBCCDD,
            TsGroup = new HexBLOB(new byte[] { 0x40, 0x41, 0x42, 0x43 }),
            GlobalConfig = 0x12345678,
            ChannelsConfig = new ChannelConfig[] { new ChannelConfig() },
            PDMCtrlsConfig = new PDMCtrlConfig[0],
        };
        public static byte[] DMICBytes => new byte[]
        {
            0xDD, 0xCC, 0xBB, 0xAA,             // GatewayAttributes
            0x40, 0x41, 0x42, 0x43, 0x00, 0x00, 0x00, 0x00, // TsGroup (16 bytes)
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x78, 0x56, 0x34, 0x12,             // GlobalConfig
            0x01, 0x00, 0x00, 0x00,             // ChannelCtrlsMask
            0x00, 0x00, 0x00, 0x00,             // ChannelsConfig[0].OutControl
            0x00, 0x00, 0x00, 0x00,             // PDMCtrlsMask
        };
        public static byte[] DMICConfigBytes => ToConfigBytes(DMICBytes);

        public static FormatConfig Format => new FormatConfig()
        {
            Channels = 2,
            SamplesPerSec = 48000,
            BitsPerSample = 32,
            ValidBitsPerSample = 24,
            ChannelMask = 0x3,
            Subformat = Guid.Empty,
        };
        public static byte[] FormatBytes => new byte[]
        {
            0xFE, 0xFF,                         // FormatTag (WAVE_FORMAT_EXTENSIBLE)
            0x02, 0x00,                         // Channels
            0x80, 0xBB, 0x00, 0x00,             // SamplesPerSec
            0x00, 0xDC, 0x05, 0x00,             // AvgBytesPerSec (384000)
            0x08, 0x00,                         // BlockAlign (8)
            0x20, 0x00,                         // BitsPerSample
            0x16, 0x00,                         // Size (22)
            0x18, 0x00,                         // ValidBitsPerSample
            0x03, 0x00, 0x00, 0x00,             // ChannelMask
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Subformat
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,             // Config.CapabilitiesSize
        };

        public static FormatConfig[] Formats => new FormatConfig[] { Format };
        public static byte[] FormatsBytes = ToCountedBytes(0x01, FormatBytes); // FormatsCount

        public static FormatConfig I2SFormat
        {
            get
            {
                FormatConfig fmt = Format;
                fmt.I2SConfig = I2SLegacy;
                return fmt;
            }
        }
        public static byte[] I2SFormatBytes
        {
            get
            {
                byte[] bytes = FormatBytes.Concat(I2SLegacyBytes).ToArray();
                bytes[40] = 0x54;               // Config.CapabilitiesSize
                return bytes;
            }
        }
        public static byte[] I2SFormatsBytes => ToCountedBytes(0x01, I2SFormatBytes);

        public static FormatConfig DMICFormat
        {
            get
            {
                FormatConfig fmt = Format;
                fmt.DMICConfig = DMIC;
                return fmt;
            }
        }
        public static byte[] DMICFormatBytes
        {
            get
            {
                byte[] bytes = FormatBytes.Concat(DMICBytes).ToArray();
                bytes[40] = 0x24;               // Config.CapabilitiesSize
                return bytes;
            }
        }
        public static byte[] DMICFormatsBytes => ToCountedBytes(0x01, DMICFormatBytes);

        public static Native.DeviceInfo[] Devices => new Native.DeviceInfo[]
        {
            new Native.DeviceInfo()
            {
                Id = "DEV0",
                InstanceId = 1,
                PortId = 2,
            }
        };
        public static byte[] DevicesBytes => new byte[]
        {
            0x01,                               // DevicesCount
            0x44, 0x00, 0x45, 0x00, 0x56, 0x00, 0x30, 0x00, // Id "DEV0" (Unicode, 8 chars)
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x01,                               // InstanceId
            0x02,                               // PortId
        };

        public static Endpoint Endpoint => new Endpoint()
        {
            LinkType = Native.LINKTYPE.SSP,
            InstanceId = 1,
            VendorId = 0x8086,
            DeviceId = 0xAE34,
            RevisionId = 1,
            SubsystemId = 0xDEADBEEF,
            DeviceType = 4,
            Direction = 1,
            VirtualBusId = 0,
            DeviceConfig = new DeviceConfig(),
            FormatsConfig = new FormatConfig[0],
            DevicesInfo = null,
        };
        public static byte[] EndpointBytes => new byte[]
        {
            0x18, 0x00, 0x00, 0x00,             // Length (24)
            0x03,                               // LinkType (SSP)
            0x01,                               // InstanceId
            0x86, 0x80,                         // VendorId
            0x34, 0xAE,                         // DeviceId
            0x01, 0x00,                         // RevisionId
            0xEF, 0xBE, 0xAD, 0xDE,             // SubsystemId
            0x04,                               // DeviceType
            0x01,                               // Direction
            0x00,                               // VirtualBusId
            0x00, 0x00, 0x00, 0x00,             // Config.CapabilitiesSize (DeviceConfig)
            0x00,                               // FormatsConfig.FormatsCount
        };

        public static Endpoint[] Endpoints => new Endpoint[] { Endpoint };
        public static byte[] EndpointsBytes => ToCountedBytes(0x01, EndpointBytes); // NHLT.EndpointsCount

        public static byte[] EndpointPDMBytes
        {
            get
            {
                byte[] bytes = EndpointBytes;
                bytes[4] = Native.LINKTYPE.PDM;
                return bytes;
            }
        }
        public static byte[] EndpointHDABytes
        {
            get
            {
                byte[] bytes = EndpointBytes;
                bytes[4] = Native.LINKTYPE.HDA;
                return bytes;
            }
        }

        public static Endpoint EndpointDevices
        {
            get
            {
                Endpoint ep = Endpoint;
                ep.DevicesInfo = new Native.DeviceInfo[0];
                return ep;
            }
        }
        public static byte[] EndpointDevicesBytes
        {
            get
            {
                byte[] bytes = EndpointBytes.Append((byte)0x00).ToArray(); // DevicesCount
                bytes[0] = 25;                  // Length (25)
                return bytes;
            }
        }

        public static HexBLOB OED => new HexBLOB(new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });
        public static byte[] OEDBytes => new byte[]
        {
            0x04, 0x00, 0x00, 0x00,             // CapabilitiesSize (4)
            0xDE, 0xAD, 0xBE, 0xEF,             // Capabilities
        };

        public static NHLT NHLT => new NHLT()
        {
            Revision = 5,
            OemId = "INTEL",
            OemTableId = "TGL",
            OemRevision = 1,
            AslCompilerId = null,
            AslCompilerRevision = 2,
            Endpoints = new Endpoint[0],
        };
        public static byte[] NHLTBytes => new byte[]
        {
            0x4E, 0x48, 0x4C, 0x54,             // Signature "NHLT"
            0x25, 0x00, 0x00, 0x00,             // Length (37)
            0x05,                               // Revision
            0x3A,                               // Checksum (58)
            0x49, 0x4E, 0x54, 0x45, 0x4C, 0x00, // OemId
            0x54, 0x47, 0x4C, 0x00, 0x00, 0x00, 0x00, 0x00, // OemTableId
            0x01, 0x00, 0x00, 0x00,             // OemRevision
            0x00, 0x00, 0x00, 0x00,             // AslCompilerId
            0x02, 0x00, 0x00, 0x00,             // AslCompilerRevision
            0x00,                               // EndpointsCount
        };
    }
}
