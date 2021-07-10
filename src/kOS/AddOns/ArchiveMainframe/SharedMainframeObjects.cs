using System;
using System.Collections.Generic;
using System.Linq;
using kOS;

namespace kOS.AddOns.ArchiveMainframe
{
    class SharedMainframeObjects : SharedObjects
    {
        public SharedMainframeObjects()
        {
            ArchiveShip = new ArchiveShip(this);
            ArchiveCore = new ArchiveCore(this);
            ArchiveConnection = new ArchiveConnection(this);
        }

        public ArchiveShip ArchiveShip;
        public ArchiveCore ArchiveCore;
        public ArchiveConnection ArchiveConnection;
    }
}
