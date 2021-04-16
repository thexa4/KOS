using System;
using System.Collections.Generic;
using System.Linq;
using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using kOS.Standalone;

namespace kOS.Standalone.Suffixed
{
    [kOS.Safe.Utilities.KOSNomenclature("TimePolyfill")]
    class TimePolyfill : Structure
    {
        private readonly DateTime startTime;
        public TimePolyfill(DateTime startTime)
        {
            this.startTime = startTime;
            AddSuffix("SECONDS", new NoArgsSuffix<ScalarValue>(() => (DateTime.Now - startTime).TotalSeconds));
        }
    }
}
