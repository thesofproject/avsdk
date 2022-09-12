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
            var reader = new BinaryReader(new FileStream(input, FileMode.Open, FileAccess.Read),
                                          System.Text.Encoding.ASCII);

            NHLT table = NHLT.ReadFromBinary(reader);
            reader.Close();
            NhltXml xtable = NhltXml.FromNative(table);

            if (parseBlob)
                foreach (var endpoint in xtable.EndpointDescriptors)
                    foreach (var format in endpoint.FormatsConfiguration)
                        format.FormatConfiguration.ParseBlob((LINK_TYPE)endpoint.LinkType);

            var xs = new XmlSerializer(typeof(NhltXml));
            TextWriter writer = new StreamWriter(output);

            xs.Serialize(writer, xtable);

            writer.Close();
        }

        private static void Compile(string input, string output)
        {
            var xs = new XmlSerializer(typeof(NhltXml));

            // To preserve whitespaces in header
            XmlReaderSettings settings = new XmlReaderSettings()
            {
                IgnoreWhitespace = false,
            };
            var reader = XmlReader.Create(new StreamReader(input), settings);

            var xtable = (NhltXml)xs.Deserialize(reader);
            reader.Close();

            var writer = new BinaryWriter(new FileStream(output, FileMode.Create));
            NHLT table = xtable.ToNative();

            table.WriteToBinary(writer);
            writer.Close();
        }
    }
}
