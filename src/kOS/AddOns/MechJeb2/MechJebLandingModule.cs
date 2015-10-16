using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using kOS.Safe.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace kOS.AddOns.MechJeb2
{
    public class MechJebLandingModule : Structure, IDrivable
    {
        TypeWrapper _landingModule;
        Addon _addon;

        public static MechJebLandingModule Create(MechJebHook hook, TypeWrapper mechCore, Addon addon)
        {
            var type = hook["MuMech.MechJebModuleLandingAutopilot"];
            if (type == null)
            {
                SafeHouse.Logger.LogError("Type MuMech.MechJebModuleLandingAutopilot not found!");
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
            return new MechJebLandingModule(module, addon);
        }

        public MechJebLandingModule(TypeWrapper landingModule, Addon addon)
        {
            _landingModule = landingModule;
            _addon = addon;

            InitializeSuffixes();
        }

        private void InitializeSuffixes()
        {
            AddSuffix("START", new NoArgsSuffix(Start, "Starts the Landing Autopilot"));
            AddSuffix("STOP", new NoArgsSuffix(Stop, "Stops the Landing Autopilot"));
            AddSuffix("ACTIVE", new Suffix<bool>(IsActive, "Returns true if the Landing Module is active"));
        }

        public void OnDrive()
        {
            if (!IsActive())
            {
                _addon.RegisteredFlightEvents.Remove(this);
                return;
            }
        }

        private void Start()
        {
            var start = (TypeWrapper.Function)_landingModule["LandUntargeted"];
            start(this);
            _addon.RegisteredFlightEvents.Add(this);
        }

        private void Stop()
        {
            var stop = (TypeWrapper.Function)_landingModule["StopLanding"];
            stop();
        }

        private bool IsActive()
        {
            return (bool)(_landingModule["enabled"] as TypeWrapper).Target;
        }
    }
}
