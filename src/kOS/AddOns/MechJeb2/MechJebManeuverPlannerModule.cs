using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using kOS.Safe.Utilities;
using kOS.Suffixed;
using kOS.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace kOS.AddOns.MechJeb2
{
    public class MechJebManeuverModule : Structure
    {
        TypeWrapper _maneuverModule;
        Addon _addon;
        SharedObjects _shared;
        MechJebHook _hook;

        public static MechJebManeuverModule Create(MechJebHook hook, TypeWrapper mechCore, Addon addon, SharedObjects shared)
        {
            var type = hook["MuMech.MechJebModuleManeuverPlanner"];
            if (type == null)
            {
                SafeHouse.Logger.LogError("Type MuMech.MechJebModuleManeuverPlanner not found!");
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
            return new MechJebManeuverModule(module, addon, shared, hook);
        }

        public MechJebManeuverModule(TypeWrapper maneuverModule, Addon addon, SharedObjects shared, MechJebHook hook)
        {
            _maneuverModule = maneuverModule;
            _addon = addon;
            _shared = shared;
            _hook = hook;

            InitializeSuffixes();
        }

        private TypeWrapper GetOperation(string name)
        {
            var type = _hook["MuMech.Operation"];
            if(type == null)
            {
                SafeHouse.Logger.LogError("Unable to find MuMech.Operation type");
                return null;
            }

            var get = type.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).Where((m) => m.Name == "getAvailableOperations").FirstOrDefault();
            if (get == null)
            {
                SafeHouse.Logger.LogError("Unable to get operations nodes out of MechJeb");
                return null;
            }
            var result = get.Invoke(null, new object[] { }) as IEnumerable<object>;
            if (result == null) SafeHouse.Logger.LogError("result is null");

            var nodes = new List<object>(result).Select((o) => new TypeWrapper(o));

            foreach (var n in nodes)
            {
                if (n.Type.FullName == name)
                    return n;
            }

            return null;
        }


        private void InitializeSuffixes()
        {
            AddSuffix("HUFFMAN", new NoArgsSuffix<Node>(() => DoOperation("MuMech.OperationGeneric", 0), "Calculates a huffman transfer"));
            AddSuffix("ADVHUFFMAN", new TwoArgsSuffix<Node, Orbit, double>((orbit, time) => DoOperation("MuMech.OperationGeneric", time, orbit), "Calculates a huffman transfer at the given time offset and orbit"));
            AddSuffix("CIRCULARIZE", new NoArgsSuffix<Node>(() => DoCircularize(0), "Calculates a circularization burn"));
            AddSuffix("ADVCIRCULARIZE", new TwoArgsSuffix<Node, Orbit, double>((orbit, time) => DoCircularize(time, orbit), "Calculates a circularization burn at the given time offset and orbit"));
            AddSuffix("MOONRETURN", new OneArgsSuffix<Node, double>((altitude) => DoMoonReturn(altitude), "Calculates a return burn from a moon"));
            AddSuffix("ADVMOONRETURN", new TwoArgsSuffix<Node, double, Orbit>((altitude, orbit) => DoMoonReturn(altitude, orbit), "Calculates a return burn from a moon"));
            AddSuffix("PLANEMATCH", new OneArgsSuffix<Node, bool>((ascending) => DoPlaneMatch(ascending), "Calculates a plane match with a target"));
        }

        private Node DoPlaneMatch(bool ascending, Orbit orbit = null)
        {
            var time = Planetarium.GetUniversalTime();
            if (orbit != null)
                time = orbit.StartUT;

            orbit = orbit ?? _shared.Vessel.orbit;

            var operation = GetOperation("MuMech.OperationPlane");
            if (operation == null)
                return null;

            var searchType = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            var field = operation.Type.GetFields(searchType).Where((f) => f.Name == "timeSelector" && !f.IsStatic).FirstOrDefault();

            var timeSelector = new TypeWrapper(field.GetValue(operation.Target));
            timeSelector["currentTimeRef"] = ascending ? 0 : 1;
            
            return ExecuteOperation(orbit, time, operation);
        }

        private Node DoMoonReturn(double altitude, Orbit orbit = null)
        {
            var time = Planetarium.GetUniversalTime();
            if (orbit != null)
                time = orbit.StartUT;

            orbit = orbit ?? _shared.Vessel.orbit;

            var operation = GetOperation("MuMech.OperationMoonReturn");
            if (operation == null)
            {
                SafeHouse.Logger.LogWarning("Operation not enabled.");
                return null;
            }

            var altParam = operation["moonReturnAltitude"] as TypeWrapper;
            altParam["val"] = altitude;

            return ExecuteOperation(orbit, time, operation);
        }

        private Node DoCircularize(double time, Orbit orbit = null)
        {
            time += Planetarium.GetUniversalTime();
            if (orbit != null)
                time = orbit.StartUT;

            orbit = orbit ?? _shared.Vessel.orbit;

            var operation = GetOperation("MuMech.OperationCircularize");
            if (operation == null)
                return null;

            var calculator = _hook["MuMech.OrbitalManeuverCalculator"];
            if (calculator == null)
                return null;

            var method = calculator.GetMethods(BindingFlags.Static | BindingFlags.Public).Where((m) => m.Name == "DeltaVToCircularize").FirstOrDefault();
            if (method == null)
                return null;

            var result = (Vector3d)method.Invoke(null, new object[] { orbit, time });
            return NormalizeDeltaV(orbit, time, result);
        }

        private Node DoOperation(string name, double offset = 0, Orbit orbit = null)
        {
            var time = Planetarium.GetUniversalTime() + offset;

            var operation = GetOperation(name);
            if (operation == null)
                return null;

            orbit = orbit ?? _shared.Vessel.orbit;

            return ExecuteOperation(orbit, time, operation);
        }

        private Node ExecuteOperation(Orbit orbit, double time, TypeWrapper operation)
        {
            var targetModule = _addon.GetTarget();
            if (targetModule == null)
                return null;

            if (_shared.Vessel.targetObject == null) {
                SafeHouse.Logger.Log("No target selected, aborting.");
                return null;
            }
            _addon.GetTarget().SetTarget(_shared.Vessel.targetObject);

            var run = operation["MakeNode"] as TypeWrapper.Function;
            var result = run(orbit, time, targetModule.Target);

            if (result == null)
                return null;
            var wrapper = new TypeWrapper(result);

            var nodetime = (double)(wrapper["UT"] as TypeWrapper).Target;
            var nodevector = (Vector3d)(wrapper["dV"] as TypeWrapper).Target;

            return NormalizeDeltaV(orbit, nodetime, nodevector);
        }

        private Node NormalizeDeltaV(Orbit orbit, double nodetime, Vector3d nodevector)
        {
            var dV = orbit.DeltaVToManeuverNodeCoordinates(nodetime, nodevector);

            return new Node(nodetime, dV.x, dV.y, dV.z, _shared);
        }
    }
}
