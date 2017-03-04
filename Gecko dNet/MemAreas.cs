using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using TCPTCPGecko;

namespace GeckoApp
{
    public enum AddressType
    {
        Rw,
        Ro,
        Ex,
        Hardware,
        Unknown
    }

    public class AddressRange
    {
        private AddressType PDesc;
        private Byte   PId;
        private UInt32 PLow;
        private UInt32 PHigh;

        public AddressType description { get { return PDesc; } }
        public Byte id { get { return PId; } }
        public UInt32 low { get { return PLow; } }
        public UInt32 high { get { return PHigh; } }

        public AddressRange(AddressType desc, Byte id, UInt32 low, UInt32 high)
        {
            this.PId = id;
            this.PDesc = desc;
            this.PLow = low;
            this.PHigh = high;
        }

        public AddressRange(AddressType desc, UInt32 low, UInt32 high) :
            this(desc, (Byte)(low >> 24), low, high)
        { }
    }

    public static class ValidMemory {
        public static bool addressDebug = false;

        public static readonly AddressRange[] ValidAreas = new AddressRange[100]; // Will likely never be as much but...
          /*   new AddressRange(AddressType.Ex,  0x01000000,0x01800000),
             new AddressRange(AddressType.Ex,  0x0e300000,0x10000000),
             new AddressRange(AddressType.Rw,  0x10000000,0x45000000),
             new AddressRange(AddressType.Ro,  0xe0000000,0xe4000000),
             new AddressRange(AddressType.Ro,  0xe8000000,0xea000000),
             new AddressRange(AddressType.Ro,  0xf4000000,0xf6000000),
             new AddressRange(AddressType.Ro,  0xf6000000,0xf6800000),
             new AddressRange(AddressType.Ro,  0xf8000000,0xfb000000),
             new AddressRange(AddressType.Ro,  0xfb000000,0xfb800000),
             new AddressRange(AddressType.Rw,  0xfffe0000,0xffffffff)
        };*/

        public static AddressType rangeCheck(UInt32 address)
        {
            int id = rangeCheckId(address);
            if (id == -1)
                return AddressType.Unknown;
            else
                return ValidAreas[id].description;
        }

        public static int rangeCheckId(UInt32 address)
        {
            for (int i = 0; i < ValidAreas.Length; i++)
            {
                AddressRange range = ValidAreas[i];
                if (range == null)
                    break;
                if (address >= range.low && address < range.high)
                    return i;
            }
            return -1;
        }

        public static bool validAddress(UInt32 address, bool debug)
        {
            if (debug)
                return true;            
            return (rangeCheckId(address) >= 0);
        }

        public static bool validAddress(UInt32 address)
        {
            return validAddress(address, addressDebug);
        }

        public static bool validRange(UInt32 low, UInt32 high, bool debug)
        {
            if (debug) 
                return true;
            return (rangeCheckId(low) == rangeCheckId(high-1));
        }

        public static bool validRange(UInt32 low, UInt32 high)
        {
            return validRange(low, high, addressDebug);
        }

        public static AddressType GetType(uint type)
        {
            if (type == 4 || type == 10)
                return AddressType.Ex;
            return AddressType.Rw;
        }

        public static void setDataUpper(TCPGecko upper)
        {
            UInt32[] data = upper.MemoryRegionRequest();

            if (data == null)
                return;

            for (int i = 0, j = 0; i < data.Length; i += 3, j++)
            {
                uint addr = data[i];
                uint size = data[i + 1];
                uint type = data[i + 2];
                ValidAreas[j] = new AddressRange(GetType(type), addr, addr + size);
            }
        }
    }
}
