using System.Collections;
using System.Collections.Generic;

namespace Fixup
{
    public class DictionaryList<K, V>
    {
        private readonly Dictionary<K, List<V>> _Values = new Dictionary<K, List<V>>();
        public bool IsPopulated => _Values != null && _Values.Count > 0;

        public void Add(K key, V value)
        {
            if (_Values.ContainsKey(key))
                _Values[key].Add(value);
            else
                _Values[key] = new List<V> { value };
        }

        public void AddRange(K key, List<V> value)
        {
            if (_Values.ContainsKey(key))
                _Values[key].AddRange(value);
            else
                _Values[key] = new List<V>(value);
        }

        public List<V> ValueOrEmptyList(K key)
        {
            return _Values.ContainsKey(key) ? _Values[key] : new List<V>();
        }

        public List<V> this[K key]
        {
            get { return _Values[key]; }
            set { _Values[key] = value; }
        }

        public IEnumerator<KeyValuePair<K, List<V>>> GetEnumerator()
        {
            return _Values.GetEnumerator();
        }
    }
}