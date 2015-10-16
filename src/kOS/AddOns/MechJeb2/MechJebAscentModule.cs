using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using kOS.Safe.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace kOS.AddOns.MechJeb2
{
    public class MechJebAscentModule : Structure, IDrivable
    {
        TypeWrapper _ascentModule;
        Addon _addon;

        public static MechJebAscentModule Create(MechJebHook hook, TypeWrapper mechCore, Addon addon)
        {
            var type = hook["MuMech.MechJebModuleAscentAutopilot"];
            if (type == null)
            {
                SafeHouse.Logger.LogError("Type MuMech.MechJebModuleAscentAutopilot not found!");
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
                SafeHouse.Logger.LogWarning("Module Ascent not enabled");
                return null;
            }
            return new MechJebAscentModule(module, addon);
        }

        public MechJebAscentModule(TypeWrapper ascentModule, Addon addon)
        {
            _ascentModule = ascentModule;
            _addon = addon;

            InitializeSuffixes();
        }

        public void OnDrive()
        {
            if(!IsActive())
            {
                _addon.RegisteredFlightEvents.Remove(this);
                return;
            }
        }

        private void InitializeSuffixes()
        {
            AddSuffix("START", new NoArgsSuffix(Start, "Starts the Ascent Autopilot"));
            AddSuffix("STOP", new NoArgsSuffix(Stop, "Stops the Ascent Autopilot"));
            AddSuffix("ACTIVE", new Suffix<bool>(IsActive, "Returns true if the ascent module is active"));
            AddSuffix("ALTITUDE", new ClampSetSuffix<double>(GetAltitude, SetAltitude, 0, 500000, "Desired altitude [0-500000]"));
            AddSuffix("INCLINATION", new ClampSetSuffix<double>(GetInclination, SetInclination, -90, 90, "Desired inclination [-90, 90]"));
        }

        private void Start()
        {
            _ascentModule["enabled"] = true;
            _addon.RegisteredFlightEvents.Add(this);
        }

        private void Stop()
        {
            _ascentModule["enabled"] = false;
        }

        private bool IsActive()
        {
            return (bool)(_ascentModule["enabled"] as TypeWrapper).Target;
        }

        private double GetAltitude()
        {
            var alt = _ascentModule["desiredOrbitAltitude"] as TypeWrapper;
            return (double)alt["val"];
        }

        private void SetAltitude(double value)
        {
            var alt = _ascentModule["desiredOrbitAltitude"] as TypeWrapper;
            alt["val"] = value;
        }

        private double GetInclination()
        {
            return (double)(_ascentModule["desiredInclination"] as TypeWrapper).Target;
        }

        private void SetInclination(double value)
        {
            _ascentModule["desiredInclination"] = value;
        }
    }
}
