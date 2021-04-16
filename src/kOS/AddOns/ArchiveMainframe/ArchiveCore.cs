using System;
using System.Collections.Generic;
using System.Linq;
using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using kOS.Communication;
using kOS.Safe.Persistence;

namespace kOS.AddOns.ArchiveMainframe
{
    [kOS.Safe.Utilities.KOSNomenclature("ArchiveCore")]
    class ArchiveCore : Structure
    {

        private readonly SharedMainframeObjects shared;
        public ArchiveCore(SharedMainframeObjects shared)
        {
            this.shared = shared;
            AddSuffix("VESSEL", new NoArgsSuffix<ArchiveShip>(() => shared.ArchiveShip));
            AddSuffix("TAG", new NoArgsSuffix<StringValue>(() => new StringValue("Archive")));
            AddSuffix("VOLUME", new NoArgsSuffix<Volume>(() => shared.VolumeMgr.CurrentVolume));
            AddSuffix("CURRENTVOLUME", new NoArgsSuffix<Volume>(() => shared.VolumeMgr.CurrentVolume));
            AddSuffix("HOMECONNECTION", new NoArgsSuffix<ArchiveConnection>(() => shared.ArchiveConnection));
            if (Mainframe.instance != null)
            {
                AddSuffix("MESSAGES", new NoArgsSuffix<MessageQueueStructure>(() => new MessageQueueStructure(Mainframe.instance.messageQueue, shared)));
            }
        }
    }
}
