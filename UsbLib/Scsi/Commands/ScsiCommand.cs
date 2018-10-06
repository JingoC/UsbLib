using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsbLib.Scsi.Commands
{
    public enum ScsiCommandCode : byte
    {
        Inquiry = 0x12,
        ReadCapacity = 0x25,
        Read10 = 0x28,
    }

    public class ScsiCommand
    {
        public ScsiPassThroughWrapper Sptw { get; private set; }

        public ScsiCommand(ScsiPassThroughWrapper sptw)
        {
            this.Sptw = sptw;
        }
    }
}
