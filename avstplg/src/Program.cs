using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using NUcmSerializer;

namespace avstplg
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

        static readonly Option s_input = new Option("-c", "--compile");
        static readonly Option s_output = new Option("-o", "--output");
        static readonly Option s_help = new Option("-h", "--help");
        static readonly Option s_version = new Option("-v", "--version");

        static readonly string s_appName = AppDomain.CurrentDomain.FriendlyName;
        static readonly Dictionary<string, Option> s_requiredOptions = new Dictionary<string, Option>()
        {
            { "input", s_input },
            { "output", s_output },
        };

        static void ShowShortHelp()
        {
            Console.WriteLine($"Try {s_appName} --help for more information.");
        }

        static void ShowHelp()
        {
            Console.WriteLine($"Usage: {s_appName} -c [INPUT] -o [OUTPUT]");
            Console.WriteLine();
            Console.WriteLine($"  {s_input} FILE\tPath to XML document to convert");
            Console.WriteLine($"  {s_output} FILE\tPath to UCM file to create");
            Console.WriteLine($"  {s_help}\t\tShow this message and exit");
            Console.WriteLine($"  {s_version}\t\tOutput version information and exit");
        }

        static Dictionary<string, string> ParseArguments(string[] args)
        {
            var result = new Dictionary<string, string>();
            int last = (args.Length / 2 - 1) * 2; // last valid to check

            for (int i = 0; i <= last; i += 2)
            {
                string key = s_requiredOptions.FirstOrDefault(
                    p => p.Value.Matches(args[i])).Key;

                if (key == null || result.ContainsKey(key))
                    continue;

                result[key] = args[i + 1];
                if (result.Keys.Count == s_requiredOptions.Count)
                    break;
            }

            return result;
        }

        static int Main(string[] args)
        {
            if (args.Any(a => s_help.Matches(a)))
            {
                ShowHelp();
                return 0;
            }
            else if (args.Any(a => s_version.Matches(a)))
            {
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                Console.WriteLine($"Intel AVS topology tool, version {version}");
                return 0;
            }

            Dictionary<string, string> dictionary = ParseArguments(args);
            if (!dictionary.ContainsKey("input") ||
                !dictionary.ContainsKey("output"))
            {
                Console.WriteLine($"Please specify -c and -o arguments.");
                ShowShortHelp();
                return 1;
            }

            try
            {
                Topology topology;
                var deserializer = new XmlSerializer(typeof(Topology));
                using (var stream = new FileStream(dictionary["input"], FileMode.Open))
                {
                    topology = (Topology)deserializer.Deserialize(stream);
                }

                var serializer = new UcmSerializer();
                IEnumerable<Section> sections = SectionProvider.GetTopologySections(topology);
                using (var stream = new FileStream(dictionary["output"], FileMode.Create))
                {
                    serializer.Serialize(stream, sections);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{s_appName} failed. Message: {ex.Message}");
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
    }
}
