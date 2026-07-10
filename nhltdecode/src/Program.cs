//
// Copyright (c) 2020-2023, Intel Corporation. All rights reserved.
//
// Authors: Piotr Maziarz <piotrx.maziarz@linux.intel.com>
//          Cezary Rojewski <cezary.rojewski@intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace nhltdecode
{
    class Program
    {
        struct Option
        {
            internal string shortName;
            internal string longName;

            internal Option(string s, string l)
            {
                shortName = s;
                longName = l;
            }

            internal bool Matches(string arg)
            {
                return arg.Equals(shortName) || arg.Equals(longName);
            }

            public override string ToString()
            {
                return $"{shortName}, {longName}";
            }
        }

        static readonly Option s_compile = new Option("-c", "--compile");
        static readonly Option s_decode = new Option("-d", "--decode");
        static readonly Option s_output = new Option("-o", "--output");
        static readonly Option s_help = new Option("-h", "--help");
        static readonly Option s_version = new Option("-v", "--version");

        static readonly string s_appName = AppDomain.CurrentDomain.FriendlyName;
        static readonly Dictionary<string, Option> s_parseOptions = new Dictionary<string, Option>()
        {
            { "compile", s_compile },
            { "decode", s_decode },
            { "output", s_output },
        };

        static int ShowHelp(int code)
        {
            Console.WriteLine($"Usage: {s_appName} [-c | -d] [FILE] -o [OUTPUT]");
            Console.WriteLine();
            Console.WriteLine($"  {s_compile} FILE\tPath to XML document to compile");
            Console.WriteLine($"  {s_decode} FILE\tPath to binary file to decode");
            Console.WriteLine($"  {s_output} FILE\tPath to output file to create");
            Console.WriteLine($"  {s_help}\t\tShow this message and exit");
            Console.WriteLine($"  {s_version}\t\tOutput version information and exit");
            return code;
        }

        static Dictionary<string, string> ParseArguments(string[] args)
        {
            if (args.Length == 0)
                return null;
            // Working with pairs: option and filename.
            if (args.Length % 2 != 0)
                return null;

            var result = new Dictionary<string, string>();

            for (int i = 0; i < args.Length; i += 2)
            {
                string option = args[i];
                string key = s_parseOptions.FirstOrDefault(p => p.Value.Matches(option)).Key;

                if (key == null || result.ContainsKey(key))
                    return null;

                result[key] = args[i + 1];
            }

            return result;
        }

        static int VerifyArguments(Dictionary<string, string> dictionary)
        {
            if (dictionary == null)
                return ShowHelp(1);

            if (!dictionary.ContainsKey("output"))
            {
                Console.WriteLine("Please specify -o argument.");
                return ShowHelp(1);
            }

            if ((dictionary.ContainsKey("compile") && dictionary.ContainsKey("decode")) ||
                !dictionary.ContainsKey("compile") && !dictionary.ContainsKey("decode"))
            {
                Console.WriteLine("Please specify either -c or -d.");
                return ShowHelp(1);
            }

            return 0;
        }

        static void Compile(string input, string output)
        {
            NHLT table;
            var xs = new XmlSerializer(typeof(NHLT));
            var settings = new XmlReaderSettings()
            {
                IgnoreWhitespace = false,
                CloseInput = true,
            };

            using (var stream = new StreamReader(input))
            using (XmlReader reader = XmlReader.Create(stream, settings))
                table = (NHLT)xs.Deserialize(reader);

            using (var stream = new FileStream(output, FileMode.Create))
            using (var writer = new BinaryWriter(stream))
                BinaryWriting.WriteNHLT(writer, table);
        }

        static void Decode(string input, string output)
        {
            NHLT table;
            var xs = new XmlSerializer(typeof(NHLT));
            var settings = new XmlWriterSettings()
            {
                Indent = true,
                CloseOutput = true,
            };

            using (var stream = new FileStream(input, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(stream, System.Text.Encoding.ASCII))
                table = BinaryReading.ReadNHLT(reader);

            using (var stream = new StreamWriter(output))
            using (XmlWriter writer = XmlWriter.Create(stream, settings))
                xs.Serialize(writer, table);
        }

        static int Main(string[] args)
        {
            if (args.Any(a => s_help.Matches(a)))
                return ShowHelp(0);

            if (args.Any(a => s_version.Matches(a)))
            {
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                Console.WriteLine($"Intel {s_appName} tool, version {version}");
                return 0;
            }

            Dictionary<string, string> dictionary = ParseArguments(args);

            int ret = VerifyArguments(dictionary);
            if (ret != 0)
                return ret;

            try
            {
                if (dictionary.ContainsKey("compile"))
                    Compile(dictionary["compile"], dictionary["output"]);
                else
                    Decode(dictionary["decode"], dictionary["output"]);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"nhltdecode failed. {ex.GetType()}: {ex.Message}");
                return 1;
            }

            return 0;
        }
    }
}
