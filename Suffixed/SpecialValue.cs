using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace kOS.Suffixed
{
    public class SpecialValue
    {
        public virtual bool SetSuffix(String suffixName, object value)
        {
            return false;
        }

        public virtual object GetSuffix(String suffixName)
        {
            return SpecialResult.SuffixNotFound;
        }

        public virtual object TryOperation(string op, object other, bool reverseOrder)
        {
            return null;
        }

        protected object ConvertToDoubleIfNeeded(object value)
        {
            if (!(value is SpecialValue) && !(value is double))
            {
                value = Convert.ToDouble(value);
            }

            return value;
        }
    }

    public enum SpecialResult
    {
        SuffixNotFound = 1,
        VoidResult = 2
    }
}
