using System;
using System.Collections.Generic;
using System.Linq;
using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using kOS.Standalone;

namespace kOS.Standalone.Suffixed
{
    [kOS.Safe.Utilities.KOSNomenclature("ArchiveShip")]
    class StandaloneShip : Structure
    {

        private readonly StandaloneSharedObjects shared;
        public StandaloneShip(StandaloneSharedObjects shared)
        {
            this.shared = shared;
            AddSuffix("NAME", new NoArgsSuffix<StringValue>(() => new StringValue("Archive")));
            AddSuffix("CONNECTION", new NoArgsSuffix<StandaloneConnection>(() => shared.StandaloneConnection));
        }
    }
}
