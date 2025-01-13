//
// Copyright (c) 2020-2022, Intel Corporation. All rights reserved.
//
// Author: Piotr Maziarz <piotrx.maziarz@linux.intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System;
using System.IO;
using System.Text;

namespace probe2wav
{
    public class ProbeWriter
    {
        public readonly AudioFormat Format;

        private BinaryWriter writer;
        private long bytesWritten;
        private readonly bool wav;

        private readonly int headerSize = 36;

        /// <summary>
        /// Creates new empty file, if wav - adds proper header.
        /// Data must be written and file must be closed.
        /// Size in wav header is corrected while closing file.
        /// </summary>
        /// <param name="file">Path to file to create</param>
        /// <param name="format">Audio format</param>
        /// <param name="wav">Is output file wav or binary data</param>
        public ProbeWriter(string file, AudioFormat format, bool wav)
        {
            Format = format;
            bytesWritten = 0;
            this.wav = wav;

            FileStream fileStream = new FileStream(file, FileMode.Create);
            writer = new BinaryWriter(fileStream);
            if (wav)
                WriteWavHeader();
        }

        public void WriteSamples(byte[] nextSamples, int count)
        {
            int samplesCount = count / Format.BlockAlign;

            if (wav && samplesCount * Format.BlockAlign != count)
                throw new Exception("Number of bytes to write " + count + " is not a multiple of block align " + Format.BlockAlign);
            bytesWritten += count;
            writer.Write(nextSamples, 0, count);
        }

        /// <summary>
        /// Closes file and updates wav header - ProbeWriter can no longer be used after closing.
        /// </summary>
        public void Close()
        {
            if (wav)
                WriteWavHeader();
            writer.Close();
            writer = null;
        }

        /// <summary>
        /// Seeks to beginning of file and writes proper wav header.
        /// </summary>
        private void WriteWavHeader()
        {
            if (headerSize + bytesWritten > uint.MaxValue)
                throw new Exception("Audio file to big: " + bytesWritten + ", wav header size overflow");
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write((uint)(headerSize + bytesWritten));
            writer.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
            writer.Write(16); // Format data lenght, constant
            writer.Write((ushort)1); // Encoding
            writer.Write((ushort)Format.Channels);
            writer.Write((uint)Format.SampleRate);
            writer.Write(Format.SampleRate * Format.BlockAlign); // Average bytes per second
            writer.Write((ushort)Format.BlockAlign);
            writer.Write((ushort)Format.ContainerBits);
            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write((uint)bytesWritten);
        }
    }

    public struct AudioFormat : IEquatable<AudioFormat>
    {
        public readonly int SampleRate;
        public readonly byte ValidBits;
        public readonly byte ContainerBits;
        public readonly int Channels;

        public int BlockAlign { get { return Channels * ContainerBits / 8; } }

        public AudioFormat(int sampleRate, byte validBits, byte containerBits, int channels)
        {
            SampleRate = sampleRate;
            ValidBits = validBits;
            ContainerBits = containerBits;
            Channels = channels;
        }

        public override string ToString()
            => ValidBits + "bit / " + ContainerBits + "bit " + SampleRate + " " + Channels + "channels";

        public override bool Equals(object obj)
            => obj is AudioFormat && Equals((AudioFormat)obj);

        public bool Equals(AudioFormat other)
            => SampleRate == other.SampleRate && ValidBits == other.ValidBits && ContainerBits == other.ContainerBits && Channels == other.Channels;

        public override int GetHashCode()
        {
            return ((Channels) << 28) | ((ContainerBits) << 24) | ((ValidBits) << 20) | SampleRate;
        }

        public static bool operator ==(AudioFormat lhs, AudioFormat rhs)
            => lhs.Equals(rhs);

        public static bool operator !=(AudioFormat lhs, AudioFormat rhs)
            => !(lhs.Equals(rhs));
    }
}
