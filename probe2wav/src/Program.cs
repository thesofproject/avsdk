//
// Copyright (c) 2020-2022, Intel Corporation. All rights reserved.
//
// Author: Piotr Maziarz <piotrx.maziarz@linux.intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System;

namespace probe2wav
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintHelp();
                return 0;
            }

            string input = "";
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
                else
                {
                    input = args[i];
                }
            }
            ProbeExtractor extractor = new ProbeExtractor(verbose, input, ignoreChecksum);

            try
            {
                extractor.Convert();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"probe2wav failed. Message: {ex.Message}");
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

        static private void PrintHelp()
        {
            Console.WriteLine("probe2wav");
            Console.WriteLine("Usage: probe2wav [Options][File]");
            Console.WriteLine("Options:");
            Console.WriteLine("-h, --help         help");
            Console.WriteLine("-v, --verbose      show verbose output");
            Console.WriteLine("--ignore-checksum  don't discard data when checksum is incorrect");
            Console.WriteLine("                   Checksum mismatch will still be displayed in verbose mode");
        }
    }
}
