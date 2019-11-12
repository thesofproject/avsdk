﻿// Program.cs - Console application
//
// Copyright (c) 2018-2019 Intel Corporation
//
// Author: Cezary Rojewski <cezary.rojewski@intel.com>
//
// The source code contained or described herein and all documents
// related to the source code ("Material") are owned by Intel Corporation
// or its suppliers or licensors. Title to the Material remains with
// Intel Corporation or its suppliers and licensors. The Material contains
// trade secrets and proprietary and confidential information of Intel or
// its suppliers and licensors. The Material is protected by worldwide
// copyright and trade secret laws and treaty provisions. No part of the
// Material may be used, copied, reproduced, modified, published, uploaded,
// posted, transmitted, distributed, or disclosed in any way without Intel's
// prior express written permission.
//
// No license under any patent, copyright, trade secret or other intellectual
// property right is granted to or conferred upon you by disclosure or
// delivery of the Materials, either expressly, by implication, inducement,
// estoppel or otherwise. Any license under such intellectual property
// rights must be express and approved by Intel in writing.
//

using NUcmSerializer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace itt
{
    class Program
    {
        struct Option
        {
            internal string Short;
            internal string Long;

            internal Option(string s, string l)
            {
                Short = s;
                Long = l;
            }

            internal bool Matches(string arg)
            {
                return arg.Equals(Short) || arg.Equals(Long);
            }

            public override string ToString()
            {
                return $"{Short}, {Long}";
            }
        }

        static Option Input = new Option("-c", "--config");
        static Option Output = new Option("-o", "--tplg-conf");
        static Option Help = new Option("-h", "--help");
        static Option Version = new Option("-v", "--version");

        static readonly string appName = AppDomain.CurrentDomain.FriendlyName;
        static readonly Dictionary<string, Option> required =
            new Dictionary<string, Option>
        {
            { "input", Input },
            { "output", Output },
        };

        static int Main(string[] args)
        {
            if (args.Any(a => Help.Matches(a)))
            {
                ShowHelp();
                return 0;
            }
            else if(args.Any(a => Version.Matches(a)))
            {
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                Console.WriteLine($"Intel Topology Tool, version {version}");
                return 0;
            }

            var dictionary = ParseArguments(args);
            if (!dictionary.ContainsKey("input") ||
                !dictionary.ContainsKey("output"))
            {
                Console.WriteLine($"Please specify -c and -o arguments.");
                ShowShortHelp();
                return 1;
            }

            try
            {
                System system;
                var deserializer = new XmlSerializer(typeof(System));
                using (var stream = new FileStream(dictionary["input"], FileMode.Open))
                {
                    system = (System)deserializer.Deserialize(stream);
                }

                var serializer = new UcmSerializer();
                var sections = new UcmConverter(system).GetAllSections();
                using (var stream = new FileStream(dictionary["output"], FileMode.Create))
                {
                    serializer.Serialize(stream, sections);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{appName} failed. Message: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine();
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine("Inner stack trace:");
                    Console.WriteLine(ex.InnerException.StackTrace);
                }

                Console.WriteLine();
                Console.WriteLine("Stack trace:");
                Console.WriteLine(ex.StackTrace);
                return 1;
            }

            return 0;
        }

        static void ShowShortHelp()
        {
            Console.WriteLine($"Try {appName} --help for more information.");
        }

        static void ShowHelp()
        {
            Console.WriteLine($"Usage: {appName} -c [INPUT] -o [OUTPUT]");
            Console.WriteLine();
            Console.WriteLine($"  {Input}=FILE\tPath to XML document to convert");
            Console.WriteLine($"  {Output}=FILE\tPath to output file to create");
            Console.WriteLine($"  {Help}\t\tShow this message and exit");
            Console.WriteLine($"  {Version}\t\tOutput version information and exit");
        }

        static Dictionary<string, string> ParseArguments(string[] args)
        {
            var result = new Dictionary<string, string>();
            int last = (args.Length / 2 - 1) * 2; // last valid to check

            for (int i = 0; i <= last; i += 2)
            {
                string key = required.FirstOrDefault(
                    p => p.Value.Matches(args[i])).Key;

                if (key == null || result.ContainsKey(key))
                    continue;

                result[key] = args[i + 1];
                if (result.Keys.Count == required.Count)
                    break;
            }

            return result;
        }
    }
}
