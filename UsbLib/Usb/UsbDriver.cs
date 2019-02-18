using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UsbLib.Usb
{
    using Scsi;
    using System.IO;

    public enum IoctlCodes
    {
        IOCTL_SCSI_PASS_THROUGH_DIRECT = 0x0004D004,
        IOCTL_STORAGE_EJECT_MEDIA = 0x2D4808
    }

    /// <summary>
    /// Usb driver class on base DeviceIoControl mechanism
    /// </summary>
    public class UsbDriver
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr CreateFileW(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr SecurityAttributes,
                                                uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode, IntPtr lpInBuffer, uint nInBufferSize,
                                                    IntPtr lpOutBuffer, uint nOutBufferSize, out uint lpBytesReturned, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode, byte[] lpInBuffer, uint nInBufferSize,
                                                    IntPtr lpOutBuffer, uint nOutBufferSize, out uint lpBytesReturned, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);
        
        [DllImport("kernel32.dll")]
        private static extern uint GetLastError();

        const uint GENERIC_READ = 0x80000000;
        const uint GENERIC_WRITE = 0x40000000;

        private IntPtr handle = IntPtr.Zero;
        private uint lastError = 0;

        public uint GetError() => this.lastError;

        /// <summary>
        /// Open device by drive name
        /// </summary>
        /// <param name="usb">drive name</param>
        /// <returns></returns>
        public bool Connect(string usb)
        {
            uint shareMode = (uint)(0x3);
            uint desiredAccess = GENERIC_READ | GENERIC_WRITE;
            
            
            string path = @"\\.\" + usb + @":";
            //this.handle = CreateFile(path, desiredAccess, shareMode, IntPtr.Zero, 0x3, 0, IntPtr.Zero);
            this.handle = CreateFileW(path, desiredAccess, shareMode, IntPtr.Zero, 
                (uint) FileMode.Open, (uint)FileAttributes.Normal, IntPtr.Zero);

            return ((long)this.handle != -1);
        }
        
        /// <summary>
        /// Close device
        /// </summary>
        public void Disconnect() => CloseHandle(this.handle);
        
        /// <summary>
        /// Wrapper calling low level function DeviceIoControl
        /// </summary>
        /// <param name="sptw">scsi params</param>
        /// <returns></returns>
        public bool Ioctl(ScsiPassThroughWrapper sptw)
        {
            IntPtr bufferPointer = IntPtr.Zero;
            bool ioresult = false;

            try
            {
                Int32 bufferSize = Marshal.SizeOf(sptw.sptBuffered);

                bufferPointer = Marshal.AllocHGlobal(bufferSize);
                Marshal.StructureToPtr(sptw.sptBuffered, bufferPointer, true);
                
                ioresult = DeviceIoControl(this.handle, (uint)IoctlCodes.IOCTL_SCSI_PASS_THROUGH_DIRECT, bufferPointer, (UInt32)bufferSize,
                    bufferPointer, (UInt32)bufferSize, out UInt32 bytesReturned, IntPtr.Zero) && (bytesReturned > 0);

                if (ioresult)
                {
                    sptw.sptBuffered = (ScsiPassThroughBuffered) Marshal.PtrToStructure(bufferPointer, typeof(ScsiPassThroughBuffered));
                }
                else
                {
                    this.lastError = GetLastError();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("IO Ctl FAIL: {0}", e.Message);
            }
            finally
            {
                Marshal.FreeHGlobal(bufferPointer);
            }

            return ioresult;
        }
    }
}
