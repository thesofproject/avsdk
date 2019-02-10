using NUmcSerializer;
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
        static readonly string appName = AppDomain.CurrentDomain.FriendlyName;
        static readonly string[] options = new[]
        {
            "--config",
            "--tplg-conf",
        };

        static int Main(string[] args)
        {
            if (args.Contains("--help"))
            {
                ShowHelp();
                return 0;
            }
            else if (args.Contains("--version"))
            {
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                Console.WriteLine($"Intel Topology Tool, version {version}");
                return 0;
            }

            var dictionary = ParseArguments(args);
            if (!dictionary.ContainsKey("--config") ||
                !dictionary.ContainsKey("--tplg-conf"))
            {
                Console.WriteLine($"Please specify --config and --tplg-conf arguments.");
                ShowShortHelp();
                return 1;
            }

            try
            {
                System system;
                var deserializer = new XmlSerializer(typeof(System));
                using (var stream = new FileStream(dictionary["--config"], FileMode.Open))
                {
                    system = (System)deserializer.Deserialize(stream);
                }

                var serializer = new UmcSerializer();
                var sections = new UmcConverter(system).GetAllSections();
                using (var stream = new FileStream(dictionary["--tplg-conf"], FileMode.Create))
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
            Console.WriteLine($"Usage: {appName} --config <input> --tplg-conf <output> [OPTIONS]");
            Console.WriteLine($"   or: {appName} --help");
            Console.WriteLine($"   or: {appName} --version");
            Console.WriteLine();
            Console.WriteLine("\t--config <path>       Path to XML document to convert");
            Console.WriteLine("\t--tplg-conf <path>    Path of output file to create");
            Console.WriteLine("\t--help                Show this message and exit");
            Console.WriteLine("\t--version             Output version information and exit");
        }

        static Dictionary<string, string> ParseArguments(string[] args)
        {
            var result = new Dictionary<string, string>();
            int last = (args.Length / 2 - 1) * 2; // last valid to check

            for (int i = 0; i <= last; i += 2)
            {
                int index = Array.IndexOf(options, args[i]);
                if (index == -1 || result.ContainsKey(options[index]))
                    continue;

                result[options[index]] = args[i + 1];
                if (result.Keys.Count == options.Length)
                    break;
            }

            return result;
        }
    }
}
