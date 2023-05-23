namespace TryToTrie
{
    public static class TestData
    {
        public static readonly IReadOnlyDictionary<string, int> SimpleDict = new Dictionary<string, int>
        {
            { "", -1 },
            { "to", 7 },
            { "tea", 3 },
            { "ted", 4 },
            { "ten", 12 },
            { "A", 15 },
            { "Ali", 2 },
            { "Alien", 21 },
            { "i", 11 },
            { "in", 5 },
            { "inn", 9 }
        };
    }
}
