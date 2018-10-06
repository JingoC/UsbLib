using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsbLib.Scsi.Commands
{
    public class ReadCapacity : ScsiCommand
    {
        public UInt32 CountSectors() =>
            BitConverter.ToUInt32(this.Sptw.GetDataBuffer().ToList().GetRange(0, 4).Reverse<byte>().ToArray(), 0);

        public UInt32 SizeSector() =>
            BitConverter.ToUInt32(this.Sptw.GetDataBuffer().ToList().GetRange(4, 4).Reverse<byte>().ToArray(), 0);

        public float Capacity() => (float)this.CountSectors() * this.SizeSector();

        public ReadCapacity() : 
            base(new ScsiPassThroughWrapper(new byte[] { (byte) ScsiCommandCode.ReadCapacity, 0, 0, 0, 0, 0, 0, 0 }, DataDirection.FILE_SHARE_READ, 8))
        {

        }


    }
}
