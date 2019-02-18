using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsbLib.Scsi.Commands
{
    public class Read10 : ScsiCommand
    {
        public void SetBounds(UInt32 lba, UInt32 sectors)
        {
            var lbaBytes = BitConverter.GetBytes(lba).Reverse<byte>().ToArray();
            var sectorsBytes = BitConverter.GetBytes(sectors).Reverse<byte>().ToArray();

            this.Sptw.SetCdb(lbaBytes, 0, 2, 4);
            this.Sptw.SetCdb(sectorsBytes, 2, 7, 2);
            
            this.Sptw.SetDataLength((uint)(sectors * 512));
        }

        public Read10() :
            base(new ScsiPassThroughWrapper(new byte[] { (byte) ScsiCommandCode.Read10, 0, 0, 0, 0, 0, 0, 0 }, 
                DataDirection.SCSI_IOCTL_IN, 8))
        {

        }
    }
}
