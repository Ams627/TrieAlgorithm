using System;
using System.Collections.Generic;
using System.Text;

namespace TrieAlgorithm
{
    /// <summary>
    /// It's a Trie, but just for Sha1s - so only for strings containing only hex digits
    /// </summary>
    public class ShaTrie
    {
        class TrieNode
        {
            public int Frequency { get; set; }
            public TrieNode[] Next { get; } = new TrieNode[26];
        }

        private readonly TrieNode root = new TrieNode();
        public void Insert(string str)
        {
            _ = str ?? throw new ArgumentException("parameter is null", nameof(str));
            var currentNode = root;
            foreach (var c in str)
            {
                var b = GetHex(c);
                if (c < 0)
                {
                    throw new ArgumentException($"invalid sha1 character in '{str}'- all characters must be valid hex digits");
                }
                if (currentNode.Next[b] == null)
                {
                    currentNode.Next[b] = new TrieNode();
                }
                currentNode.Next[b].Frequency++;
                currentNode = currentNode.Next[b];
            }
        }

        private int GetHex(char c)
        {
            if (c < '0' || c > 'f') return -1;
            if (c > '9' && c < 'A') return -1;
            if (c > 'F' && c < 'a') return -1;
            if (char.IsDigit(c)) return c - '0';
            return char.ToUpper(c) - 'A' + 10;
        }

        public string[] GetShortestPrefix(IEnumerable<string> strings)
        {
            foreach (var s in strings)
            {
                Insert(s);
            }

            var list = new List<string>();

            foreach (var s in strings)
            {
                var sb = new StringBuilder();
                var currentNode = root;
                foreach (var c in s)
                {
                    sb.Append(c);
                    var b = GetHex(c);
                    if (c < 0)
                    {
                        throw new ArgumentException($"invalid sha1 character in '{s}'- all characters must be valid hex digits");
                    }

                    if (currentNode.Next[b].Frequency == 1)
                    {
                        break;
                    }
                    currentNode = currentNode.Next[b];
                }
                list.Add(sb.ToString());
            }

            return list.ToArray();
        }
    }
}
