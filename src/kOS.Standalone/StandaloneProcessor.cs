using System;
using System.Collections.Generic;
using System.Text;
using kOS.Safe.Module;
using kOS.Safe.Persistence;

namespace kOS.Standalone
{
    class StandaloneProcessor : IProcessor
    {
        private readonly StandaloneSharedObjects shared;
        public StandaloneProcessor(StandaloneSharedObjects shared, string bootfile = null)
        {
            this.shared = shared;
            if (bootfile != null)
            {
                BootFilePath = VolumePath.FromString(bootfile);
            }
        }
        public VolumePath BootFilePath { get; set; }

        public string Tag => throw new NotImplementedException();

        public int KOSCoreId => throw new NotImplementedException();

        public bool CheckCanBoot()
        {
            return true;
        }

        public void SetMode(ProcessorModes newProcessorMode)
        {
            if (shared.ProcessorMode == ProcessorModes.OFF && newProcessorMode == ProcessorModes.READY)
                shared.ProcessorMode = ProcessorModes.STARVED;
            else
                shared.ProcessorMode = newProcessorMode;
        }
    }
}
