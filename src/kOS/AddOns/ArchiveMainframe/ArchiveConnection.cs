using System;
using System.Collections.Generic;
using System.Linq;
using kOS.Safe.Encapsulation;
using kOS.Safe.Encapsulation.Suffixes;
using kOS.Communication;

namespace kOS.AddOns.ArchiveMainframe
{
    [kOS.Safe.Utilities.KOSNomenclature("ArchiveConnection")]
    class ArchiveConnection : Structure
    {

        private readonly SharedMainframeObjects shared;
        public ArchiveConnection(SharedMainframeObjects shared)
        {
            this.shared = shared;
            AddSuffix("ISCONNECTED", new NoArgsSuffix<BooleanValue>(() => new BooleanValue(true)));
            AddSuffix("DELAY", new NoArgsSuffix<ScalarValue>(() => ScalarValue.Create(0)));
            AddSuffix("DESTINATION", new NoArgsSuffix<ArchiveShip>(() => shared.ArchiveShip));
            if (Mainframe.instance != null)
            {
                AddSuffix("MESSAGES", new NoArgsSuffix<MessageQueueStructure>(() => new MessageQueueStructure(Mainframe.instance.messageQueue, shared)));
            }
        }
    }
}
