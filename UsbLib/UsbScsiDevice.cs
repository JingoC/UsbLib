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

    /// <summary>
    /// Scsi device class on base Usb Bulk protocol
    /// </summary>
    public class UsbScsiDevice : ScsiDevice
    {
        UsbDriver usb = new UsbDriver();

        public bool Connect(string drive) => this.usb.Connect(drive);

        public void Disconnect() => this.usb.Disconnect();

        /// <summary>
        /// Execute scsi command by scsi code
        /// </summary>
        /// <param name="code">scsi command code</param>
        /// <returns>result operation</returns>
        public bool Execute(ScsiCommandCode code) => this.usb.Ioctl(this.Commands[code].Sptw);

        /// <summary>
        /// Read data from device
        /// </summary>
        /// <param name="lba">logical block address</param>
        /// <param name="sectors">count read sectors</param>
        /// <returns>read data</returns>
        public byte[] Read(UInt32 lba, UInt32 sectors)
        {
            byte[] data = new byte[sectors * 512];
            
            UInt32 offset = 0;
            
            while (sectors > 0)
            {
                UInt32 transferSectorLength = (sectors >= 64) ? 64 : sectors;
                UInt32 transferBytes = transferSectorLength * 512;

                this.Read10.SetBounds(lba, transferSectorLength);
                this.Execute(ScsiCommandCode.Read10);

                var buf = this.Read10.Sptw.GetDataBuffer();
                Array.Copy(buf, 0, data, offset, transferBytes);

#if false
                this.PrintData(lba, transferSectorLength, buf);
#endif

                lba += transferSectorLength;
                sectors -= transferSectorLength;
                offset += transferBytes;
            }

            return data;
        }

        /// <summary>
        /// Write data in device
        /// </summary>
        /// <param name="lba">logical block address</param>
        /// <param name="sectors">count read sectors</param>
        /// <param name="data">writing data (alignment by 512 bytes)</param>
        public void Write(UInt32 lba, UInt32 sectors, byte[] data)
        {
            UInt32 offset = 0;
            var buf = this.Write10.Sptw.GetDataBuffer();

            while(sectors > 0)
            {
                UInt32 transferSectorLength = (sectors >= 64) ? 64 : sectors;
                UInt32 transferBytes = transferSectorLength * 512;

                Array.Copy(data, offset, buf, 0, transferBytes);
                this.Write10.SetBounds(lba, transferSectorLength);
                if (!this.Execute(ScsiCommandCode.Write10))
                    Console.WriteLine("Error Ioctl: 0x{0:X8}", this.usb.GetError());

                lba += transferSectorLength;
                sectors -= transferSectorLength;
                offset += transferBytes;
            }
        }

        private void PrintData(UInt32 lba, UInt32 sectors, byte[] data)
        {
            int columns = 16;
            int rows = (int)sectors * 512 / columns;
            for (int row = 0; row < rows; row++)
            {
                Console.Write(string.Format("{0}{1:X8} | ", Environment.NewLine, lba * 512 + row * columns));
                for (int col = 0; col < columns; col++)
                {
                    Console.Write(string.Format("{0:X2} ", data[row * columns + col]));
                }

                Console.Write("| ");

                for (int ch = 0; ch < columns; ch++)
                {
                    char code = (char)data[row * columns + ch];
                    bool isTextNumber = (code > 0x20) && (code < 0x80);
                    Console.Write(isTextNumber ? code : '.');
                }
            }
        }
    }
}
