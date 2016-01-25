﻿using kOS.Safe.Encapsulation.Suffixes;
using kOS.Module;
using kOS.Safe.Persistence;

namespace kOS.Suffixed.PartModuleField
{
    public class kOSProcessorFields : PartModuleFields
    {
        private readonly kOSProcessor processor;

        public kOSProcessorFields(kOSProcessor processor, SharedObjects sharedObj):base(processor, sharedObj)
        {
            this.processor = processor;
            InitializeSuffixes();
        }

        private void InitializeSuffixes()
        {
            AddSuffix("MODE", new NoArgsSuffix<string>(() => processor.ProcessorMode.ToString(), "This processor's mode"));
            AddSuffix("ACTIVATE", new NoArgsSuffix(Activate, "Activate this processor"));
            AddSuffix("DEACTIVATE", new NoArgsSuffix(Deactivate, "Deactivate this processor"));
            AddSuffix("VOLUME", new NoArgsSuffix<Volume>(() => processor.HardDisk, "This processor's hard disk"));
            AddSuffix("TAG", new NoArgsSuffix<string>(() => processor.Tag, "This processor's tag"));
            AddSuffix("BOOTFILENAME", new SetSuffix<string>(GetBootFilename, SetBootFilename, "The name of the processor's boot file."));
        }

        private void Activate()
        {
            ThrowIfNotCPUVessel();

            processor.ProcessorMode = kOS.Safe.Module.ProcessorModes.STARVED;
        }

        private void Deactivate()
        {
            ThrowIfNotCPUVessel();

            processor.ProcessorMode = kOS.Safe.Module.ProcessorModes.OFF;
        }

        private string GetBootFilename()
        {
            return processor.BootFilename;
        }

        private void SetBootFilename(string name)
        {
            ThrowIfNotCPUVessel();

            processor.BootFilename = name;
        }
    }
}
