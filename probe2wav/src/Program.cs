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

namespace probe2wav
{
    class Program
    {
        static void PrintHelp()
        {
            Console.WriteLine("Usage: probe2wav [OPTIONS] [FILE]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("   -h, --help\t\t\tprint this message");
            Console.WriteLine("   -v, --verbose\t\tshow verbose output");
            Console.WriteLine("       --ignore-checksum\tdo not discard data when checksum is incorrect");
            Console.WriteLine("\t\t\t\tchecksum mismatch will still be displayed in verbose mode");
        }

        static int Main(string[] args)
        {
            string filename = null;
            bool verbose = false;
            bool ignoreChecksum = false;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("-h") || args[i].Equals("--help"))
                {
                    PrintHelp();
                    return 0;
                }
                if (args[i].Equals("-v") || args[i].Equals("--verbose"))
                {
                    verbose = true;
                }
                else if (args[i].Equals("--ignore-checksum"))
                {
                    ignoreChecksum = true;
                }
                else if (!args[i].StartsWith("-") && i == args.Length - 1)
                {
                    filename = args[i];
                }
                else
                {
                    Console.WriteLine($"Unknown option: \"{args[i]}\".");
                    PrintHelp();
                    return 1;
                }
            }

            if (String.IsNullOrWhiteSpace(filename))
            {
                PrintHelp();
                return 0;
            }
            if (!File.Exists(filename))
            {
                Console.WriteLine($"File \"{filename}\" does not exist.");
                PrintHelp();
                return 1;
            }

            try
            {
                DataExtractor.Extract(filename, verbose, ignoreChecksum);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"probe2wav failed. {ex.GetType()}: {ex.Message}");
                return 1;
            }

            return 0;
        }
    }
}
