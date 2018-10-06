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
            while(sectors > 0)
            {
                UInt32 transferSectorLength = 0;

                if (sectors >= 64)
                {
                    transferSectorLength = 64;
                }
                else
                {
                    transferSectorLength = sectors;
                }

                this.Read10.SetBounds(lba, transferSectorLength);
                this.Execute(ScsiCommandCode.Read10);
                this.Read10.Sptw.GetDataBuffer().CopyTo(data, transferSectorLength * 512);

                lba += transferSectorLength;
                sectors -= transferSectorLength;
                offset += transferSectorLength * 512;
            }

            return data;
        }
    }
}
