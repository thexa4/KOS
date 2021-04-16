using System;
using System.Collections.Generic;
using System.Linq;
using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using kOS.Standalone;

namespace kOS.Standalone.Suffixed
{
    [kOS.Safe.Utilities.KOSNomenclature("ArchiveConnection")]
    class StandaloneConnection : Structure
    {

        private readonly StandaloneSharedObjects shared;
        public StandaloneConnection(StandaloneSharedObjects shared)
        {
            this.shared = shared;
            AddSuffix("ISCONNECTED", new NoArgsSuffix<BooleanValue>(() => new BooleanValue(true)));
            AddSuffix("DELAY", new NoArgsSuffix<ScalarValue>(() => ScalarValue.Create(0)));
            AddSuffix("DESTINATION", new NoArgsSuffix<StandaloneShip>(() => shared.StandaloneShip));
        }
    }
}
