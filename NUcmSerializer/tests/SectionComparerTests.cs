using NUcmSerializer;
using Xunit;

namespace NUcmSerializerTests
{
    public class SectionComparerTests
    {
        SectionComparer comparer;

        public SectionComparerTests()
        {
            comparer = new SectionComparer();
        }

        [Fact]
        public void TestNullComparison()
        {
            var section = new SectionData();

            Assert.False(comparer.Equals(null, section));
            Assert.False(comparer.Equals(section, null));
            Assert.True(comparer.Equals(null, null));
        }

        [Fact]
        public void TestSameTypeComparison()
        {
            var section1 = new SectionData(string.Empty);
            var section2 = new SectionData();

            Assert.True(comparer.Equals(section1, section1));
            Assert.False(comparer.Equals(section1, section2));
        }

        [Fact]
        public void TestDistinctTypeComparison()
        {
            var section1 = new SectionCC(string.Empty);
            var section2 = new SectionData();

            Assert.False(comparer.Equals(null, section1));
            Assert.False(comparer.Equals(section1, null));

            Assert.True(comparer.Equals(null, null));
            Assert.True(comparer.Equals(section1, section1));
            Assert.False(comparer.Equals(section1, section2));
        }

        [Fact]
        public void TestHashCode()
        {
            var section1 = new SectionCC(string.Empty);
            var section2 = new SectionData();

            Assert.StrictEqual(0, comparer.GetHashCode(null));
            Assert.NotStrictEqual(0, comparer.GetHashCode(section1));
            Assert.NotStrictEqual(0, comparer.GetHashCode(section2));
        }
    }
}
