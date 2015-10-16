using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using kOS.Safe.Utilities;
using kOS.Suffixed;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace kOS.AddOns.MechJeb2
{
    public class Addon : Suffixed.Addon
    {
        public List<IDrivable> RegisteredFlightEvents { get; protected set; }
        private TypeWrapper.Function _drive;
        private TypeWrapper _core;
        private MethodInfo _deactivate;

        public Addon(SharedObjects shared)
            : base("MJ", shared)
        {
            InitializeSuffixes();
            RegisteredFlightEvents = new List<IDrivable>();
            if (kOS.AddOns.RemoteTech.RemoteTechHook.IsAvailable())
            {
                kOS.AddOns.RemoteTech.RemoteTechHook.Instance.AddSanctionedPilot(shared.Vessel.id, OnDrive);
                _core = MechJebHook.Instance.GetMechJeb(shared.Vessel);
                var flyByWire = _core.Type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Where((m) => m.Name == "OnFlyByWire").FirstOrDefault();
                _drive = (TypeWrapper.Function)(args => flyByWire.Invoke(_core.Target, args));
                _deactivate = _core.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where((p) => p.Name == "DeactivateControl").First().GetSetMethod();
            }
        }

        private void OnDrive(FlightCtrlState state)
        {
            _deactivate.Invoke(_core.Target, new object[] { false });
            if (RegisteredFlightEvents.Count == 0)
                return;

            if (!kOS.AddOns.RemoteTech.RemoteTechHook.IsAvailable())
                return;
            if (kOS.AddOns.RemoteTech.RemoteTechHook.Instance.HasLocalControl(shared.Vessel.id))
                return;
            if (kOS.AddOns.RemoteTech.RemoteTechHook.Instance.HasAnyConnection(shared.Vessel.id))
                return;
            
            
            _drive(state);

            // To enable subscribers to remove themselves
            var subscribers = RegisteredFlightEvents.ToList();
            foreach (var sub in subscribers)
                sub.OnDrive();

            _deactivate.Invoke(_core.Target, new object[] { true });
        }

        private void InitializeSuffixes()
        {
            AddSuffix("ASCENT", new Suffix<MechJebAscentModule>(GetAscent, "Returns a reference to the Ascent Module"));
            AddSuffix("LANDING", new Suffix<MechJebLandingModule>(GetLanding, "Returns a reference to the Landing Module"));
            AddSuffix("TARGET", new Suffix<MechJebTargetModule>(GetTarget, "Returns a reference to the Target Module"));
            AddSuffix("MANEUVER", new Suffix<MechJebManeuverModule>(GetManeuver, "Returns a reference to the Maneuver Module"));
            AddSuffix("EXECUTOR", new Suffix<MechJebNodeExecutorModule>(GetExecutor, "Returns a reference to the Node Executor Module"));
        }

        private MechJebNodeExecutorModule GetExecutor()
        {
            if (!MechJebHook.IsAvailable())
                return null;

            return MechJebNodeExecutorModule.Create(MechJebHook.Instance, MechJebHook.Instance.GetMechJeb(shared.Vessel), this);
        }

        public MechJebTargetModule GetTarget()
        {
            if (!MechJebHook.IsAvailable())
                return null;

            return MechJebTargetModule.Create(MechJebHook.Instance, MechJebHook.Instance.GetMechJeb(shared.Vessel));
        }

        public MechJebManeuverModule GetManeuver()
        {
            if (!MechJebHook.IsAvailable())
                return null;

            return MechJebManeuverModule.Create(MechJebHook.Instance, MechJebHook.Instance.GetMechJeb(shared.Vessel), this, shared);
        }

        public MechJebAscentModule GetAscent()
        {
            if (!MechJebHook.IsAvailable())
                return null;

            return MechJebAscentModule.Create(MechJebHook.Instance, MechJebHook.Instance.GetMechJeb(shared.Vessel), this);
        }

        public MechJebLandingModule GetLanding()
        {
            if (!MechJebHook.IsAvailable())
                return null;

            return MechJebLandingModule.Create(MechJebHook.Instance, MechJebHook.Instance.GetMechJeb(shared.Vessel), this);
        }

        public override bool Available()
        {
            return MechJebHook.IsAvailable() && MechJebHook.Instance.IsInstalled(shared.Vessel);
        }

    }
}