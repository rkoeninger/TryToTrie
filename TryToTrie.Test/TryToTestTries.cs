using NUnit.Framework;

namespace TryToTrie.Tests
{
    public class TryToTestTries
    {
        [Test]
        public void Test1()
        {
            var t = Trie.Build(TestData.SimpleDict);
            Assert.Multiple(() =>
            {
                Assert.That(t.Get(""), Is.EqualTo(-1));
                Assert.That(t.Get("A"), Is.EqualTo(15));
                Assert.That(t.Get("ted"), Is.EqualTo(4));
                Assert.Throws<KeyNotFoundException>(() => t.Get("teddy"));
            });
        }
    }
}
