using kOS.Safe.Encapsulation.Suffixes;
using kOS.Safe.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace kOS.Safe.Encapsulation
{
    public class Lexicon<T, TU> : Structure, IDictionary<T, TU>, IIndexable, IDumper
    {
        public class LexiconComparer<TI> : IEqualityComparer<TI>
        {
            public bool Equals(TI x, TI y)
            {
                if (x == null || y == null)
                {
                    return false;
                }

                if (x.GetType() != y.GetType())
                {
                    return false;
                }

                if (x is string && y is string)
                {
                    var compare = string.Compare(x.ToString(), y.ToString(), StringComparison.InvariantCultureIgnoreCase);
                    return compare == 0;
                }

                return x.Equals(y);
            }

            public int GetHashCode(TI obj)
            {
                if (obj is string)
                {
                    return obj.ToString().ToLower().GetHashCode();
                }
                return obj.GetHashCode();
            }
        }

        private IDictionary<T, TU> internalDictionary;
        private bool caseSensitive;
        private const int INDENT_SPACES = 2;

        public Lexicon()
        {
            internalDictionary = new Dictionary<T, TU>(new LexiconComparer<T>());
            caseSensitive = false;
            InitalizeSuffixes();
        }

        private Lexicon(IEnumerable<KeyValuePair<T, TU>> lexicon)
            : this()
        {
            foreach (var u in lexicon)
            {
                internalDictionary.Add(u);
            }
        }

        private void InitalizeSuffixes()
        {
            AddSuffix("CLEAR", new NoArgsSuffix(Clear, "Removes all items from Lexicon"));
            AddSuffix("KEYS", new Suffix<ListValue<object>>(GetKeys, "Returns the lexicon keys"));
            AddSuffix("HASKEY", new OneArgsSuffix<bool, object>(HasKey, "Returns true if a key is in the Lexicon"));
            AddSuffix("HASVALUE", new OneArgsSuffix<bool, object>(HasValue, "Returns true if value is in the Lexicon"));
            AddSuffix("VALUES", new Suffix<ListValue<object>>(GetValues, "Returns the lexicon values"));
            AddSuffix("COPY", new NoArgsSuffix<Lexicon<T, TU>>(() => new Lexicon<T, TU>(this), "Returns a copy of Lexicon"));
            AddSuffix("LENGTH", new NoArgsSuffix<int>(() => internalDictionary.Count, "Returns the number of elements in the collection"));
            AddSuffix("REMOVE", new OneArgsSuffix<bool, object>(one => Remove((T)one), "Removes the value at the given key"));
            AddSuffix("ADD", new TwoArgsSuffix<object, object>((one, two) => Add((T)one, (TU)two), "Adds a new item to the lexicon, will error if the key already exists"));
            AddSuffix("DUMP", new NoArgsSuffix<string>(() => string.Join(Environment.NewLine, Dump(99)), "Serializes the collection to a string for printing"));
            AddSuffix(new[] { "CASESENSITIVE", "CASE" }, new SetSuffix<bool>(() => caseSensitive, SetCaseSensitivity, "Lets you get/set the case sensitivity on the collection, changing sensitivity will clear the collection"));
        }

        private void SetCaseSensitivity(bool newCase)
        {
            if (newCase == caseSensitive)
            {
                return;
            }
            caseSensitive = newCase;

            internalDictionary = newCase ?
                new Dictionary<T, TU>() :
                new Dictionary<T, TU>(new LexiconComparer<T>());
        }

        private bool HasValue(object value)
        {
            return internalDictionary.Values.Contains((TU)value);
        }

        private bool HasKey(object key)
        {
            return internalDictionary.ContainsKey((T)key);
        }

        public ListValue<object> GetValues()
        {
            return ListValue.CreateList(Values);
        }

        public ListValue<object> GetKeys()
        {
            return ListValue.CreateList(Keys);
        }

        public IEnumerator<KeyValuePair<T, TU>> GetEnumerator()
        {
            return internalDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<T, TU> item)
        {
            if (internalDictionary.ContainsKey(item.Key))
            {
                throw new KOSDuplicateKeyException(item.Key.ToString(), caseSensitive);
            }
            internalDictionary.Add(item);
        }

        public void Clear()
        {
            internalDictionary.Clear();
        }

        public bool Contains(KeyValuePair<T, TU> item)
        {
            return internalDictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<T, TU>[] array, int arrayIndex)
        {
            internalDictionary.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<T, TU> item)
        {
            return internalDictionary.Remove(item);
        }

        public int Count
        {
            get { return internalDictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return internalDictionary.IsReadOnly; }
        }

        public bool ContainsKey(T key)
        {
            return internalDictionary.ContainsKey(key);
        }

        public void Add(T key, TU value)
        {
            if (internalDictionary.ContainsKey(key))
            {
                throw new KOSDuplicateKeyException(key.ToString(), caseSensitive);
            }
            internalDictionary.Add(key, value);
        }

        public bool Remove(T key)
        {
            return internalDictionary.Remove(key);
        }

        public bool TryGetValue(T key, out TU value)
        {
            return internalDictionary.TryGetValue(key, out value);
        }

        public TU this[T key]
        {
            get
            {
                if (internalDictionary.ContainsKey(key))
                {
                    return internalDictionary[key];
                }
                throw new KOSKeyNotFoundException(key.ToString(), caseSensitive);
            }
            set
            {
                internalDictionary[key] = value;
            }
        }

        public ICollection<T> Keys
        {
            get
            {
                return internalDictionary.Keys;
            }
        }

        public ICollection<TU> Values
        {
            get
            {
                return internalDictionary.Values;
            }
        }

        public object GetIndex(object key)
        {
            T castKey;
            if (key is T)
            {
                castKey = (T)key;
            }
            else
            {
                throw new KOSInvalidArgumentException("LexiconIndexer", "Index", key + " was invalid");
            }

            if (!ContainsKey(castKey))
            {
                throw new KOSKeyNotFoundException(key.ToString(), caseSensitive);
            }
            return internalDictionary[(T)key];
        }

        public void SetIndex(object index, object value)
        {
            if (index is T)
            {
                internalDictionary[(T)index] = (TU)value;
            }
            else
            {
                throw new KOSInvalidArgumentException("LexiconIndexer", "Index", string.Format("{0} is an invalid type: {1} Expected: {2}", index, index.GetType(), typeof(T)));
            }
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, Dump(1));
        }

        public string[] Dump(int limit, int depth = 0)
        {
            var toReturn = new List<string>();

            var listString = string.Format("LEXICON of {0} items", Count);
            toReturn.Add(listString);

            if (limit <= 0) return toReturn.ToArray();

            var keys = internalDictionary.Keys.ToList();
            foreach (var key in keys)
            {
                var item = internalDictionary[key];

                var dumper = item as IDumper;
                if (dumper != null)
                {
                    var entry = string.Empty.PadLeft(depth * INDENT_SPACES);

                    var itemDump = dumper.Dump(limit - 1, depth + 1);

                    var itemString = string.Format("  [\"{0}\"]= {1}", key, itemDump[0]);
                    entry += itemString;

                    toReturn.Add(entry);

                    for (int i = 1; i < itemDump.Length; i++)
                    {
                        var subEntry = string.Format("{0}", itemDump[i]);
                        toReturn.Add(subEntry);
                    }
                }
                else
                {
                    var entry = string.Empty.PadLeft(depth * INDENT_SPACES);

                    if (key is double || key is int || key is float)
                    {
                        entry += string.Format("  [{0}]= {1}", key, item);
                    }
                    else
                    {
                        entry += string.Format("  [\"{0}\"]= {1}", key, item);
                    }

                    toReturn.Add(entry);
                }
            }
            return toReturn.ToArray();
        }
    }
}