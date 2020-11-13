using System;
using System.Xml.Serialization;
using System.IO;

namespace nhltdecode
{
    class Program
    {
        static void Main(string[] args)
        {
            string input = args[0], output = args[1];

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
    }
}
