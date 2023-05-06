using System;
using System.IO;
using NUcmSerializer;
using Xunit;

namespace NUcmSerializerTests
{
    public class UcmReaderTests
    {
        [Fact]
        public void TestConstructor()
        {
            using (var reader = new UcmReader(new MemoryStream(), System.Text.Encoding.UTF8))
            {
            }
            using (var reader = new UcmReader(new MemoryStream(), null))
            {
            }
            using (var reader = new UcmReader(new MemoryStream()))
            {
            }
            Assert.Throws<ArgumentNullException>(() => new UcmReader(null));
        }

        [Fact]
        public void TestDoubleClose()
        {
            using (var reader = new UcmReader(new MemoryStream()))
            {
                // Check if double Close() does not cause trouble.
                reader.Close();
                reader.Close();
            }
        }

        [Fact]
        public void TestDoubleDispose()
        {
            using (var reader = new UcmReader(new MemoryStream()))
            {
                // Check if double Dispose() does not cause trouble.
                reader.Dispose();
                reader.Dispose();
            }
        }
    }
}
