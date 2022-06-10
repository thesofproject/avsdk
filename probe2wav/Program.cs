using System;

namespace ProbeExtractor
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
                else
                {
                    input = args[i];
                }
            }
            WavProbeExtractor extractor = new WavProbeExtractor(verbose, input);

            try
            {
                extractor.ConvertDataToWav();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ProbeExtractor failed. Message: {ex.Message}");
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
            Console.WriteLine("ProbeExtractor");
            Console.WriteLine("Usage: ProbeExtractor [Options][File]");
            Console.WriteLine("Options:");
            Console.WriteLine("-h, --help         help");
            Console.WriteLine("-v, --verbose      show verbose output");
        }
    }
}
