// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="EquatableByValue.cs">
// //     Copyright 2017
// //           Thomas PIERRAIN (@tpierrain)    
// //     Licensed under the Apache License, Version 2.0 (the "License");
// //     you may not use this file except in compliance with the License.
// //     You may obtain a copy of the License at
// //         http://www.apache.org/licenses/LICENSE-2.0
// //     Unless required by applicable law or agreed to in writing, software
// //     distributed under the License is distributed on an "AS IS" BASIS,
// //     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// //     See the License for the specific language governing permissions and
// //     limitations under the License.b 
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Value
{
    /// <summary>
    ///     A dictionary with equality based on its content and not on the dictionary's reference
    ///     (i.e.: 2 different instances containing the same entries will be equals).
    /// </summary>
    /// <remarks>This type is not thread-safe (for hashcode updates).</remarks>
    /// <typeparam name="K">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="V">The type of values in the dictionary.</typeparam>
    public class DictionaryByValue<K, V> : EquatableByValueWithoutOrder<DictionaryByValue<K, V>>, IDictionary<K, V>
    {
        private readonly IDictionary<K, V> dictionary;

        public DictionaryByValue(IDictionary<K, V> dictionary)
        {
            this.dictionary = dictionary;
        }

        IEnumerator IEnumerable.GetEnumerator() => dictionary.GetEnumerator();

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator() => dictionary.GetEnumerator();

        public void Add(KeyValuePair<K, V> item)
        {
            ResetHashCode();
            dictionary.Add(item);
        }

        public void Clear()
        {
            ResetHashCode();
            dictionary.Clear();
        }

        public bool Contains(KeyValuePair<K, V> item) => dictionary.Contains(item);

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            dictionary.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<K, V> item)
        {
            ResetHashCode();
            return dictionary.Remove(item);
        }

        public int Count => dictionary.Count;

        public bool IsReadOnly => dictionary.IsReadOnly;

        public bool ContainsKey(K key) => dictionary.ContainsKey(key);

        public void Add(K key, V value)
        {
            ResetHashCode();
            dictionary.Add(key, value);
        }

        public bool Remove(K key)
        {
            ResetHashCode();
            return dictionary.Remove(key);
        }

        public bool TryGetValue(K key, out V value) => dictionary.TryGetValue(key, out value);

        public V this[K key]
        {
            get => dictionary[key];
            set
            {
                ResetHashCode();
                dictionary[key] = value;
            }
        }

        public ICollection<K> Keys => dictionary.Keys;

        public ICollection<V> Values => dictionary.Values;

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            foreach (var kv in dictionary)
            {
                yield return kv;
            }
        }

        protected override bool EqualsWithoutOrderImpl(EquatableByValueWithoutOrder<DictionaryByValue<K, V>> obj)
        {
            var other = obj as DictionaryByValue<K, V>;
            if (other == null)
            {
                return false;
            }

            return !dictionary.Except(other).Any();
        }
    }
}
