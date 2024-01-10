//
// Copyright (c) 2023, Intel Corporation. All rights reserved.
//
// Authors: Cezary Rojewski <cezary.rojewski@intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace nhltdecode
{
    public struct HexUInt8 : IXmlSerializable
    {
        byte value;

        public HexUInt8(byte v)
        {
            value = v;
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            value = reader.ReadElementContentAsString().ToUInt8();
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteValue(ToString());
        }

        public static implicit operator byte(HexUInt8 h)
        {
            return h.value;
        }

        public static implicit operator HexUInt8(byte v)
        {
            return new HexUInt8(v);
        }

        public override string ToString()
        {
            return string.Format("0x{0:X2}", value);
        }
    }

    public struct HexUInt16 : IXmlSerializable
    {
        ushort value;

        public HexUInt16(ushort v)
        {
            value = v;
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            value = reader.ReadElementContentAsString().ToUInt16();
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteValue(ToString());
        }

        public static implicit operator ushort(HexUInt16 h)
        {
            return h.value;
        }

        public static implicit operator HexUInt16(ushort v)
        {
            return new HexUInt16(v);
        }

        public override string ToString()
        {
            return string.Format("0x{0:X4}", value);
        }
    }

    public struct HexUInt32 : IXmlSerializable
    {
        uint value;

        public HexUInt32(uint v)
        {
            value = v;
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            value = reader.ReadElementContentAsString().ToUInt32();
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteValue(ToString());
        }

        public static implicit operator uint(HexUInt32 h)
        {
            return h.value;
        }

        public static implicit operator HexUInt32(uint v)
        {
            return new HexUInt32(v);
        }

        public override string ToString()
        {
            return string.Format("0x{0:X8}", value);
        }
    }

    public struct HexBLOB : IXmlSerializable
    {
        static readonly string[] HexTable = new string[] {
            "00", "01", "02", "03", "04", "05", "06", "07",
            "08", "09", "0A", "0B", "0C", "0D", "0E", "0F",
            "10", "11", "12", "13", "14", "15", "16", "17",
            "18", "19", "1A", "1B", "1C", "1D", "1E", "1F",
            "20", "21", "22", "23", "24", "25", "26", "27",
            "28", "29", "2A", "2B", "2C", "2D", "2E", "2F",
            "30", "31", "32", "33", "34", "35", "36", "37",
            "38", "39", "3A", "3B", "3C", "3D", "3E", "3F",
            "40", "41", "42", "43", "44", "45", "46", "47",
            "48", "49", "4A", "4B", "4C", "4D", "4E", "4F",
            "50", "51", "52", "53", "54", "55", "56", "57",
            "58", "59", "5A", "5B", "5C", "5D", "5E", "5F",
            "60", "61", "62", "63", "64", "65", "66", "67",
            "68", "69", "6A", "6B", "6C", "6D", "6E", "6F",
            "70", "71", "72", "73", "74", "75", "76", "77",
            "78", "79", "7A", "7B", "7C", "7D", "7E", "7F",
            "80", "81", "82", "83", "84", "85", "86", "87",
            "88", "89", "8A", "8B", "8C", "8D", "8E", "8F",
            "90", "91", "92", "93", "94", "95", "96", "97",
            "98", "99", "9A", "9B", "9C", "9D", "9E", "9F",
            "A0", "A1", "A2", "A3", "A4", "A5", "A6", "A7",
            "A8", "A9", "AA", "AB", "AC", "AD", "AE", "AF",
            "B0", "B1", "B2", "B3", "B4", "B5", "B6", "B7",
            "B8", "B9", "BA", "BB", "BC", "BD", "BE", "BF",
            "C0", "C1", "C2", "C3", "C4", "C5", "C6", "C7",
            "C8", "C9", "CA", "CB", "CC", "CD", "CE", "CF",
            "D0", "D1", "D2", "D3", "D4", "D5", "D6", "D7",
            "D8", "D9", "DA", "DB", "DC", "DD", "DE", "DF",
            "E0", "E1", "E2", "E3", "E4", "E5", "E6", "E7",
            "E8", "E9", "EA", "EB", "EC", "ED", "EE", "EF",
            "F0", "F1", "F2", "F3", "F4", "F5", "F6", "F7",
            "F8", "F9", "FA", "FB", "FC", "FD", "FE", "FF",
        };
        static readonly Regex WsRegex = new Regex(@"\s+");
        static readonly int RowWidth = 4;
        byte[] values;

        static int ParseNybble(char c)
        {
            switch (c)
            {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return c - '0';

                case 'a':
                case 'b':
                case 'c':
                case 'd':
                case 'e':
                case 'f':
                    return c - ('a' - 10);

                case 'A':
                case 'B':
                case 'C':
                case 'D':
                case 'E':
                case 'F':
                    return c - ('A' - 10);

                default:
                    throw new ArgumentException("Invalid nybble: " + c);
            }
        }

        static byte[] HexStringToBytes(string hs)
        {
            if ((hs.Length & 1) != 0)
                throw new ArgumentException("Input must have even number of characters");

            byte[] result = new byte[hs.Length / 2];
            int i = 0;

            while (i < hs.Length)
            {
                int chunkLength = Math.Min(sizeof(uint) * 2, hs.Length - i);

                for (int j = i, k = chunkLength - 1; k >= 0; k -= 2)
                {
                    int high = ParseNybble(hs[i++]);
                    int low = ParseNybble(hs[i++]);

                    result[(j + k) / 2] = (byte)(high << 4 | low);
                }
            }

            return result;
        }

        static string BytesToHexString(byte[] bytes)
        {
            StringBuilder result = new StringBuilder(bytes.Length * 2);
            int i = 0;

            while (i < bytes.Length)
            {
                result.Append("\n              ");
                for (int j = 0; j < RowWidth && i < bytes.Length; j++)
                {
                    int chunkLength = Math.Min(sizeof(uint), bytes.Length - i);

                    if (j > 0)
                        result.Append(" ");
                    for (int k = chunkLength - 1; k >= 0; k--)
                        result.Append(HexTable[bytes[i + k]]);
                    i += chunkLength;
                }
            }

            return result.ToString();
        }

        public HexBLOB(byte[] b)
        {
            if (b == null)
                throw new ArgumentNullException(nameof(b));
            values = b;
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            string s = reader.ReadElementContentAsString();

            s = WsRegex.Replace(s, "");
            values = HexStringToBytes(s);
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            if (values != null)
                writer.WriteValue(BytesToHexString(values));
        }

        public static implicit operator byte[](HexBLOB b)
        {
            return b.values;
        }

        public static implicit operator HexBLOB(byte[] b)
        {
            return new HexBLOB(b);
        }

        public int Length
        {
            get => values.Length;
        }
    }
}
