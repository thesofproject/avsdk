using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace ProbeExtractor
{
    public class WavProbeExtractor
    {
        /// <summary>
        /// Sample rates supported in firmware - we need to hardcode it as we only get index within this array
        /// </summary>
        private int[] PROBE_SAMPLE_RATES = { 8000, 11025, 12000, 16000, 22050, 24000, 32000, 44100,
                                             48000, 64000, 88200, 96000, 128000, 176400, 192000 };
        private readonly string inFilePath;
        private BinaryReader binaryReader;
        private long BytesLeft => binaryReader.BaseStream.Length - binaryReader.BaseStream.Position;

        public WavProbeExtractor(string inFilePath)
        {
            this.inFilePath = inFilePath;

            FileStream probePcmDataStream = File.OpenRead(inFilePath);
            binaryReader = new BinaryReader(probePcmDataStream);
        }

        /// <summary>
        /// Converts input pcm file to output wav file.
        /// Fails after any checksum issue.
        /// </summary>
        public void ConvertDataToWav()
        {
            Dictionary<uint, WavBuilder> wavBuilders = new Dictionary<uint, WavBuilder>();
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

                    if (!wavBuilders.ContainsKey(header.ProbeId))
                    {
                        Console.WriteLine($"Found probeId {header.ProbeIdStr} with audio format {probeAudioFormat}");
                        wavBuilders[header.ProbeId] = new WavBuilder(OutFilePath(header.ProbeIdStr),
                                                                     probeAudioFormat);
                    }
                    else
                    {
                        if (!probeAudioFormat.Equals(wavBuilders[header.ProbeId].Format))
                        {
                            Console.WriteLine($"Probe format changed from {wavBuilders[header.ProbeId].Format} to {probeAudioFormat}");
                            formatMismatchCount++;
                        }
                    }

                    WavBuilder wavBuilder = wavBuilders[header.ProbeId];

                    // last chunk may be truncated
                    uint dataSize = Math.Min((uint)BytesLeft, header.DataSize);

                    byte[] tempArray = new byte[dataSize];

                    binaryReader.Read(tempArray, 0, (int)dataSize);

                    if (BytesLeft < 8)
                    {
                        // checksum truncated in the last chunk
                        wavBuilder.WriteSamples(tempArray, (int)dataSize);
                        break;
                    }

                    ulong actualCheckSum = binaryReader.ReadUInt64();

                    if (actualCheckSum != header.ExpectedChecksum)
                    {
                        Console.WriteLine($"Checksum mismatch. Expected: {header.ExpectedChecksum}, actual: {actualCheckSum}");
                        checksumMismatchCount++;
                    }
                    else
                    {
                        wavBuilder.WriteSamples(tempArray, (int)dataSize);
                        chunkSuccessCount++;
                    }
                }
            }
            finally
            {
                foreach (var builder in wavBuilders)
                    builder.Value.Close();
            }

            if (wavBuilders.Count == 0)
                throw new Exception("Pcm file doesn't contain any probe data");

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

                    if (skipped > 0)
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

        private string OutFilePath(string probeId)
        {
            string outFilename = Path.GetFileNameWithoutExtension(inFilePath) + "_" + probeId + ".wav";

            return Path.Combine(Path.GetDirectoryName(inFilePath), outFilename);
        }
    }
}
