using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace ProbeExtractor
{
    public class ProbeExtractor
    {
        /// <summary>
        /// Sample rates supported in firmware - we need to hardcode it as we only get index within this array
        /// </summary>
        private int[] PROBE_SAMPLE_RATES = { 8000, 11025, 12000, 16000, 22050, 24000, 32000, 44100,
                                             48000, 64000, 88200, 96000, 128000, 176400, 192000 };
        private readonly bool verbose;
        private readonly string inFilePath;
        private BinaryReader binaryReader;
        private long BytesLeft => binaryReader.BaseStream.Length - binaryReader.BaseStream.Position;

        public ProbeExtractor(bool verbose, string inFilePath)
        {
            this.verbose = verbose;
            this.inFilePath = inFilePath;

            FileStream probePcmDataStream = File.OpenRead(inFilePath);
            binaryReader = new BinaryReader(probePcmDataStream);
        }

        /// <summary>
        /// Converts input pcm file to output files.
        /// </summary>
        public void Convert()
        {
            Dictionary<uint, ProbeWriter> probeWriters = new Dictionary<uint, ProbeWriter>();
            uint allChunksCount = 0, chunkSuccessCount = 0, checksumMismatchCount = 0,
                 syncPatternSearchCount = 0, formatMismatchCount = 0;

            try
            {
                int ret;

                while ((ret = SearchSyncPattern(binaryReader)) >= 0)
                {
                    allChunksCount++;

                    if (ret > 0)
                        syncPatternSearchCount++;

                    if (BytesLeft < Marshal.SizeOf(typeof(ChunkHeader)))
                        break; // Less than chunk header size - there is nothing to read

                    ChunkHeader header = ChunkHeader.Create(binaryReader);

                    AudioFormat probeAudioFormat = DecodeAudioFormat(header.ProbeFormat);

                    if (!probeWriters.ContainsKey(header.ProbeId))
                    {
                        Console.WriteLine($"Found probeId {header.ProbeIdStr} with audio format {probeAudioFormat}");
                        probeWriters[header.ProbeId] = new ProbeWriter(OutFilePath(header),
                                                            probeAudioFormat, header.Wav);
                    }
                    else
                    {
                        if (!probeAudioFormat.Equals(probeWriters[header.ProbeId].Format))
                        {
                            Console.WriteLine($"Probe format changed from {probeWriters[header.ProbeId].Format} to {probeAudioFormat}");
                            formatMismatchCount++;
                        }
                    }

                    ProbeWriter probeWriter = probeWriters[header.ProbeId];

                    // last chunk may be truncated
                    uint dataSize = Math.Min((uint)BytesLeft, header.DataSize);

                    byte[] tempArray = new byte[dataSize];

                    binaryReader.Read(tempArray, 0, (int)dataSize);

                    if (BytesLeft < 8)
                    {
                        // checksum truncated in the last chunk
                        probeWriter.WriteSamples(tempArray, (int)dataSize);
                        break;
                    }

                    if (ChecksumCorrect(binaryReader.ReadUInt64(), header.ExpectedChecksum))
                    {
                        probeWriter.WriteSamples(tempArray, (int)dataSize);
                        chunkSuccessCount++;

                    }
                    else
                    {
                        checksumMismatchCount++;
                    }
                }
            }
            finally
            {
                foreach (var writer in probeWriters)
                    writer.Value.Close();
            }

            if (probeWriters.Count == 0)
                throw new Exception("Pcm file doesn't contain any probe data");

            if (verbose)
                Console.WriteLine($"Conversion finished - {chunkSuccessCount}/{allChunksCount} chunks correct");

            if (checksumMismatchCount > 0 || syncPatternSearchCount > 1 || formatMismatchCount > 0)
                throw new Exception($"Checksum mismatch: {checksumMismatchCount}, Sync pattern search: {syncPatternSearchCount}, Format mismatch: {formatMismatchCount}");
        }

        private int SearchSyncPattern(BinaryReader reader)
        {
            long startPos = reader.BaseStream.Position;

            while (reader.BaseStream.Position < reader.BaseStream.Length - 4)
            {
                if (reader.ReadUInt32() == Constants.SyncPattern)
                {
                    long skipped = reader.BaseStream.Position - 4 - startPos;

                    if (skipped > 0 && verbose)
                        Console.WriteLine($"Found sync pattern, offset 0x{startPos.ToString("X8")}, skipped {skipped} bytes");
                    return (int)(reader.BaseStream.Position - 4 - startPos);
                }
                reader.BaseStream.Position -= 3;
            }
            return -1;
        }

        // based on C struct:
        // struct ProbeDataFmtStandard
        // {
        //     uint32_t fmt_type        : 1; // = 1
        //     uint32_t standard_type   : 4;
        //     uint32_t audio_fmt       : 4;
        //     uint32_t sample_rate     : 4;
        //     uint32_t nb_channels     : 5;
        //     uint32_t sample_size     : 2;
        //     uint32_t container_size  : 2;
        //     uint32_t sample_fmt      : 1;
        //     uint32_t sample_end      : 1;
        //     uint32_t interleaving_st : 1;
        //     uint32_T _rsvd_0         : 7;
        // };
        private AudioFormat DecodeAudioFormat(uint probeFormat)
        {
            int sampleRateId = (int)((probeFormat >> 9) & 0x0f);
            if (sampleRateId >= PROBE_SAMPLE_RATES.Length)
                throw new Exception("Unsupported sample rate id: " + sampleRateId + " in probe format: " + probeFormat);

            int channels = (int)((probeFormat >> 13) & 0x1f);
            int sampleSize = (int)((probeFormat >> 18) & 0x03);
            int containerSize = (int)((probeFormat >> 20) & 0x03);

            return new AudioFormat(PROBE_SAMPLE_RATES[sampleRateId], (byte)((sampleSize + 1) * 8),
                                   (byte)((containerSize + 1) * 8), channels + 1);
        }

        private string OutFilePath(ChunkHeader header)
        {
            string outFilename = Path.GetFileNameWithoutExtension(inFilePath) + "_" + header.ProbeIdStr;
            if (header.Wav)
                outFilename = outFilename + ".wav";

            return Path.Combine(Path.GetDirectoryName(inFilePath), outFilename);
        }

        private bool ChecksumCorrect(ulong actualChecksum, ulong expectedChecksum)
        {
            // FW bug, some checksums are in 4 most significant bytes, while 4 least significant contains garbage
            if (actualChecksum > uint.MaxValue && (actualChecksum >> 32) == expectedChecksum)
            {
                if (verbose)
                    Console.WriteLine("Checksum shifted to 4 most significant bytes due to FW issue");
                return true;
            }

            if (actualChecksum == expectedChecksum)
                return true;

            Console.WriteLine($"Checksum mismatch. Expected: {expectedChecksum}, actual: {actualChecksum}");
            return false;
        }
    }
}
