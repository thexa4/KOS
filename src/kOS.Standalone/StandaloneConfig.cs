using System;
using System.Collections.Generic;
using System.Text;
using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;

namespace kOS.Standalone
{
    class StandaloneConfig : IConfig
    {
        public StandaloneConfig()
        {
            InstructionsPerUpdate = 20000;
            VerboseExceptions = true;
        }

        public int InstructionsPerUpdate { get; set; }
        public bool UseCompressedPersistence { get; set; }
        public bool ShowStatistics { get; set; }
        public bool StartOnArchive { get => true; set { } }
        public bool ObeyHideUI { get; set; }
        public bool EnableSafeMode { get; set; }
        public bool VerboseExceptions { get; set; }
        public bool EnableTelnet { get; set; }
        public int TelnetPort { get; set; }
        public bool AudibleExceptions { get; set; }
        public string TelnetIPAddrString { get; set; }
        public bool UseBlizzyToolbarOnly { get; set; }
        public int TerminalFontDefaultSize { get; set; }
        public string TerminalFontName { get; set; }
        public double TerminalBrightness { get; set; }
        public int TerminalDefaultWidth { get; set; }
        public int TerminalDefaultHeight { get; set; }
        public bool SuppressAutopilot { get; set; }

        public DateTime TimeStamp => throw new NotImplementedException();

        public bool DebugEachOpcode { get; set; }

        public IList<ConfigKey> GetConfigKeys()
        {
            throw new NotImplementedException();
        }

        public ISuffixResult GetSuffix(string suffixName, bool failOkay = false)
        {
            throw new NotImplementedException();
        }

        public void SaveConfig()
        {
            throw new NotImplementedException();
        }

        public bool SetSuffix(string suffixName, object value, bool failOkay = false)
        {
            throw new NotImplementedException();
        }
    }
}
