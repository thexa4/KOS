using kOS.Safe.Encapsulation.Suffixes;
using kOS.Safe.Exceptions;
using kOS.Safe.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace kOS.Safe.Encapsulation
{
    public class ListValue<T> : Structure, IList<T>, IIndexable, IDumper
    {
        private readonly IList<T> internalList;
        private const int INDENT_SPACES = 2;

        public ListValue()
            : this(new List<T>())
        {
        }

        public ListValue(IEnumerable<T> listValue)
        {
            internalList = listValue.ToList();
            ListInitializeSuffixes();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return internalList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            internalList.Add(item);
        }

        public void Clear()
        {
            internalList.Clear();
        }

        public bool Contains(T item)
        {
            return internalList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            internalList.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return internalList.Remove(item);
        }

        public int Count
        {
            get { return internalList.Count; }
        }

        public bool IsReadOnly
        {
            get { return internalList.IsReadOnly; }
        }

        public int IndexOf(T item)
        {
            return internalList.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            internalList.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            internalList.RemoveAt(index);
        }

        public T this[int index]
        {
            get { return internalList[index]; }
            set { internalList[index] = value; }
        }

        private void ListInitializeSuffixes()
        {
            AddSuffix("ADD",      new OneArgsSuffix<T>                  (toAdd => internalList.Add(toAdd), Resources.ListAddDescription));
            AddSuffix("INSERT",   new TwoArgsSuffix<int, T>             ((index, toAdd) => internalList.Insert(index, toAdd)));
            AddSuffix("REMOVE",   new OneArgsSuffix<int>                (toRemove => internalList.RemoveAt(toRemove)));
            AddSuffix("CLEAR",    new NoArgsSuffix                      (() => internalList.Clear()));
            AddSuffix("LENGTH",   new NoArgsSuffix<int>                 (() => internalList.Count));
            AddSuffix("ITERATOR", new NoArgsSuffix<Enumerator>          (() => new Enumerator(internalList.GetEnumerator())));
            AddSuffix("COPY",     new NoArgsSuffix<ListValue<T>>        (() => new ListValue<T>(this)));
            AddSuffix("CONTAINS", new OneArgsSuffix<bool, T>            (item => internalList.Contains(item)));
            AddSuffix("SUBLIST",  new TwoArgsSuffix<ListValue, int, int>(SubListMethod));
            AddSuffix("EMPTY",    new NoArgsSuffix<bool>                (() => !internalList.Any()));
            AddSuffix("DUMP",     new NoArgsSuffix<string>              (() => string.Join(Environment.NewLine, Dump(99))));
        }

        // This test case was added to ensure there was an example method with more than 1 argument.
        private ListValue SubListMethod(int start, int runLength)
        {
            var subList = new ListValue();
            for (int i = start; i < internalList.Count && i < start + runLength; ++i)
            {
                subList.Add(internalList[i]);
            }
            return subList;
        }

        public override bool SetSuffix(string suffixName, object value)
        {
            //These were deprecated in v0.15. Text here it to assist in upgrading scripts
            switch (suffixName)
            {
                case "ADD":
                    throw new Exception("Old syntax \n" +
                                           "   SET _somelist_:ADD TO _value_\n" +
                                           "is no longer supported. Try replacing it with: \n" +
                                           "   _somelist_:ADD(_value_).\n");
                case "CONTAINS":
                    throw new Exception("Old syntax \n" +
                                           "   SET _somelist_:CONTAINS TO _value_\n" +
                                           "is no longer supported. Try replacing it with: \n" +
                                           "   SET test TO _somelist_:CONTAINS(_value_)\n");
                case "REMOVE":
                    throw new Exception("Old syntax \n" +
                                           "   SET _somelist_:REMOVE TO _number_\n" +
                                           "is no longer supported. Try replacing it with: \n" +
                                           "   _somelist_:REMOVE(_number_).\n");
                default:
                    return false;
            }
        }

        public static ListValue<T> CreateList<TU>(IEnumerable<TU> list)
        {
            return new ListValue<T>(list.Cast<T>());
        }

        public object GetIndex(object index)
        {
            if (index is double || index is float)
            {
                index = Convert.ToInt32(index);  // allow expressions like (1.0) to be indexes
            }
            if (!(index is int)) throw new Exception("The index must be an integer number");

            return internalList[(int)index];
        }

        public void SetIndex(object index, object value)
        {
            if (index is double || index is float)
            {
                index = Convert.ToInt32(index);  // allow expressions like (1.0) to be indexes
            }

            if (!(index is int)) throw new KOSException("The index must be an integer number");

            internalList[(int)index] = (T)value;
        }

        public string[] Dump(int limit, int depth = 0)
        {
            var toReturn = new List<string>();

            var listString = string.Format("LIST of {0} items", Count);
            toReturn.Add(listString);

            if (limit <= 0) return toReturn.ToArray();

            for (int index = 0; index < internalList.Count; index++)
            {
                var item = internalList[index];

                var dumper = item as IDumper;
                if (dumper != null)
                {
                    var entry = string.Empty.PadLeft(depth * INDENT_SPACES);

                    var itemDump = dumper.Dump(limit - 1, depth + 1);

                    var itemString = string.Format("  [{0,2}]= {1}", index, itemDump[0]);
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
                    entry += string.Format("  [{0,2}]= {1}", index, item);
                    toReturn.Add(entry); 
                }
            }
            return toReturn.ToArray();
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, Dump(1));
        }
    }

    public class ListValue : ListValue<object>
    {
        public ListValue()
        {
            InitializeSuffixes();
        }

        public ListValue(IEnumerable<object> toCopy)
            : base(toCopy)
        {
            InitializeSuffixes();
        }

        private void InitializeSuffixes()
        {
            AddSuffix("COPY", new NoArgsSuffix<ListValue>(() => new ListValue(this)));
        }

        public new static ListValue CreateList<T>(IEnumerable<T> toCopy)
        {
            return new ListValue(toCopy.Cast<object>());
        }
    }
}