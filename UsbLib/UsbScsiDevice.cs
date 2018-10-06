using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsbLib
{
    using UsbLib.Usb;
    using UsbLib.Scsi;
    using UsbLib.Scsi.Commands;

    public class UsbScsiDevice : ScsiDevice
    {
        UsbDriver usb = new UsbDriver();

        public bool Connect(string drive) => this.usb.Connect(drive);

        public void Disconnect() => this.usb.Disconnect();


        public bool Execute(ScsiCommandCode code) => this.usb.Ioctl(this.Commands[code].Sptw);

        public byte[] Read(UInt32 lba, UInt32 sectors)
        {
            byte[] data = new byte[sectors * 512];
            
            UInt32 offset = 0;
            var buf = this.Read10.Sptw.GetDataBuffer();

            while (sectors > 0)
            {
                UInt32 transferSectorLength = (sectors >= 64) ? 64 : sectors;
                UInt32 transferBytes = transferSectorLength * 512;

                this.Read10.SetBounds(lba, transferSectorLength);
                this.Execute(ScsiCommandCode.Read10);
                Array.Copy(buf, 0, data, offset, transferBytes);

                lba += transferSectorLength;
                sectors -= transferSectorLength;
                offset += transferBytes;
            }

            return data;
        }
    }
}
