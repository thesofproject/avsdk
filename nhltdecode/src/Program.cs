//
// Copyright (c) 2020-2022, Intel Corporation. All rights reserved.
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

            var blobOption = new Option<bool>("--blob", "parse blob while decoding binary");
            blobOption.AddAlias("-b");
            rootCmd.AddOption(blobOption);

            var outputArgument = new Argument<FileInfo>("output", "output file");
            rootCmd.AddArgument(outputArgument);

            rootCmd.AddValidator((result) =>
            {
                if (!((result.GetValueForOption(compileOption) == null) ^ (result.GetValueForOption(decodeOption) == null)))
                {
                    result.ErrorMessage = "You have to provide either --compile or --decode";
                }
            });

            rootCmd.SetHandler((compile, decode, parseBlob, output) =>
            {
                if (decode != null)
                    Decode(decode.FullName, output.FullName, parseBlob);
                else
                    Compile(compile.FullName, output.FullName);
            }, compileOption, decodeOption, blobOption, outputArgument);

            rootCmd.Invoke(args);
        }

        private static void Decode(string input, string output, bool parseBlob)
        {
        }

        private static void Compile(string input, string output)
        {
        }
    }
}
