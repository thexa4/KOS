using kOS.Safe.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace kOS.AddOns.MechJeb2
{
    public class TypeWrapper
    {
        public object Target { get; protected set; }
        public Type Type { get { return Target.GetType(); } }

        public delegate object Function(params object[] args);
        public delegate object GenericFunction(Type type, params object[] args);

        private BindingFlags _defaultSearch = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;

        public TypeWrapper(object target)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            Target = target;
        }

        public override bool Equals(object obj)
        {
            if(obj is TypeWrapper)
                return (obj as TypeWrapper).Target == Target;
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Target.GetHashCode();
        }

        public object this[string name]
        {
            get
            {
                var field = Type.GetFields(_defaultSearch).Where((f) => f.Name == name && !f.IsStatic).FirstOrDefault();
                if (field != null)
                    return new TypeWrapper(field.GetValue(Target));

                var property = Type.GetProperties(_defaultSearch).Where((p) => p.Name == name).FirstOrDefault();
                if (property != null)
                    return new TypeWrapper(property.GetGetMethod().Invoke(Target, new object[] { }));

                var method = Type.GetMethods(_defaultSearch).Where((m) => m.Name == name && !m.IsStatic).FirstOrDefault();
                if (method != null)
                {
                    if (method.IsGenericMethod)
                        return (GenericFunction)((type, args) => method.MakeGenericMethod(type).Invoke(Target, args));
                    else
                        return (Function)(args => method.Invoke(Target, args));
                }

                return null;
            }
            set
            {
                var field = Type.GetFields(_defaultSearch).Where((f) => f.Name == name && !f.IsStatic).FirstOrDefault();
                if (field != null)
                {
                    field.SetValue(Target, (object)value);
                    return;
                }

                foreach (var n in Type.GetProperties(_defaultSearch).Select((p) => p.Name))
                    SafeHouse.Logger.Log(n);

                var property = Type.GetProperties(_defaultSearch).Where((p) => p.Name == name).FirstOrDefault();
                if (property != null)
                {
                    property.GetSetMethod().Invoke(Target, new object[] { value });
                    return;
                }

                SafeHouse.Logger.LogError(String.Format("Unknown field {0}:{1}", Type.Name, name));
            }
        }
    }
}
