//
// Copyright (c) 2026, Intel Corporation. All rights reserved.
//
// Author: Cezary Rojewski <cezary.rojewski@intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System;
using System.IO;
using Xunit;

namespace probe2wav.tests
{
    public class ProgramTests : IDisposable
    {
        readonly string tempDir;
        readonly string tempFile;

        public ProgramTests()
        {
            tempDir = Path.Combine(Path.GetTempPath(), "probe2wav_tests_" + Guid.NewGuid().ToString("N"));
            tempFile = Path.Combine(tempDir, "empty.bin");

            Directory.CreateDirectory(tempDir);
            File.WriteAllBytes(tempFile, Array.Empty<byte>());            
        }

        public void Dispose()
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }

        static int RunMain(params string[] args)
        {
            // Use reflection to invoke Program.Main since it's private.
            var type = typeof(DataExtractor).Assembly.GetType("probe2wav.Program");
            var method = type.GetMethod("Main", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            return (int)method.Invoke(null, new object[] { args });
        }

        [Fact]
        public void NoArgs()
        {
            Assert.Equal(0, RunMain());
        }

        [Fact]
        public void NoOptions()
        {
            Assert.Equal(0, RunMain(tempFile));
        }

        [Fact]
        public void InvalidFile()
        {
            Assert.Equal(1, RunMain("nonexistent.bin"));
        }

        [Theory]
        [InlineData("-h")]
        [InlineData("--help")]
        public void HelpOption(string option)
        {
            Assert.Equal(0, RunMain(option));
            Assert.Equal(0, RunMain(option, tempFile));
            Assert.Equal(0, RunMain(option, "nonexistent.bin"));
        }

        [Theory]
        [InlineData("-v")]
        [InlineData("--verbose")]
        [InlineData("--ignore-checksum")]
        public void Options(string option)
        {
            Assert.Equal(0, RunMain(option));
            Assert.Equal(0, RunMain(option, tempFile));
        }

        [Theory]
        [InlineData("-v")]
        [InlineData("--verbose")]
        [InlineData("--ignore-checksum")]
        public void OptionsInvalidFile(string option)
        {
            Assert.Equal(1, RunMain(option, "nonexistent.bin"));
        }

        [Theory]
        [InlineData("-v", "--help")]
        [InlineData("-h", "--ignore-checksum")]
        [InlineData("--ignore-checksum", "--verbose")]
        public void OptionsMixed(string option1, string option2)
        {
            Assert.Equal(0, RunMain(option1, option2, tempFile));
        }

        [Theory]
        [InlineData("-c")]
        [InlineData("an_option")]
        public void UnknownOption(string option)
        {
            Assert.Equal(1, RunMain(option, tempFile));
        }

        [Fact]
        public void ExtractGracefulException()
        {
            // Hold an exclusive lock so Extract() cannot open the file.
            using var fs = new FileStream(tempFile, FileMode.Open, FileAccess.Read, FileShare.None);

            Assert.Equal(1, RunMain(tempFile));
        }
    }
}
