using System.IO;

namespace nhltdecode
{
    internal static class ExtensionMethods
    {
        internal static byte CalculateChecksum(this NHLT table)
        {
            var writer = new BinaryWriter(new MemoryStream());
            int checksum = 0;

            table.WriteToBinary(writer);
            writer.BaseStream.Seek(0, SeekOrigin.Begin);

            while (true)
            {
                int oneByte = writer.BaseStream.ReadByte();
                if (oneByte < 0)
                    break;
                checksum += oneByte;
            }

            writer.Close();
            return (byte)(256 - checksum + table.Header.Checksum);
        }

        internal static uint PopCount(uint i)
        {
            return (i & 0x01) + ((i >> 1) & 0x01);
        }
    }
}
