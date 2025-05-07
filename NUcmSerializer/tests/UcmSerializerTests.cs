using System;
using System.IO;
using System.Text;
using NUcmSerializer;
using Xunit;

namespace NUcmSerializerTests
{
    public class UcmSerializerTests
    {
        UcmSerializer serializer;

        public UcmSerializerTests()
        {
            serializer = new UcmSerializer();
        }

        [Fact]
        public void TestSerialize()
        {
            byte[] buf = new byte[256];
            var sections = new Section[]
            {
                new SectionGraph(),
            };

            using (var stream = new MemoryStream(buf))
            {
                serializer.Serialize(stream, sections, Encoding.UTF8);
            }
            using (var stream = new MemoryStream(buf))
            {
                serializer.Serialize(stream, sections, null);
            }
            using (var stream = new MemoryStream(buf))
            {
                serializer.Serialize(stream, sections);
            }
            using (var stream = new MemoryStream(buf))
            {
                Assert.Throws<ArgumentNullException>(() => serializer.Serialize(stream, null));
            }
            using (var stream = new MemoryStream(buf))
            {
                Assert.Throws<ArgumentNullException>(() => serializer.Serialize(stream, new Section[] { null }));
            }
            Assert.Throws<ArgumentNullException>(() => serializer.Serialize(null, sections));
        }

        [Fact]
        public void TestDeserialize()
        {
            string s = @"
SectionGraph.""Graph"" {
    index ""0""

    lines [
        ""sink, , source""
    ]
}";
            byte[] buf = Encoding.UTF8.GetBytes(s);

            using (var stream = new MemoryStream(buf))
            {
                serializer.Deserialize(stream, Encoding.UTF8);
            }
            using (var stream = new MemoryStream(buf))
            {
                serializer.Deserialize(stream, null);
            }
            using (var stream = new MemoryStream(buf))
            {
                serializer.Deserialize(stream);
            }
            Assert.Throws<ArgumentNullException>(() => serializer.Deserialize(null));
        }
    }
}
