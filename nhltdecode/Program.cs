using System;
using System.Xml.Serialization;
using System.IO;

namespace nhltdecode
{
    class Program
    {
        static void Main(string[] args)
        {
            string input = args[0];

            var reader = new BinaryReader(new FileStream(input, FileMode.Open, FileAccess.Read),
                                              System.Text.Encoding.ASCII);
            var table = new NHLT();
            table.ReadFromBinary(reader);
            reader.Close();
        }
    }
}
