using System;
using System.Collections.Generic;
using System.Linq;
using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using kOS.Communication;

namespace kOS.AddOns.ArchiveMainframe
{
    [kOS.Safe.Utilities.KOSNomenclature("ArchiveShip")]
    class ArchiveShip : Structure
    {

        private readonly SharedMainframeObjects shared;
        public ArchiveShip(SharedMainframeObjects shared)
        {
            this.shared = shared;
            AddSuffix("NAME", new NoArgsSuffix<StringValue>(() => new StringValue("Archive")));
            AddSuffix("CONNECTION", new NoArgsSuffix<ArchiveConnection>(() => shared.ArchiveConnection));
            if (Mainframe.instance != null)
            {
                AddSuffix("MESSAGES", new NoArgsSuffix<MessageQueueStructure>(() => new MessageQueueStructure(Mainframe.instance.messageQueue, shared)));
            }
        }
    }
}
