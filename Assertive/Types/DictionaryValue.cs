using System.Collections.Specialized;
using System.Web;

namespace Assertive.Types
{
    /// <summary>
    /// Dictionary where keys do NOT have to be unique
    /// </summary>
    internal class DictionaryValue : Value
    {
        private List<DictionaryEntry> _entries = new();

        public void AddEntry(DictionaryEntry entry)
        {
            _entries.Add(entry);
        }

        public void RemoveEntries(Value key)
        {
          _entries.RemoveAll(x => x.Key.ToString() == key.ToString());
        }

        public Value GetEntry(Value key)
        {
            return _entries.First(x => x.Key.ToString() == key.ToString()).Value;
        }

        public bool ContainsKey(Value key)
        {
            return _entries.Any(x => x.Key.ToString() == key.ToString());
        }

        public List<DictionaryEntry> GetEntries()
        {
            return _entries;
        }

        public override string ToString()
        {
            return "{" + string.Join(',', _entries) + "}";
        }

        public IEnumerable<KeyValuePair<string, string>> ToKeyValuePairs()
        {
            var kvpList = new List<KeyValuePair<string, string>>();
            foreach (DictionaryEntry entry in _entries)
            {
                kvpList.Add(new KeyValuePair<string, string>(entry.Key.ToString()!, entry.Value.ToString()!));
            }
            return kvpList;
        }

    }
}
