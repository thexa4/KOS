using kOS.Safe.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace kOS.AddOns.MechJeb2
{
    public class MechJebHook
    {
        private const String MECHJEB_ASSEMBLY = "MechJeb2";

        private Assembly _assembly;
        private static MechJebHook _instance;

        public static MechJebHook Instance
        {
            get
            {
                if (_instance == null)
                    _instance = Hook();
                return _instance;
            }
        }

        public Type this[string fullName]
        {
            get
            {
                return _assembly.GetTypes().Where((t) => t.FullName == fullName).FirstOrDefault();
            }
        }

        public static MechJebHook Hook()
        {
            SafeHouse.Logger.Log(string.Format("Looking for MechJeb2"));
            var loadedAssembly = AssemblyLoader.loadedAssemblies.FirstOrDefault(a => a.assembly.GetName().Name.Equals(MECHJEB_ASSEMBLY));
            if (loadedAssembly == null) return null;
            SafeHouse.Logger.Log(string.Format("Found MechJeb2! Version: {0}.{1}", loadedAssembly.versionMajor, loadedAssembly.versionMinor));

            return new MechJebHook(loadedAssembly.assembly);
        }

        public MechJebHook(Assembly assembly)
        {
            _assembly = assembly;
        }

        public static bool IsAvailable()
        {
            return Instance != null;
        }

        public TypeWrapper GetMechJeb(Vessel vessel)
        {
            var extensions = this["MuMech.VesselExtensions"];
            if (extensions == null)
                return null;

            var fetchFunction = extensions.GetMethods().Where((m) => m.Name == "GetMasterMechJeb" && m.IsStatic).FirstOrDefault();
            if(fetchFunction == null)
            {
                SafeHouse.Logger.LogError("GetMasterMechJeb method not found!");
                return null;
            }

            try
            {
                return new TypeWrapper(fetchFunction.Invoke(null, new object[] { vessel }));
            }
            catch (Exception e)
            {
                SafeHouse.Logger.LogException(e);
                return null;
            }
        }

        public bool IsInstalled(Vessel vessel)
        {
            return GetMechJeb(vessel) != null;
        }
    }
}
