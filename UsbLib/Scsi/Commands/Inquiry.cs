using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsbLib.Scsi.Commands
{
    public class Inquiry : ScsiCommand
    {
        public Inquiry() :
            base(new ScsiPassThroughWrapper(new byte[] { (byte)ScsiCommandCode.Inquiry, 0, 0, 0, 0, 0 },
                DataDirection.SCSI_IOCTL_IN, 58))
        {

        }
    }
}
