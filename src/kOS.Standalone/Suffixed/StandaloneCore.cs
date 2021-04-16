using System;
using System.Collections.Generic;
using System.Linq;
using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using kOS.Safe.Persistence;
using kOS.Standalone;

namespace kOS.Standalone.Suffixed
{
    [kOS.Safe.Utilities.KOSNomenclature("ArchiveCore")]
    class StandaloneCore : Structure
    {

        private readonly StandaloneSharedObjects shared;
        public StandaloneCore(StandaloneSharedObjects shared)
        {
            this.shared = shared;
            AddSuffix("VESSEL", new NoArgsSuffix<StandaloneShip>(() => shared.StandaloneShip));
            AddSuffix("TAG", new NoArgsSuffix<StringValue>(() => new StringValue("Archive")));
            AddSuffix("VOLUME", new NoArgsSuffix<Volume>(() => shared.VolumeMgr.CurrentVolume));
            AddSuffix("CURRENTVOLUME", new NoArgsSuffix<Volume>(() => shared.VolumeMgr.CurrentVolume));
            AddSuffix("HOMECONNECTION", new NoArgsSuffix<StandaloneConnection>(() => shared.StandaloneConnection));
        }
    }
}
