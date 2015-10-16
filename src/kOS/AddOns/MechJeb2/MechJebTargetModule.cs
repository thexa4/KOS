using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using kOS.Safe.Utilities;
using kOS.Suffixed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace kOS.AddOns.MechJeb2
{
    public class MechJebTargetModule : Structure
    {
        TypeWrapper _targetModule;
        public object Target { get { return _targetModule.Target; } }

        public static MechJebTargetModule Create(MechJebHook hook, TypeWrapper mechCore)
        {
            var type = hook["MuMech.MechJebModuleTargetController"];
            if (type == null)
            {
                SafeHouse.Logger.LogError("Type MuMech.MechJebModuleTargetController not found!");
                return null;
            }

            var fetch = mechCore.Type.GetMethods().Where((m) => m.Name == "GetComputerModule" && !m.IsGenericMethod && m.GetParameters().Count() == 1 && m.GetParameters().First().ParameterType == typeof(string)).FirstOrDefault();
            if (fetch == null)
            {
                SafeHouse.Logger.LogError("Missing method GetComputerModule in MechJebCore");
                return null;
            }

            var result = fetch.Invoke(mechCore.Target, new object[]{type.Name});
            var module = new TypeWrapper(result);

            if (module == null)
            {
                SafeHouse.Logger.LogWarning("Module Landing not enabled");
                return null;
            }
            return new MechJebTargetModule(module);
        }

        public MechJebTargetModule(TypeWrapper targetModule)
        {
            _targetModule = targetModule;

            InitializeSuffixes();
        }

        private TypeWrapper GetOperation(string name)
        {
            var get = _targetModule["GetManeuverNodes"] as TypeWrapper.Function;
            var nodes = ((get() as TypeWrapper).Target as List<object>).Select((o) => new TypeWrapper(o));

            foreach (var n in nodes)
            {
                if (n.Type.FullName == name)
                    return n;
            }

            return null;
        }

        private void InitializeSuffixes()
        {
            AddSuffix("SET", new OneArgsSuffix<IKOSTargetable>(SetTarget, "Sets the target"));
        }

        public void SetTarget(IKOSTargetable target)
        {
            var ksptarget = target.Target;
            SetTarget(ksptarget);
        }

        public void SetTarget(ITargetable target)
        {
            var set = _targetModule["Set"] as TypeWrapper.Function;
            set(target);
        }
    }
}
