//
// Copyright (c) 2020-2026, Intel Corporation. All rights reserved.
//
// Authors: Cezary Rojewski <cezary.rojewski@intel.com>
//          Piotr Maziarz <piotrx.maziarz@linux.intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System;
using System.Collections.Generic;
using System.IO;
using probe2wav.Native;

namespace probe2wav
{
    public class DataExtractor
    {
        delegate void PrintMessage(string message);

        Dictionary<uint, DataWriter> writers;
        PrintMessage printVerbose;
        bool ignoreChecksum;

        uint statHits;
        uint statGaps;
        uint statMismatches;

        DataExtractor(bool verbose, bool ignoreChecksum)
        {
            if (verbose)
                printVerbose = Console.WriteLine;
            else
                printVerbose = delegate (string m) { };

            this.ignoreChecksum = ignoreChecksum;
            writers = new Dictionary<uint, DataWriter>();
        }

        void Reset()
        {
            writers.Clear();
            statHits = 0;
            statGaps = 0;
            statMismatches = 0;
        }

        static void Print(BinaryReader reader, uint id, string msg)
        {
            long pos = reader.BaseStream.Position;

            Console.WriteLine($"Position: 0x{pos.ToString("X8")}, probe: 0x{id.ToString("X8")} {msg}");
        }

        void PrintVerbose(BinaryReader reader, uint id, string msg)
        {
            long pos = reader.BaseStream.Position;

            printVerbose($"Position: 0x{pos.ToString("X8")}, probe: 0x{id.ToString("X8")} {msg}");
        }

        long SeekProbe(BinaryReader reader)
        {
            Stream stream = reader.BaseStream;
            long start = stream.Position;

            // There may be gaps in the binary, check every byte.
            for (; reader.Remaining() > 0; stream.Position++)
            {
                if (reader.PeekUInt32() == Constants.SYNC_WORD)
                {
                    long pos = stream.Position;
                    long gap = pos - start;

                    if (gap > 0)
                    {
                        statGaps++;
                        printVerbose($"Position: 0x{start.ToString("X8")}-0x{pos.ToString("X8")}, gap {gap} bytes");
                    }

                    return pos;
                }
            }

            return -1;
        }

        ProbePacketHeader ReadVerifyProbe(BinaryReader reader)
        {
            ProbePacketHeader hdr = reader.Read<ProbePacketHeader>();
            DataFormat format = hdr.DataFormat;

            // Data and Checksum follow the header, see struct ProbePacket.
            if (reader.Remaining() < hdr.DataSize + sizeof(long))
            {
                Print(reader, hdr.ProbeId, "spans beyond EOF");
                throw new EndOfStreamException();
            }

            if (hdr.DataSize == 0)
                PrintVerbose(reader, hdr.ProbeId, "holds no data");
            if (format.SampleRate == uint.MaxValue)
                PrintVerbose(reader, hdr.ProbeId, $"invalid rate: {format.SampleRate0}");

            statHits++;
            return hdr;
        }

        static string GenerateOutputPath(string filename, uint id)
        {
            string path = Path.ChangeExtension(filename, null);
            string ext = id.IsWavType() ? ".wav" : ".bin";

            return String.Join(null, path, "_0x", id.ToString("X8"), ext);
        }

        DataWriter GetDataWriter(BinaryReader reader, ProbePacketHeader hdr)
        {
            string filename = (reader.BaseStream as FileStream).Name;
            uint id = hdr.ProbeId;

            if (writers.ContainsKey(id))
            {
                return writers[id];
            }

            Print(reader, id, "FOUND!");

            string output = GenerateOutputPath(filename, id);
            var writer = new DataWriter(output, id, hdr.DataFormat);

            writers[id] = writer;
            return writer;
        }

        void WriteData(DataWriter writer, BinaryReader reader, uint dataSize)
        {
            int remaining = Math.Min((int)reader.Remaining(), (int)dataSize);
            byte[] buf = new byte[1024];

            while (remaining > 0)
            {
                int n = Math.Min(buf.Length, remaining);

                reader.Read(buf, 0, n);
                writer.Write(buf, 0, n);
                remaining -= n;
            }
        }

        bool TestChecksum(BinaryReader reader, ProbePacketHeader hdr)
        {
            ulong expected = hdr.Checksum();
            ulong actual = reader.ReadUInt64();

            //
            // WA: In older AudioDSP firmware versions the checksum may be
            // located in the upper 32 bits with 32 lower containing garbage.
            //
            if (actual > uint.MaxValue && (actual >> 32) == expected)
            {
                PrintVerbose(reader, hdr.ProbeId, $"checksum: 0x{actual.ToString("X")} shifted to upper 32 bits");
                return true;
            }

            if (actual == expected)
                return true;

            statMismatches++;
            PrintVerbose(reader, hdr.ProbeId, $"checksum mismatch, expected: 0x{expected.ToString("X")} " +
                                              $"actual: 0x{actual.ToString("X")}");

            return ignoreChecksum;
        }

        void Extract(BinaryReader reader)
        {
            Reset();
            while (SeekProbe(reader) != -1)
            {
                ProbePacketHeader hdr = ReadVerifyProbe(reader);

                // The checksum is located after Data, see struct ProbePacket.
                reader.BaseStream.Position += hdr.DataSize;

                if (TestChecksum(reader, hdr))
                {
                    DataWriter writer = GetDataWriter(reader, hdr);
                    long save = reader.BaseStream.Position;

                    // Navigate back to Data, see struct ProbePacket.
                    reader.BaseStream.Position -= hdr.DataSize + sizeof(long);
                    WriteData(writer, reader, hdr.DataSize);
                    reader.BaseStream.Position = save;
                }
            }
        }

        void TryExtract(string filename)
        {
            using (FileStream stream = File.OpenRead(filename))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                try
                {
                    Extract(reader);
                }
                catch (EndOfStreamException)
                {
                }
                finally
                {
                    foreach (DataWriter writer in writers.Values)
                        writer.Dispose();
                }
            }

            Console.WriteLine($"Total hits: {statHits}, gaps: {statGaps}, checksum mismatches: {statMismatches}");
        }

        public static void Extract(string filename, bool verbose, bool ignoreChecksum)
        {
            new DataExtractor(verbose, ignoreChecksum).TryExtract(filename);
        }
    }
}
