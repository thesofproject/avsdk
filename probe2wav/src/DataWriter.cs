//
// Copyright (c) 2020-2026, Intel Corporation. All rights reserved.
//
// Authors: Cezary Rojewski <cezary.rojewski@intel.com>
//          Piotr Maziarz <piotrx.maziarz@linux.intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System;
using System.IO;
using probe2wav.Native;

namespace probe2wav
{
    public class DataWriter : IDisposable
    {
        BinaryWriter writer;
        DataFormat format;
        uint bytesWritten;
        uint probeId;
        bool disposed;

        public DataWriter(string filename, uint id, DataFormat fmt)
        {
            var stream = new FileStream(filename, FileMode.Create);

            writer = new BinaryWriter(stream);
            format = fmt;
            bytesWritten = 0;
            probeId = id;
            disposed = false;

            // Header is updated when closing the writer to reflect bytesWritten.
            if (probeId.IsWavType())
                WriteWavHeader();
        }

        public void Write(byte[] buffer, int index, int count)
        {
            writer.Write(buffer, index, count);
            bytesWritten += (uint)count;
        }

        private void WriteWavHeader()
        {
            WavHeader hdr;

            // Constants for PCM WAV header.
            hdr.ChunkID         = new char[] { 'R', 'I', 'F', 'F' };
            hdr.Format          = new char[] { 'W', 'A', 'V', 'E' };
            hdr.Subchunk1ID     = new char[] { 'f', 'm', 't', ' ' };
            hdr.Subchunk2ID     = new char[] { 'd', 'a', 't', 'a' };
            hdr.Subchunk1Size   = 16; // PCM
            hdr.AudioFormat     = 1;  // PCM (linear quantization)

            // 36 = WAV header size without ChunkID and ChunkSize
            hdr.ChunkSize       = 36 + bytesWritten;
            hdr.NumChannels     = (ushort)format.NumChannels;
            hdr.BitsPerSample   = (ushort)format.ContainerBits;
            hdr.BlockAlign      = (ushort)(format.NumChannels * format.ContainerBits / 8);
            hdr.ByteRate        = format.SampleRate * hdr.BlockAlign;
            hdr.SampleRate      = format.SampleRate;
            hdr.Subchunk2Size   = bytesWritten;

            writer.Seek(0, SeekOrigin.Begin);
            writer.Write<WavHeader>(hdr);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                if (probeId.IsWavType())
                    WriteWavHeader();
                writer.Dispose();
            }

            disposed = true;
        }
    }
}
