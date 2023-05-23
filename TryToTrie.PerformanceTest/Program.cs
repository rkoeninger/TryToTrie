using System.Diagnostics;
using TryToTrie;

var trie = Trie.Build(TestData.SimpleDict);
var keys = TestData.SimpleDict.Keys.ToList();
var r = new Random();
var keySequence = Enumerable.Range(0, 100000).Select(_ => keys[r.Next(keys.Count)]).ToList();
var stopwatch = Stopwatch.StartNew();

foreach (var k in keySequence)
{
    TestData.SimpleDict.TryGetValue(k, out int _);
}

Console.WriteLine("Dict: " + stopwatch.Elapsed);
stopwatch.Restart();

foreach (var k in keySequence)
{
    trie.Get(k);
}

Console.WriteLine("Trie: " + stopwatch.Elapsed);
stopwatch.Restart();

foreach (var k in keySequence)
{
    TestData.SimpleDict.TryGetValue(k, out int _);
}

Console.WriteLine("Dict: " + stopwatch.Elapsed);
stopwatch.Restart();

foreach (var k in keySequence)
{
    trie.Get(k);
}

Console.WriteLine("Trie: " + stopwatch.Elapsed);
stopwatch.Restart();

foreach (var k in keySequence)
{
    TestData.SimpleDict.TryGetValue(k, out int _);
}

Console.WriteLine("Dict: " + stopwatch.Elapsed);
stopwatch.Restart();

foreach (var k in keySequence)
{
    trie.Get(k);
}

Console.WriteLine("Trie: " + stopwatch.Elapsed);
