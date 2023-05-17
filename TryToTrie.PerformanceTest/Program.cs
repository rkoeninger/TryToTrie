using System.Diagnostics;
using TryToTrie;

var testDict = new Dictionary<string, int>
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
var trie = Trie.Build(testDict);
var keys = testDict.Keys.ToList();
var r = new Random();
var keySequence = Enumerable.Range(0, 100000).Select(_ => keys[r.Next(keys.Count)]).ToList();
var stopwatch = Stopwatch.StartNew();

foreach (var k in keySequence)
{
    testDict.TryGetValue(k, out int _);
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
    testDict.TryGetValue(k, out int _);
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
    testDict.TryGetValue(k, out int _);
}

Console.WriteLine("Dict: " + stopwatch.Elapsed);
stopwatch.Restart();

foreach (var k in keySequence)
{
    trie.Get(k);
}

Console.WriteLine("Trie: " + stopwatch.Elapsed);
