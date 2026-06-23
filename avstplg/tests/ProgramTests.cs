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

namespace avstplg.tests
{
    public class ProgramTests : IDisposable
    {
        readonly string tempDir;
        readonly string tempInputFile;
        readonly string tempOutputFile;

        const string ValidTopologyXml =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Topology>
  <Name>test_topology</Name>
  <Version>1</Version>
  <Libraries></Libraries>
  <AudioFormats></AudioFormats>
  <ModuleConfigsBase></ModuleConfigsBase>
  <ModuleConfigsExt></ModuleConfigsExt>
  <PipelineConfigs></PipelineConfigs>
  <Bindings></Bindings>
  <PathTemplates></PathTemplates>
  <CondpathTemplates></CondpathTemplates>
  <FEDAIs></FEDAIs>
  <Graphs></Graphs>
</Topology>";

        // Schema forbids any child content under Topology, so the valid topology
        // (which has children) raises validation events handled by the callback.
        const string StrictXsd =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
  <xs:element name=""Topology"">
    <xs:complexType>
      <xs:sequence />
    </xs:complexType>
  </xs:element>
</xs:schema>";

        public ProgramTests()
        {
            string name = "avstplg_tests_" + Guid.NewGuid().ToString("N");

            tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), name);
            Directory.CreateDirectory(tempDir);
            tempInputFile = System.IO.Path.Combine(tempDir, "input.xml");
            tempOutputFile = System.IO.Path.Combine(tempDir, "output.ucm");
        }

        public void Dispose()
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }

        private static int InvokeMain(params string[] args)
        {
            Type programType = typeof(ExtensionMethods).Assembly.GetType("avstplg.Program");
            BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
            MethodInfo mainMethod = programType.GetMethod("Main", flags);

            return (int)mainMethod.Invoke(null, new object[] { args });
        }

        [Fact]
        public void HelpOption()
        {
            Assert.Equal(0, InvokeMain("-h"));
            Assert.Equal(0, InvokeMain("--help"));
        }

        [Fact]
        public void HelpTakesPrecedence()
        {
            Assert.Equal(0, InvokeMain("-c", "input.xml", "--help"));
        }

        [Fact]
        public void VersionOption()
        {
            Assert.Equal(0, InvokeMain("-v"));
            Assert.Equal(0, InvokeMain("--version"));
        }

        [Fact]
        public void NoArguments()
        {
            Assert.Equal(1, InvokeMain());
        }

        [Fact]
        public void MissingArgument()
        {
            Assert.Equal(1, InvokeMain("-c", "input.xml"));
            Assert.Equal(1, InvokeMain("-o", "output.ucm"));
        }

        [Theory]
        [InlineData("-c", "duplicate")]
        [InlineData("-u", "unknown")]
        public void InvalidArguments(string option1, string option2)
        {
            Assert.Equal(1, InvokeMain("-c", tempInputFile, "-o", tempOutputFile, option1, option2));
            Assert.False(File.Exists(tempOutputFile));
        }

        [Fact]
        public void OddNumberOfArguments()
        {
            Assert.Equal(1, InvokeMain("-c", tempInputFile, "-o", tempOutputFile, "--odd"));
            Assert.False(File.Exists(tempOutputFile));
        }

        [Fact]
        public void NonExistentInput()
        {
            Assert.Equal(1, InvokeMain("-c", tempInputFile, "-o", tempOutputFile));
            Assert.False(File.Exists(tempOutputFile));
        }

        [Fact]
        public void MalformedInput()
        {
            File.WriteAllText(tempInputFile, "<Topology><Name>broken</Topology>");

            Assert.Equal(1, InvokeMain("-c", tempInputFile, "-o", tempOutputFile));
            Assert.False(File.Exists(tempOutputFile));
        }

        [Theory]
        [InlineData("-c", "-o")]
        [InlineData("--compile", "--output")]
        public void Compile(string option1, string option2)
        {
            File.WriteAllText(tempInputFile, ValidTopologyXml);

            int result = InvokeMain(option1, tempInputFile, option2, tempOutputFile);

            Assert.Equal(0, result);
            Assert.True(new FileInfo(tempOutputFile).Length > 0);
        }

        [Fact]
        public void CompileWithXsd()
        {
            string xsd = System.IO.Path.Combine(tempDir, "schema.xsd");

            File.WriteAllText(tempInputFile, ValidTopologyXml);
            File.WriteAllText(xsd, StrictXsd);

            Assert.Equal(0, InvokeMain("-c", tempInputFile, "-o", tempOutputFile, "-x", xsd));
            Assert.True(new FileInfo(tempOutputFile).Length > 0);
        }
    }
}
