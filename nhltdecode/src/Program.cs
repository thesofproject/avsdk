//
// Copyright (c) 2020-2023, Intel Corporation. All rights reserved.
//
// Authors: Piotr Maziarz <piotrx.maziarz@linux.intel.com>
//          Cezary Rojewski <cezary.rojewski@intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System.CommandLine;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace nhltdecode
{
    class Program
    {
        static void Main(string[] args)
        {
            var rootCmd = new RootCommand();

            var compileOption = new Option<FileInfo>("--compile", "compile XML file") { ArgumentHelpName = "file" };
            compileOption.AddAlias("-c");
            rootCmd.AddOption(compileOption);

            var decodeOption = new Option<FileInfo>("--decode", "decode NHLT binary file") { ArgumentHelpName = "file" };
            decodeOption.AddAlias("-d");
            rootCmd.AddOption(decodeOption);

            var outputArgument = new Argument<FileInfo>("output", "output file");
            rootCmd.AddArgument(outputArgument);

            rootCmd.AddValidator((result) =>
            {
                if (!((result.GetValueForOption(compileOption) == null) ^ (result.GetValueForOption(decodeOption) == null)))
                {
                    result.ErrorMessage = "You have to provide either --compile or --decode";
                }
            });

            rootCmd.SetHandler((compile, decode, output) =>
            {
                if (decode != null)
                    Decode(decode.FullName, output.FullName);
                else
                    Compile(compile.FullName, output.FullName);
            }, compileOption, decodeOption, outputArgument);

            rootCmd.Invoke(args);
        }

        private static void Decode(string input, string output)
        {
            var reader = new BinaryReader(new FileStream(input, FileMode.Open, FileAccess.Read),
                                          System.Text.Encoding.ASCII);
            NHLT table = null;

            try
            {
                table = BinaryReading.ReadNHLT(reader);
            }
            finally
            {
                reader.Dispose();
            }

            if (table == null)
                return;

            var xs = new XmlSerializer(typeof(NHLT));
            var settings = new XmlWriterSettings()
            {
                Indent = true,
            };
            XmlWriter writer = XmlWriter.Create(new StreamWriter(output), settings);

            try
            {
                xs.Serialize(writer, table);
            }
            finally
            {
                writer.Dispose();
            }
        }

        private static void Compile(string input, string output)
        {
            var xs = new XmlSerializer(typeof(NHLT));
            var settings = new XmlReaderSettings()
            {
                IgnoreWhitespace = false,
            };
            XmlReader reader = XmlReader.Create(new StreamReader(input), settings);
            NHLT table = null;

            try
            {
                table = (NHLT)xs.Deserialize(reader);
            }
            finally
            {
                reader.Dispose();
            }

            if (table == null)
                return;

            var writer = new BinaryWriter(new FileStream(output, FileMode.Create));

            try
            {
                BinaryWriting.WriteNHLT(writer, table);
            }
            finally
            {
                writer.Dispose();
            }
        }
    }
}
