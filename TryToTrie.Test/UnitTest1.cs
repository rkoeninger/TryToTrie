using NUnit.Framework;

namespace TryToTrie.Tests
{
    public class UnitTest1
    {
        [Test]
        public void Test1()
        {
            var t = Trie.Build(testDict);
            Assert.That(t.Get("ted"), Is.EqualTo(4));
            Assert.That(t.Get("A"), Is.EqualTo(15));
        }

        private readonly IReadOnlyDictionary<string, int> testDict = new Dictionary<string, int>
        {
            { "to", 7 },
            { "tea", 3 },
            { "ted", 4 },
            { "ten", 12 },
            { "A", 15 },
            { "i", 11 },
            { "in", 5 },
            { "inn", 9 }
        };
    }
}
