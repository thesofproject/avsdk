using System;
using System.IO;
using NUcmSerializer;
using Xunit;

namespace NUcmSerializerTests
{
    public class UcmWriterTests
    {
        [Fact]
        public void TestConstructor()
        {
            using (var writer = new UcmWriter(new MemoryStream(), System.Text.Encoding.UTF8))
            {
            }
            using (var writer = new UcmWriter(new MemoryStream(), null))
            {
            }
            using (var writer = new UcmWriter(new MemoryStream()))
            {
            }
            Assert.Throws<ArgumentNullException>(() => new UcmWriter(null));
        }

        [Fact]
        public void TestDoubleFlush()
        {
            using (var writer = new UcmWriter(new MemoryStream()))
            {
                // Check if double Flush() does not cause trouble.
                writer.Flush();
                writer.Flush();
            }
        }

        [Fact]
        public void TestDoubleClose()
        {
            using (var writer = new UcmWriter(new MemoryStream()))
            {
                // Check if double Close() does not cause trouble.
                writer.Close();
                writer.Close();
            }
        }

        [Fact]
        public void TestDoubleDispose()
        {
            using (var writer = new UcmWriter(new MemoryStream()))
            {
                // Check if double Dispose() does not cause trouble.
                writer.Dispose();
                writer.Dispose();
            }
        }
    }
}
