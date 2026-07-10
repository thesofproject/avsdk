//
// Copyright (c) 2026, Intel Corporation. All rights reserved.
//
// Author: Cezary Rojewski <cezary.rojewski@intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System;
using System.IO;
using System.Reflection;
using Xunit;

namespace nhltdecode.tests
{
    public class ProgramTests : IDisposable
    {
        readonly string tempDir;
        readonly string tempBinary;
        readonly string tempXML;

        const string NHLT_XML =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<NHLT xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <Revision>5</Revision>
  <OemId>INTEL</OemId>
  <OemTableId>TGL</OemTableId>
  <OemRevision>1</OemRevision>
  <AslCompilerId />
  <AslCompilerRevision>2</AslCompilerRevision>
  <Endpoints />
</NHLT>";

        public ProgramTests()
        {
            tempDir = Path.Combine(Path.GetTempPath(), "nhltdecode_tests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            tempBinary = Path.Combine(tempDir, "nhlt.bin");
            tempXML = Path.Combine(tempDir, "nhlt.xml");
        }

        public void Dispose()
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }

        private static int InvokeMain(params string[] args)
        {
            Type programType = typeof(ExtensionMethods).Assembly.GetType("nhltdecode.Program");
            BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
            MethodInfo mainMethod = programType.GetMethod("Main", flags);

            return (int)mainMethod.Invoke(null, new object[] { args });
        }

        [Fact]
        public void HelpOption()
        {
            Assert.Equal(0, InvokeMain("-h"));
            Assert.Equal(0, InvokeMain("--help"));
            Assert.Equal(0, InvokeMain("-c", "--help"));
        }

        [Fact]
        public void VersionOption()
        {
            Assert.Equal(0, InvokeMain("-v"));
            Assert.Equal(0, InvokeMain("--version"));
            Assert.Equal(0, InvokeMain("-c", "--version"));
        }

        [Fact]
        public void MissingArgument()
        {
            Assert.Equal(1, InvokeMain());
            Assert.Equal(1, InvokeMain("-c"));
            Assert.Equal(1, InvokeMain("-c", tempXML));
            Assert.Equal(1, InvokeMain("-c", "-o", tempXML));
            Assert.Equal(1, InvokeMain("-c", tempXML, "-o"));
            Assert.Equal(1, InvokeMain("-d", tempBinary));
            Assert.Equal(1, InvokeMain("-d", "-o", tempBinary));
            Assert.Equal(1, InvokeMain("-d", tempBinary, "-o"));
        }

        [Fact]
        public void InvalidArguments()
        {
            Assert.Equal(1, InvokeMain("-c", tempXML, "-d", tempBinary, "-o", "file"));
            Assert.False(File.Exists(tempBinary));
            Assert.Equal(1, InvokeMain("-d", tempBinary, "-c", tempXML, "-o", "file"));
            Assert.False(File.Exists(tempXML));
            Assert.Equal(1, InvokeMain("-c", tempXML, "-c", tempXML, "-o", "file"));
            Assert.False(File.Exists(tempBinary));
            Assert.Equal(1, InvokeMain("-d", tempBinary, "-d", tempBinary, "-o", "file"));
            Assert.False(File.Exists(tempXML));

            Assert.Equal(1, InvokeMain("-u", tempXML, "unknown", tempBinary));
            Assert.False(File.Exists(tempXML));
            Assert.False(File.Exists(tempBinary));
        }

        [Fact]
        public void NonExistentInput()
        {
            Assert.Equal(1, InvokeMain("-c", tempXML, "-o", tempBinary));
            Assert.False(File.Exists(tempBinary));
            Assert.Equal(1, InvokeMain("-d", tempBinary, "-o", tempXML));
            Assert.False(File.Exists(tempXML));
        }

        [Fact]
        public void Compile()
        {
            File.WriteAllText(tempXML, NHLT_XML);
            InvokeMain("-c", tempXML, "-o", tempBinary);

            Assert.True(File.Exists(tempBinary));
            Assert.Contains("NHLT", File.ReadAllText(tempXML));
        }

        [Fact]
        public void Decode()
        {
            File.WriteAllBytes(tempBinary, Data.NHLTBytes);
            InvokeMain("-d", tempBinary, "-o", tempXML);

            Assert.True(File.Exists(tempXML));
            Assert.Contains("NHLT", File.ReadAllText(tempXML));
        }

        [Fact]
        public void CompileDecode()
        {
            string tempXML2 = Path.Combine(tempDir, "nhlt2.xml");

            File.WriteAllText(tempXML, NHLT_XML);
            InvokeMain("-c", tempXML, "-o", tempBinary);
            InvokeMain("-d", tempBinary, "-o", tempXML2);

            Assert.True(File.Exists(tempXML));
            Assert.True(File.Exists(tempXML2));
            Assert.Equal(File.ReadAllText(tempXML), File.ReadAllText(tempXML2), ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void DecodeCompile()
        {
            string tempBinary2 = Path.Combine(tempDir, "nhlt2.bin");

            File.WriteAllBytes(tempBinary, Data.NHLTBytes);
            InvokeMain("-d", tempBinary, "-o", tempXML);
            InvokeMain("-c", tempXML, "-o", tempBinary2);

            Assert.True(File.Exists(tempBinary));
            Assert.True(File.Exists(tempBinary2));
            Assert.Equal(File.ReadAllBytes(tempBinary), File.ReadAllBytes(tempBinary2));
        }
    }
}
