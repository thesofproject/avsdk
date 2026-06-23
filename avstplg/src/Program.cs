//
// Copyright (c) 2020-2022, Intel Corporation. All rights reserved.
//
// Author: Cezary Rojewski <cezary.rojewski@intel.com>
//
// SPDX-License-Identifier: Apache-2.0
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
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
        static readonly Option s_xsd = new Option("-x", "--xsd");
        static readonly Option s_help = new Option("-h", "--help");
        static readonly Option s_version = new Option("-v", "--version");

        static readonly string s_appName = AppDomain.CurrentDomain.FriendlyName;
        static readonly Dictionary<string, Option> s_parseOptions = new Dictionary<string, Option>()
        {
            { "input", s_input },
            { "output", s_output },
            { "xsd", s_xsd },
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

        static void ValidateXml(string xmlFilename, string xsdFilename)
        {
            var schemaDeserializer = new XmlSerializer(typeof(XmlSchema));
            XmlSchema schema;

            using (var stream = new FileStream(xsdFilename, FileMode.Open))
                schema = (XmlSchema)schemaDeserializer.Deserialize(stream);

            var settings = new XmlReaderSettings();
            settings.Schemas.Add(schema);
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessIdentityConstraints;
            settings.ValidationEventHandler += new ValidationEventHandler(SchemaValidationEventCallback);

            using (XmlReader reader = XmlReader.Create(xmlFilename, settings))
                while (reader.Read()) ;
        }

        static void Compile(string inputFilename, string outputFilename)
        {
            var deserializer = new XmlSerializer(typeof(Topology));
            Topology topology;

            using (var stream = new FileStream(inputFilename, FileMode.Open))
                topology = (Topology)deserializer.Deserialize(stream);

            var serializer = new UcmSerializer();
            IEnumerable<Section> sections = SectionProvider.GetTopologySections(topology);

            using (var stream = new FileStream(outputFilename, FileMode.Create))
                serializer.Serialize(stream, sections);
        }

        static void SchemaValidationEventCallback(object sender, ValidationEventArgs args)
        {
            Console.WriteLine("Error: " + args.Message);
        }

        static int Main(string[] args)
        {
            if (args.Any(a => s_help.Matches(a)))
            {
                ShowHelp();
                return 0;
            }

            if (args.Any(a => s_version.Matches(a)))
            {
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                Console.WriteLine($"Intel AVS topology tool, version {version}");
                return 0;
            }

            Dictionary<string, string> dictionary = ParseArguments(args);

            if (dictionary == null)
            {
                ShowShortHelp();
                return 1;
            }
            if (!dictionary.ContainsKey("input") || !dictionary.ContainsKey("output"))
            {
                Console.WriteLine($"Please specify -c and -o arguments.");
                ShowShortHelp();
                return 1;
            }

            try
            {
                if (dictionary.ContainsKey("xsd"))
                    ValidateXml(dictionary["input"], dictionary["xsd"]);

                Compile(dictionary["input"], dictionary["output"]);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{s_appName} failed. {ex.GetType()}: {ex.Message}");
                return 1;
            }

            return 0;
        }
    }
}
