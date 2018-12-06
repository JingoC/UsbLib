using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsbLibConsole
{
    using System.Runtime.InteropServices;
    using UsbLib;
    using UsbLib.Usb;
    using UsbLib.Scsi.Commands;

    class Program
    {
        static void PrintBuffer(byte[] buf, int start, int count)
        {
            for (int i = start; i < count; i++)
            {
                if ((i % 16) == 0)
                    Console.WriteLine("");

                Console.Write(string.Format("{0:X2} ", buf[i]));
            }
                
                    
        }

        static void UsbDriverTest()
        {
            UsbScsiDevice usb = new UsbScsiDevice();

            try
            {
                Console.WriteLine($"Connect device: {usb.Connect("E")}");

                if (usb.Execute(ScsiCommandCode.ReadCapacity))
                {
                    ReadCapacity rc10 = usb.ReadCapacity;
                    UInt32 cntSector = rc10.CountSectors();
                    UInt32 sizeSector = rc10.SizeSector();
                    Console.WriteLine($"C: {cntSector}, S: {sizeSector}");

                    UInt32 mb = (UInt32)(rc10.Capacity() / (1024 * 1024));
                    Console.WriteLine($"Sectors: {cntSector} [{mb}] MB");
                    Console.Write("ReadCapacity10: ");
                    PrintBuffer(rc10.Sptw.GetDataBuffer(0, 8), 0, 8);
                }
                else
                    Console.WriteLine($"Bad command: {Marshal.GetLastWin32Error()}");

                if (usb.Execute(ScsiCommandCode.Inquiry))
                {
                    byte[] po = usb.Inquiry.Sptw.GetDataBuffer();

                    Console.WriteLine("");
                    Console.Write("Inquiry: ");
                    for (int i = 0; i < 32; i++)
                        Console.Write(string.Format("{0:X} ", po[i]));
                }

#if false
                usb.Read10.SetBounds(0, 64);
                usb.Read10.Sptw.SetCdb(new byte[]
                {
                    (byte) ScsiCommandCode.Read10, 0,
                    0, 0, 0, 64,
                    0,
                    0, 64,
                    0
                });
                usb.Execute(ScsiCommandCode.Read10);
                var data = usb.Read10.Sptw.GetDataBuffer();
                Console.WriteLine("");
                Console.WriteLine("Data: ");
                PrintBuffer(data, 0, 128);
#else
                byte[] readData = usb.Read(1, 1);
                System.IO.File.WriteAllBytes("test.bin", readData);
#endif

            }
            catch(Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}");
                usb.Disconnect();
            }
        }

        static void Main(string[] args)
        {
            UsbDriverTest();

            Console.ReadLine();
        }
    }
}
