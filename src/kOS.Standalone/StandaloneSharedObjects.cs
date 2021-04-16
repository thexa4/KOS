using System;
using kOS.Safe;
using kOS.Safe.Execution;
using kOS.Safe.Compilation.KS;
using kOS.Safe.Function;
using kOS.Safe.Persistence;
using kOS.Safe.Module;
using kOS.Standalone.Suffixed;

namespace kOS.Standalone
{
    class StandaloneSharedObjects : SafeSharedObjects
    {
        public StandaloneSharedObjects()
        {
            ProcessorMode = ProcessorModes.READY;

            StartTime = DateTime.Now;

            UpdateHandler = new UpdateHandler();
            ScriptHandler = new KSScript();
            Interpreter = new StandaloneInterpreter(this);
            Screen = Interpreter;
            BindingMgr = new StandaloneBindingManager(this);
            Logger = new StandaloneLogger(this);
            VolumeMgr = new VolumeManager();
            VolumeMgr.Add(new Archive(kOS.Safe.Utilities.SafeHouse.ArchiveFolder));
            Processor = new StandaloneProcessor(this, "boot/archive");
            FunctionManager = new FunctionManager(this);
            GameEventDispatchManager = new StandaloneEventDispatchManager();
            Cpu = new CPU(this);

            StandaloneConnection = new StandaloneConnection(this);
            StandaloneCore = new StandaloneCore(this);
            StandaloneShip = new StandaloneShip(this);
        }

        public ProcessorModes ProcessorMode { get; set; }
        public StandaloneConnection StandaloneConnection { get; private set; }
        public StandaloneCore StandaloneCore { get; private set; }
        public StandaloneShip StandaloneShip { get; private set; }

        public DateTime StartTime { get; set; }
    }
}
