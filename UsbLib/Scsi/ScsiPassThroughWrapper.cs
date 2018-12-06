using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UsbLib.Scsi
{
    public enum DataDirection
    {
        FILE_SHARE_READ = 1,
        FILE_SHARE_WRITE = 2,
        SCSI_IOCTL_DATA_UNSPECIFIED = 3
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ScsiPassThrough
    {
        public UInt16 Length;
        public Byte SCSIStatus;
        public Byte PathID;
        public Byte TargetID;
        public Byte LogicalUnitNumber;
        public Byte CommandDescriptorBlockLength;
        public Byte SenseInfoLength;
        public Byte DataIn;
        public UInt32 DataTransferLength;
        public UInt32 TimeOutValue;
        public IntPtr DataBufferOffset;
        public UInt32 SenseInfoOffset;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public Byte[] Cdb;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ScsiPassThroughBuffered
    {
        public ScsiPassThrough Spt;
        public UInt32 Filler;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 65536)]
        public Byte[] Buffer;
    }

    /// <summary>
    /// Wrapping standart format scsi protocol data
    /// </summary>
    public class ScsiPassThroughWrapper
    {
        public ScsiPassThroughBuffered sptBuffered;
        
        public ScsiPassThroughWrapper(byte[] cdb = null, 
            DataDirection direction = DataDirection.SCSI_IOCTL_DATA_UNSPECIFIED, 
            UInt32 dataTransferLength = 0)
        {
            this.sptBuffered = new ScsiPassThroughBuffered();
            this.sptBuffered.Spt.Cdb = new byte[16];

            this.SetCdb(cdb);

            this.sptBuffered.Spt.Length = (UInt16)Marshal.SizeOf(this.sptBuffered.Spt);
            this.sptBuffered.Spt.CommandDescriptorBlockLength = 16;
            this.sptBuffered.Spt.TimeOutValue = 60;
            
            this.sptBuffered.Spt.DataIn = (byte) direction;
            this.SetDataLength(dataTransferLength);
            this.sptBuffered.Spt.DataBufferOffset = new IntPtr(Marshal.SizeOf(this.sptBuffered) - 65536);
        }

        public byte[] GetCdb() => this.sptBuffered.Spt.Cdb;
        public byte[] GetCdb(int start, int count) => this.sptBuffered.Spt.Cdb.ToList().GetRange(start, count).ToArray();
        public void SetCdb(byte[] cdb) => this.SetCdb(cdb, 0, 0, cdb.Length);
        public void SetCdb(byte[] cdb, int startSrc, int startDst, int count)
        {
            Array.Copy(cdb, startSrc, this.sptBuffered.Spt.Cdb, startDst, count);
            
#if true
            Console.WriteLine("");
            Console.Write("CDB: ");
            for (int i = 0; i < 16; i++)
                Console.Write(string.Format("{0:X} ", this.sptBuffered.Spt.Cdb[i]));
#endif
        }

        public byte[] GetDataBuffer() => this.sptBuffered.Buffer;
        public byte[] GetDataBuffer(int start, int count) => this.sptBuffered.Buffer.ToList().GetRange(start, count).ToArray();

        public void SetDataLength(UInt32 dataLength) => this.sptBuffered.Spt.DataTransferLength = dataLength;
    }
}
