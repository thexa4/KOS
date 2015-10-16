using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using kOS.Safe.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace kOS.AddOns.MechJeb2
{
    class MechJebNodeExecutorModule : Structure, IDrivable
    {
        TypeWrapper _executorModule;
        Addon _addon;

        public static MechJebNodeExecutorModule Create(MechJebHook hook, TypeWrapper mechCore, Addon addon)
        {
            var type = hook["MuMech.MechJebModuleNodeExecutor"];
            if (type == null)
            {
                SafeHouse.Logger.LogError("Type MuMech.MechJebModuleNodeExecutor not found!");
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
            return new MechJebNodeExecutorModule(module, addon);
        }

        public MechJebNodeExecutorModule(TypeWrapper targetModule, Addon addon)
        {
            _executorModule = targetModule;
            _addon = addon;

            InitializeSuffixes();
        }

        public void OnDrive()
        {
            if (!IsActive())
            {
                _addon.RegisteredFlightEvents.Remove(this);
                return;
            }
        }

        private void InitializeSuffixes()
        {
            AddSuffix("STARTONE", new NoArgsSuffix(StartOne, "Executes the next node"));
            AddSuffix("ACTIVE", new NoArgsSuffix<bool>(IsActive, "Returns true if the module is active"));
        }

        private void StartOne()
        {
            var start = (TypeWrapper.Function)_executorModule["ExecuteOneNode"];
            start(this);
            _addon.RegisteredFlightEvents.Add(this);
        }

        private void StartAll()
        {
            var start = (TypeWrapper.Function)_executorModule["ExecuteAllNodes"];
            start(this);
            _addon.RegisteredFlightEvents.Add(this);
        }

        private void Stop()
        {
            var stop = (TypeWrapper.Function)_executorModule["Abort"];
            stop();
        }

        private bool IsActive()
        {
            return (bool)(_executorModule["enabled"] as TypeWrapper).Target;
        }


    }
}
