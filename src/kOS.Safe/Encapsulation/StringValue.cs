﻿using System;
using System.Globalization;
using System.Text.RegularExpressions;
using kOS.Safe.Encapsulation.Suffixes;
using kOS.Safe.Exceptions;

namespace kOS.Safe.Encapsulation
{
    /// <summary>
    /// The class is a simple wrapper around the string class to 
    /// implement the Structure and IIndexable interface on
    /// strings. Currently, strings are only boxed with this
    /// class temporarily when suffix/indexing support is
    /// necessary.
    /// 
    /// </summary>
    public class StringValue : Structure, IIndexable
    {
        private readonly string internalString;

        public StringValue(): 
            this (string.Empty)
        {
        }

        public StringValue(string stringValue)
        {
            internalString = stringValue;
            StringInitializeSuffixes();
        }

        public int Length
        {
            get { return internalString.Length; }
        }

        public string Substring(int start, int count)
        {
            return internalString.Substring(start, count);
        }

        public bool Contains(string s)
        {
            return internalString.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public bool EndsWith(string s)
        {
            return internalString.EndsWith(s,true, CultureInfo.CurrentCulture);
        }

        public int IndexOf(string s)
        {
            return internalString.IndexOf(s, StringComparison.OrdinalIgnoreCase);
        }

        // IndexOf with a start position.
        // This was named FindAt because IndexOfAt made little sense.
        public int FindAt(string s, int start)
        {
            return internalString.IndexOf(s, start, StringComparison.OrdinalIgnoreCase);
        }

        public string Insert(int location, string s)
        {
            return internalString.Insert(location, s);
        }

        public int LastIndexOf(string s)
        {
            return internalString.LastIndexOf(s, StringComparison.OrdinalIgnoreCase);
        }

        public int FindLastAt(string s, int start)
        {
            return internalString.LastIndexOf(s, start, StringComparison.OrdinalIgnoreCase);
        }

        public string PadLeft(int width)
        {
            return internalString.PadLeft(width);
        }

        public string PadRight(int width)
        {
            return internalString.PadRight(width);
        }

        public string Remove(int start, int count)
        {
            return internalString.Remove(start, count);
        }

        public string Replace(string oldString, string newString)
        {
            return internalString.Replace(oldString, newString);
        }

        public string ToLower()
        {
            return internalString.ToLower();
        }

        public string ToUpper()
        {
            return internalString.ToUpper();
        }

        public bool StartsWith(string s)
        {
            return internalString.StartsWith(s, true, CultureInfo.CurrentCulture);
        }

        public string Trim()
        {
            return internalString.Trim();
        }

        public string TrimEnd()
        {
            return internalString.TrimEnd();
        }

        public string TrimStart()
        {
            return internalString.TrimStart();
        }

        public object GetIndex(object index)
        {
            if (index is double || index is float)
            {
                index = Convert.ToInt32(index);  // allow expressions like (1.0) to be indexes
            }
            if (!(index is int)) throw new Exception("The index must be an integer number");

            return internalString[(int)index].ToString();
        }

        // Required by the interface but unimplemented, because strings are immutable.
        public void SetIndex(object index, object value)
        {
            throw new KOSException("String are immutable; they can not be modified using the syntax \"SET string[1] TO 'a'\", etc.");
        }

        // As the regular Split, except returning a ListValue rather than an array.
        public ListValue<string> SplitToList(string separator)
        {
            string[] split = Regex.Split(internalString, separator, RegexOptions.IgnoreCase);
            return new ListValue<string>(split);
        }

        private void StringInitializeSuffixes()
        {
            AddSuffix("LENGTH",     new NoArgsSuffix<int>                           (() => Length));
            AddSuffix("SUBSTRING",  new TwoArgsSuffix<string, int, int>             (Substring));
            AddSuffix("CONTAINS",   new OneArgsSuffix<bool, string>                 (Contains));
            AddSuffix("ENDSWITH",   new OneArgsSuffix<bool, string>                 (EndsWith));
            AddSuffix("FINDAT",     new TwoArgsSuffix<int, string, int>             (FindAt));
            AddSuffix("INSERT",     new TwoArgsSuffix<string, int, string>          (Insert));
            AddSuffix("FINDLASTAT", new TwoArgsSuffix<int, string, int>             (FindLastAt));
            AddSuffix("PADLEFT",    new OneArgsSuffix<string, int>                  (PadLeft));
            AddSuffix("PADRIGHT",   new OneArgsSuffix<string, int>                  (PadRight));
            AddSuffix("REMOVE",     new TwoArgsSuffix<string, int, int>             (Remove));
            AddSuffix("REPLACE",    new TwoArgsSuffix<string, string, string>       (Replace));
            AddSuffix("SPLIT",      new OneArgsSuffix<ListValue<string>, string>    (SplitToList));
            AddSuffix("STARTSWITH", new OneArgsSuffix<bool, string>                 (StartsWith));
            AddSuffix("TOLOWER",    new NoArgsSuffix<string>                        (ToLower));
            AddSuffix("TOUPPER",    new NoArgsSuffix<string>                        (ToUpper));
            AddSuffix("TRIM",       new NoArgsSuffix<string>                        (Trim));
            AddSuffix("TRIMEND",    new NoArgsSuffix<string>                        (TrimEnd));
            AddSuffix("TRIMSTART",  new NoArgsSuffix<string>                        (TrimStart));

            // Aliased "IndexOf" with "Find" to match "FindAt" (since IndexOfAt doesn't make sense, but I wanted to stick with common/C# names when possible)
            AddSuffix(new[] { "INDEXOF",     "FIND" },     new OneArgsSuffix<int, string>   (IndexOf));
            AddSuffix(new[] { "LASTINDEXOF", "FINDLAST" }, new OneArgsSuffix<int, string>   (LastIndexOf));

        }

        // Implicitly converts to a string (i.e., unboxes itself automatically)
        public static implicit operator string(StringValue value)
        {
            return value.internalString;
        }

        public override string ToString()
        {
            return this;
        }
    }
}
