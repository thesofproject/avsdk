using System;
using System.Xml.Serialization;
using System.IO;

namespace nhltdecode
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintHelp();
                return;
            }

            string input = "", output = "";
            bool decode = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("-h") || args[i].Equals("--help"))
                {
                    PrintHelp();
                    return;
                }
                if ((args[i].Equals("-c") || args[i].Equals("--compile")) && (i + 1) < args.Length)
                {
                    input = args[i + 1];
                    i++;
                }
                else if ((args[i].Equals("-d") || args[i].Equals("--decode")) && (i + 1) < args.Length)
                {
                    input = args[i + 1];
                    i++;
                    decode = true;
                }
                else if ((args[i].Equals("-o") || args[i].Equals("--output")) && (i + 1) < args.Length)
                {
                    output = args[i + 1];
                    i++;
                }
            }

            if (input.Length == 0 || output.Length == 0)
            {
                PrintHelp();
                Console.WriteLine("Please specify input and output files");
                return;
            }

            if (decode)
            {
                var reader = new BinaryReader(new FileStream(input, FileMode.Open, FileAccess.Read),
                                              System.Text.Encoding.ASCII);
                var table = new NHLT();
                table.ReadFromBinary(reader);
                reader.Close();

                var xtable = NhltXml.FromNative(table);

                var xs = new XmlSerializer(typeof(NhltXml));
                TextWriter writer = new StreamWriter(output);

                xs.Serialize(writer, xtable);

                writer.Close();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        static private void PrintHelp()
        {
            Console.WriteLine("nhltdecode");
            Console.WriteLine("-h, --help              help");
            Console.WriteLine("-c, --compile=FILE      compile XML file");
            Console.WriteLine("-d, --decode=FILE       decode NHLT binary file");
            Console.WriteLine("-o, --output=FILE       set output file");
        }
    }
}
