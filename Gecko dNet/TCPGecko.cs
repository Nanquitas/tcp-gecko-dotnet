#define DIRECT

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms.VisualStyles;
using Ionic.Zip;
using System.Linq;

namespace TCPTCPGecko
{
    public class ByteSwap
    {
        public static UInt16 Swap(UInt16 input)
        {
            if (BitConverter.IsLittleEndian)
                return ((UInt16)(
                    ((0xFF00 & input) >> 8) |
                    ((0x00FF & input) << 8)));
            else
                return input;
        }

        public static UInt32 Swap(UInt32 input)
        {
            if (BitConverter.IsLittleEndian)
                return ((UInt32)(
                    ((0xFF000000 & input) >> 24) |
                    ((0x00FF0000 & input) >> 8) |
                    ((0x0000FF00 & input) << 8) |
                    ((0x000000FF & input) << 24)));
            else
                return input;
        }

        public static UInt64 Swap(UInt64 input)
        {
            if (BitConverter.IsLittleEndian)
                return ((UInt64)(
                    ((0xFF00000000000000 & input) >> 56) |
                    ((0x00FF000000000000 & input) >> 40) |
                    ((0x0000FF0000000000 & input) >> 24) |
                    ((0x000000FF00000000 & input) >> 8) |
                    ((0x00000000FF000000 & input) << 8) |
                    ((0x0000000000FF0000 & input) << 24) |
                    ((0x000000000000FF00 & input) << 40) |
                    ((0x00000000000000FF & input) << 56)));
            else
                return input;
        }
    }

    public class Dump
    {
        public Dump(UInt32 theStartAddress, UInt32 theEndAddress)
        {
            Construct(theStartAddress, theEndAddress, 0);
        }

        public Dump(UInt32 theStartAddress, UInt32 theEndAddress, int theFileNumber)
        {
            Construct(theStartAddress, theEndAddress, theFileNumber);
        }

        private void Construct(UInt32 theStartAddress, UInt32 theEndAddress, int theFileNumber)
        {
            startAddress = theStartAddress;
            endAddress = theEndAddress;
            readCompletedAddress = theStartAddress;
            mem = new Byte[endAddress - startAddress];
            fileNumber = theFileNumber;            
        }
        public UInt32 ReadAddress32(UInt32 addressToRead)
        {
            //dumpStream.Seek(addressToRead - startAddress, SeekOrigin.Begin);
            //byte [] buffer = new byte[4];

            //dumpStream.Read(buffer, 0, 4);
            if (addressToRead < startAddress) return 0;
            if (addressToRead > endAddress - 4) return 0;
            Byte[] buffer = new Byte[4];
            Buffer.BlockCopy(mem, index(addressToRead), buffer, 0, 4);
            //GeckoApp.SubArray<byte> buffer = new GeckoApp.SubArray<byte>(mem, (int)(addressToRead - startAddress), 4);

            //Read buffer
            UInt32 result = BitConverter.ToUInt32(buffer, 0);

            //Swap to machine endianness and return
            return result;//ByteSwap.Swap(result);
        }

        private int index(UInt32 addressToRead)
        {
            return (int)(addressToRead - startAddress);
        }

        public UInt32 ReadAddress(UInt32 addressToRead, int numBytes)
        {
            if (addressToRead < startAddress) return 0;
            if (addressToRead > endAddress - numBytes) return 0;
            
            Byte[] buffer = new Byte[4];
            Buffer.BlockCopy(mem, index(addressToRead), buffer, 0, numBytes);

            //Read buffer
            switch (numBytes)
            {
                case 4:
                    UInt32 result = BitConverter.ToUInt32(buffer, 0);

                    //Swap to machine endianness and return
                    return result;//ByteSwap.Swap(result);

                case 2:
                    UInt16 result16 = BitConverter.ToUInt16(buffer, 0);

                    //Swap to machine endianness and return
                    return result16;//ByteSwap.Swap(result16);

                default:
                    return buffer[0];
            }
        }

        public void WriteStreamToDisk()
        {
            string myDirectory = Environment.CurrentDirectory + @"\searchdumps\";
            if (!Directory.Exists(myDirectory))
            {
                Directory.CreateDirectory(myDirectory);
            }
            string myFile = myDirectory + "dump" + fileNumber.ToString() + ".dmp";

            WriteStreamToDisk(myFile);
        }

        public void WriteStreamToDisk(string filepath)
        {
            FileStream foo = new FileStream(filepath, FileMode.Create);
            foo.Write(mem, 0, (int)(endAddress-startAddress));
            foo.Close();
            foo.Dispose();
        }

        public void WriteCompressedStreamToDisk(string filepath)
        {
            ZipFile foo = new ZipFile(filepath);
            foo.AddEntry("mem", mem);
            foo.Dispose();
        }

        public Byte[] mem;
        private UInt32 startAddress;
        public UInt32 StartAddress
        {
            get { return startAddress; }
        }
        private UInt32 endAddress;
        public UInt32 EndAddress
        {
            get { return endAddress; }
        }
        private UInt32 readCompletedAddress;
        public UInt32 ReadCompletedAddress
        {
            get { return readCompletedAddress; }
            set { readCompletedAddress = value; }
        }
        private int fileNumber;
    }

    public enum ETCPErrorCode {
        FTDIQueryError,
        noFTDIDevicesFound,
        noTCPGeckoFound,
        FTDIResetError,
        FTDIPurgeRxError,
        FTDIPurgeTxError,
        FTDITimeoutSetError,
        FTDITransferSetError,
        FTDICommandSendError,
        FTDIReadDataError,
        FTDIWriteDataError,
        FTDIInvalidAddress,
        FTDIInvalidReply,
        TooManyRetries,
        REGStreamSizeInvalid,
        CheatStreamSizeInvalid
    }

    public enum FTDICommand {
        CmdResultError,
        CmdFatalError,
        CmdOk
    }


    public enum WiiStatus {
        Running,
        Paused,
        Breakpoint,
        Loader,
        Unknown
    }

    public enum WiiLanguage {
        NoOverride,
        Japanese,
        English,
        German,
        French,
        Spanish,
        Italian,
        Dutch,
        ChineseSimplified,
        ChineseTraditional,
        Korean
    }
    public enum WiiPatches {
        NoPatches,
        PAL60,
        VIDTV,
        PAL60VIDTV,
        NTSC,
        NTSCVIDTV,
        PAL50,
        PAL50VIDTV
    }
    public enum WiiHookType {
        VI,
        WiiRemote,
        GamecubePad
    }

    public delegate void GeckoProgress(UInt32 address, UInt32 currentchunk, UInt32 allchunks, UInt32 transferred, UInt32 length, bool okay, bool dump);

    public class ETCPGeckoException : Exception
    {
        private ETCPErrorCode PErrorCode;
        public ETCPErrorCode ErrorCode 
        {
            get 
            {
                return PErrorCode;
            }
        }

        public ETCPGeckoException(ETCPErrorCode code)
            : base()            
        {
            PErrorCode = code;
        }
        public ETCPGeckoException(ETCPErrorCode code, string message)
            : base(message)
        {
            PErrorCode = code;
        }
        public ETCPGeckoException(ETCPErrorCode code, string message, Exception inner)
            : base(message, inner)
        {
            PErrorCode = code;
        }
    }

    public class TCPGecko
    {
        private tcpconn PTCP;
        private readonly object _networkInUse = new object();
        private static volatile TCPGecko _instance;

        public static TCPGecko Instance => _instance;

        #region base constants

        private const UInt32 packetsize = 0x1000;// 0x400;
        private const UInt32 uplpacketsize = 0x1000;//0x400;

        private const Byte      cmd_poke08 = 0x01;
        private const Byte      cmd_poke16 = 0x02;
        private const Byte     cmd_pokemem = 0x03;
        private const Byte     cmd_readmem = 0x04;
        private const Byte       cmd_pause = 0x06;
        private const Byte    cmd_unfreeze = 0x07;
        private const Byte  cmd_breakpoint = 0x09;
        private const Byte   cmd_writekern = 0x0b;
        private const Byte    cmd_readkern = 0x0c;
        private const Byte cmd_breakpointx = 0x10;
        private const Byte    cmd_sendregs = 0x2F;
        private const Byte     cmd_getregs = 0x30;
        private const Byte    cmd_cancelbp = 0x38;
        private const Byte  cmd_sendcheats = 0x40;
        private const Byte      cmd_upload = 0x41;
        private const Byte        cmd_hook = 0x42;
        private const Byte   cmd_hookpause = 0x43;
        private const Byte        cmd_step = 0x44;
        private const Byte      cmd_status = 0x50;
        private const Byte  cmd_title_type = 0x51;
        private const Byte    cmd_title_id = 0x52;
        private const Byte    cmd_game_pid = 0x53;
        private const Byte   cmd_game_name = 0x54;
        private const Byte cmd_patch_wireless = 0x55;
        private const Byte cmd_add_cheat = 0x56;
        private const Byte cmd_delete_cheat = 0x57;
        private const Byte cmd_enable_cheat = 0x58;
        private const Byte cmd_disable_cheat = 0x59;
        private const Byte cmd_list_cheats = 0x60;
        private const Byte   cmd_cheatexec = 0x9E;
        private const Byte         cmd_rpc = 0x70;
        private const Byte cmd_nbreakpoint = 0x89;
        private const Byte     cmd_version = 0x99;
        private const Byte  cmd_os_version = 0x9A;
        private const Byte cmd_kern_version = 0x9B;
        private const Byte cmd_list_region = 0x9C;
        private const Byte   cmd_fetch_log = 0x9D;

        private const Byte         GCBPHit = 0x11;
        private const Byte           GCACK = 0xAA;
        private const Byte         GCRETRY = 0xBB;
        private const Byte          GCFAIL = 0xCC;
        private const Byte          GCDONE = 0xFF;

        private const Byte       BlockZero = 0xB0;
        private const Byte    BlockNonZero = 0xBD;

        private const Byte        GCWiiVer = 0x80;
        private const Byte        GCNgcVer = 0x81;
        private const Byte       GCWiiUVer = 0x82;
        private const Byte     N3DSVersion = 0x83;

        private static readonly Byte[] GCAllowedVersions = new Byte[] { GCWiiUVer };

        private const Byte       BPExecute = 0x03;
        private const Byte          BPRead = 0x05;
        private const Byte         BPWrite = 0x06;
        private const Byte     BPReadWrite = 0x07;
        #endregion

        private event GeckoProgress PChunkUpdate;

        public event GeckoProgress chunkUpdate
        {
            add
            {
                PChunkUpdate += value;
            }
            remove
            {
                PChunkUpdate -= value;
            }
        }

        private bool PConnected;

        public bool connected
        {
            get
            {
                return PConnected;
            }
        }

        private bool PCancelDump;

        public bool CancelDump
        {
            get
            {
                return PCancelDump;
            }
            set
            {
                PCancelDump = value;
            }
        }

        public string Host
        {
            get
            {
                return PTCP.Host;
            }
            set
            {
                if (!PConnected)
                {
                    PTCP = new tcpconn(value, PTCP.Port);
                }
            }
        }

        public int Port
        {
            get { return PTCP.Port; }
            set {
                if (!PConnected)
                {
                    PTCP = new tcpconn(PTCP.Host, value);
                }
            }
        }

        public TCPGecko(string host, int port)
        {
            PTCP = new tcpconn(host, port);
            PConnected = false;
            PChunkUpdate = null;
            _instance = this;
        }

        ~ TCPGecko()
        {
            if (PConnected)
                Disconnect();
        }

        protected bool InitGecko()
        {
            //Reset device
            return true;
        }

        public bool Connect()
        {
			if(PConnected)
                Disconnect();

            PConnected = false;

           //Open TCP Gecko
            try
            {
                PTCP.Connect();
                /*Byte[] init = new Byte[1];
                if (GeckoRead(init, 1) != FTDICommand.CmdOk || init[0] != 1)
                    throw new IOException("init byte missing");*/
            }
            catch (IOException)
            {
				// Don't disconnect if there's nothing connected
                Disconnect();
				throw new ETCPGeckoException(ETCPErrorCode.noTCPGeckoFound);
            }

            //Initialise TCP Gecko
           if (InitGecko())
            {
               System.Threading.Thread.Sleep(150);
                PConnected = true;
                return true;
            }
            else
                return false;
        }

        public void Disconnect()
        {
            PConnected = false;
            PTCP.Close();
        }

        protected FTDICommand GeckoRead(Byte[] recbyte, UInt32 nobytes)
        {
            lock (_networkInUse)
            {
                UInt32 bytes_read = 0;

                try
                {
                    PTCP.Read(recbyte, nobytes, ref bytes_read);
                }
                catch (IOException)
                {
                    Disconnect();
                    return FTDICommand.CmdFatalError; // fatal error
                }
                if (bytes_read != nobytes)
                {
                    return FTDICommand.CmdResultError; // lost bytes in transmission
                }

                return FTDICommand.CmdOk;
            }
        }

        protected FTDICommand GeckoWrite(Byte[] sendbyte, Int32 nobytes)
        {
            lock (_networkInUse)
            {
                UInt32 bytes_written = 0;

                try
                {
                    PTCP.Write(sendbyte, nobytes, ref bytes_written);
                }
                catch (IOException)
                {
                    Disconnect();
                    return FTDICommand.CmdFatalError; // fatal error
                }
                if (bytes_written != nobytes)
                {
                    return FTDICommand.CmdResultError; // lost bytes in transmission
                }

                return FTDICommand.CmdOk;
            }
        }

        protected FTDICommand GeckoWrite(UInt32[] array, uint size = 0)
        {
            int max = size == 0 ? array.Length : (int)size;
            int nobytes = max * 4;
            byte[] sendbyte = new byte[nobytes];

            for (int i = 0; i < max; i++)
            {
                sendbyte[i * 4] = BitConverter.GetBytes(array[i])[0];
                sendbyte[i * 4 + 1] = BitConverter.GetBytes(array[i])[1];
                sendbyte[i * 4 + 2] = BitConverter.GetBytes(array[i])[2];
                sendbyte[i * 4 + 3] = BitConverter.GetBytes(array[i])[3];
            }

            lock (_networkInUse)
            {
                UInt32 bytesWritten = 0;

                try
                {
                    PTCP.Write(sendbyte, nobytes, ref bytesWritten);
                }
                catch (IOException)
                {
                    Disconnect();
                    return FTDICommand.CmdFatalError; // fatal error
                }
                if (bytesWritten != nobytes)
                    return FTDICommand.CmdResultError; // lost bytes in transmission

                return FTDICommand.CmdOk;
            }
        }

        //Send update on a running process to the parent class
        protected void SendUpdate(UInt32 address, UInt32 currentchunk, UInt32 allchunks, UInt32 transferred, UInt32 length, bool okay, bool dump)
        {
            PChunkUpdate?.Invoke(address, currentchunk, allchunks, transferred, length, okay, dump);
        }

        public void Dump(Dump dump)
        {
            //Stream[] tempStream = { dump.dumpStream, dump.getOutputStream() };
            //Stream[] tempStream = { dump.dumpStream };
            //Dump(dump.startAddress, dump.endAddress, tempStream);
            //dump.getOutputStream().Dispose();
            //dump.WriteStreamToDisk();
            Dump(dump.StartAddress, dump.EndAddress, dump);
        }

        public void Dump(UInt32 startdump, UInt32 enddump, Stream saveStream)
        {
            Stream [] tempStream = { saveStream };
            Dump(startdump, enddump, tempStream);
        }

        private bool CheckDumpStatus()
        {
            lock (_networkInUse)
            {
                byte[] response = new byte[1];

                if (GeckoRead(response, 1) != FTDICommand.CmdOk)
                {
                    //Major fail, give it up
                    GeckoWrite(BitConverter.GetBytes(GCFAIL), 1);
                    throw new ETCPGeckoException(ETCPErrorCode.FTDIReadDataError);
                }

                if (response[0] == GCFAIL)
                    return false;
                return true;
            }
        }

        public void Dump(UInt32 startdump, UInt32 enddump, Stream[] saveStream)
        {
            lock (_networkInUse)
            {

                // Ensure boundaries
                if (GeckoApp.ValidMemory.rangeCheckId(startdump) != GeckoApp.ValidMemory.rangeCheckId(enddump))
                    enddump = GeckoApp.ValidMemory.ValidAreas[GeckoApp.ValidMemory.rangeCheckId(startdump)].high;

                if (!GeckoApp.ValidMemory.validAddress(startdump)) return;

                //How many bytes of data have to be transferred
                int memlength = (int)(enddump - startdump);


                // Send the read memory command to client
                if (GeckoWrite(BitConverter.GetBytes(cmd_readmem), 1) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                //Now let's send the dump information
                UInt32[] GeckoMemRange = new UInt32[2] {startdump, enddump};

                if (GeckoWrite(GeckoMemRange) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);




                // Reset cancel flag
                bool done = false;
                CancelDump = false;

                Byte[] buffer = new Byte[packetsize]; //read buffer

                uint transfered = 0;
                uint chunkCount = (uint)(memlength/packetsize);

                if ((uint) (memlength%packetsize) > 0)
                    chunkCount++;

                uint chunk = 0;
                int mlength = memlength;
                while (memlength > 0 && !done)
                {
                    //No output yet availible
                    SendUpdate(startdump + transfered, chunk, chunkCount, transfered, (uint)mlength, true, true);

                    // Check dump status
                    if (!CheckDumpStatus())
                        throw new ETCPGeckoException(ETCPErrorCode.FTDIReadDataError);

                    //Set buffer
                    Byte[] response = new Byte[1];
                    if (GeckoRead(response, 1) != FTDICommand.CmdOk)
                    {
                        //Major fail, give it up
                        GeckoWrite(BitConverter.GetBytes(GCFAIL), 1);
                        throw new ETCPGeckoException(ETCPErrorCode.FTDIReadDataError);
                    }

                    Byte reply = response[0];

                    if (reply == BlockZero)
                    {
                        // Send that everything is correct
                        GeckoWrite(BitConverter.GetBytes(GCACK), 1);

                        uint length = (uint)memlength < packetsize ? (uint)memlength : packetsize;

                        // Create the zero filled zone
                        for (int i = 0; i < length; i++)
                        {
                            buffer[i] = 0;
                        }

                        // Write received package to output stream
                        foreach (Stream stream in saveStream)
                        {
                            stream.Write(buffer, 0, ((Int32)length));
                        }

                        memlength -= (int)length;
                        chunk++;
                        transfered += length;
                    }
                    else
                    {
                        // Get the length to read
                        if (GeckoRead(buffer, 4) != FTDICommand.CmdOk)
                            throw new ETCPGeckoException(ETCPErrorCode.FTDIReadDataError);

                        UInt32 length = BitConverter.ToUInt32(buffer, 0);

                        // Read data
                        if (GeckoRead(buffer, length) != FTDICommand.CmdOk)
                        {
                            GeckoWrite(BitConverter.GetBytes(GCFAIL), 1);
                            throw new ETCPGeckoException(ETCPErrorCode.FTDIReadDataError);
                        }

                        // Send that data is correctly received
                        GeckoWrite(BitConverter.GetBytes(GCACK), 1);

                        memlength -= (int)length;
                        transfered += length;
                        chunk++;

                        try
                        {
                            // Write received package to output stream
                            foreach (Stream stream in saveStream)
                            {
                                stream.Write(buffer, 0, ((Int32) length));
                            }
                        }
                        catch (Exception ex)
                        {
                            GeckoApp.Program.Logger.Error(ex);
                        }


                        if (CancelDump)
                        {
                            // User requested a cancel
                            GeckoWrite(BitConverter.GetBytes(GCFAIL), 1);
                            done = true;
                        }
                    }
                    SendUpdate(startdump + transfered, chunk, chunkCount, transfered, (uint)mlength, true, true);
                }
            }
        }


        public void Dump(UInt32 startdump, uint enddump, Dump memdump)
        {
            lock (_networkInUse)
            {

                // Ensure boundaries
                if (GeckoApp.ValidMemory.rangeCheckId(startdump) != GeckoApp.ValidMemory.rangeCheckId(enddump))
                    enddump = GeckoApp.ValidMemory.ValidAreas[GeckoApp.ValidMemory.rangeCheckId(startdump)].high;

                if (!GeckoApp.ValidMemory.validAddress(startdump)) return;

                //How many bytes of data have to be transferred
                int memlength = (int)(enddump - startdump);


                // Send the read memory command to client
                if (GeckoWrite(BitConverter.GetBytes(cmd_readmem), 1) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                //Now let's send the dump information
                UInt32[] GeckoMemRange = new UInt32[2] { startdump, enddump };

                if (GeckoWrite(GeckoMemRange) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);


                // Reset cancel flag
                bool done = false;
                CancelDump = false;

                Byte[] buffer = new Byte[packetsize]; //read buffer

                uint transfered = 0;
                uint chunkCount = (uint)(memlength / packetsize);

                if ((uint)(memlength % packetsize) > 0)
                    chunkCount++;

                uint chunk = 0;
                int mlength = memlength;
                while (memlength > 0 && !done)
                {
                    //No output yet availible
                    SendUpdate(startdump + transfered, chunk, chunkCount, transfered, (uint)mlength, true, true);

                    // Check dump status
                    if (!CheckDumpStatus())
                    {
                        throw new ETCPGeckoException(ETCPErrorCode.FTDIReadDataError);
                    }
                        

                    //Set buffer
                    Byte[] response = new Byte[1];
                    if (GeckoRead(response, 1) != FTDICommand.CmdOk)
                    {
                        //Major fail, give it up
                        GeckoWrite(BitConverter.GetBytes(GCFAIL), 1);
                        throw new ETCPGeckoException(ETCPErrorCode.FTDIReadDataError);
                    }

                    Byte reply = response[0];

                    if (reply == BlockZero)
                    {

                        // Send that everything is correct
                        GeckoWrite(BitConverter.GetBytes(GCACK), 1);

                        uint length = (uint)memlength < packetsize ? (uint)memlength : packetsize;
                        // Create the zero filled zone
                        for (int i = 0; i < length; i++)
                        {
                            buffer[i] = 0;
                        }

                        memlength -= (int)length;
                        chunk++;

                        Buffer.BlockCopy(buffer, 0, memdump.mem,
                            (int)(transfered), (int)length);

                        transfered += length;
                        memdump.ReadCompletedAddress = startdump + transfered;
                    }
                    else
                    {
                        // Get the length to read
                        if (GeckoRead(buffer, 4) != FTDICommand.CmdOk)
                        {
                            throw new ETCPGeckoException(ETCPErrorCode.FTDIReadDataError);
                        }

                        UInt32 length = BitConverter.ToUInt32(buffer, 0);

                        // Read data
                        if (GeckoRead(buffer, length) != FTDICommand.CmdOk)
                        {
                            GeckoWrite(BitConverter.GetBytes(GCFAIL), 1);
                            throw new ETCPGeckoException(ETCPErrorCode.FTDIReadDataError);
                        }

                        // Send that data is correctly received
                        GeckoWrite(BitConverter.GetBytes(GCACK), 1);

                        memlength -= (int)length;
                        chunk++;

                        Buffer.BlockCopy(buffer, 0, memdump.mem,
                            (int) (transfered), (int) length);

                        transfered += length;
                        memdump.ReadCompletedAddress = startdump + transfered;

                        if (CancelDump)
                        {
                            // User requested a cancel
                            GeckoWrite(BitConverter.GetBytes(GCFAIL), 1);
                            done = true;
                        }
                    }
                    SendUpdate(startdump + transfered, chunk, chunkCount, transfered, (uint)mlength, true, true);
                }
            }
        }

        public void Upload(UInt32 startupload, UInt32 endupload, Stream sendStream)
        {
            {
                if (endupload < startupload)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDIInvalidAddress, "End address can't be lower than start address.");

                lock (_networkInUse)
                {
                    // How many bytes of data have to be transferred
                    uint memlength = endupload - startupload;

                    // How many chunks do I need to split this data into
                    // How big ist the last chunk
                    uint chunkcount = memlength / uplpacketsize;

                    if (memlength % uplpacketsize > 0)
                        chunkcount++;

                    UInt32[] geckoMemRange = { startupload, endupload };

                    // Send command
                    if (GeckoWrite(BitConverter.GetBytes(cmd_upload), 1) != FTDICommand.CmdOk)
                        throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                    // Now let's send the upload information
                    if (GeckoWrite(geckoMemRange) != FTDICommand.CmdOk)
                        throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                    byte[] buffer = new byte[uplpacketsize];
                    uint transfered = 0;
                    int restlength = (int)memlength;
                    uint chunk = 0;

                    while (restlength > 0)
                    {
                        // Update progress
                        SendUpdate(startupload + transfered, chunk, chunkcount, transfered, memlength, true, false);

                        uint length = restlength > uplpacketsize ? uplpacketsize : (uint)restlength;

                        // Read buffer from stream
                        sendStream.Read(buffer, 0, (int)length);

                        if (GeckoWrite(buffer, (int)length) != FTDICommand.CmdOk)
                        {
                            throw new ETCPGeckoException(ETCPErrorCode.FTDIWriteDataError);
                        }
                        chunk++;
                        transfered += length;
                        restlength -= (int)length;
                    }

                    // Check the good ending of the upload
                    byte[] response = new byte[1];

                    if (GeckoRead(response, 1) != FTDICommand.CmdOk)
                        throw new ETCPGeckoException(ETCPErrorCode.FTDIReadDataError);

                    if (response[0] != GCACK)
                        throw new ETCPGeckoException(ETCPErrorCode.FTDIInvalidReply);

                    // Update with finished stage
                    SendUpdate(startupload + transfered, chunk, chunkcount, transfered, memlength, true, false);
                }
            }
        }

        public bool Reconnect()
        {
            Disconnect();
            try
            {
                return Connect();
            }
            catch
            {
                return false;
            }
        }

        //Allows sending a basic one byte command to the Wii
        public FTDICommand RawCommand(Byte id)
        {
            return GeckoWrite(BitConverter.GetBytes(id), 1);
        }

        //Pauses the game
        public void Pause()
        {
            lock (_networkInUse)
            {
                //Only needs to send a cmd_pause to Wii
                if (RawCommand(cmd_pause) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);
            }
        }

        // Tries to repeatedly pause the game until it succeeds
        public void SafePause()
        {
            bool WasRunning = (status() == WiiStatus.Running);
            while (WasRunning)
            {
                Pause();
                System.Threading.Thread.Sleep(100);
                // Sometimes, the game doesn't actually pause...
                // So loop repeatedly until it does!
                WasRunning = (status() == WiiStatus.Running);
            }
        }

        //Unpauses the game
        public void Resume()
        {
            lock (_networkInUse)
            {
                //Only needs to send a cmd_unfreeze to Wii
                if (RawCommand(cmd_unfreeze) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);
            }
        }

        // Tries repeatedly to resume the game until it succeeds
        public void SafeResume()
        {
            bool NotRunning = (status() != WiiStatus.Running);
            int failCounter = 0;
            while (NotRunning && failCounter < 10)
            {
                Resume();
                System.Threading.Thread.Sleep(100);
                // Sometimes, the game doesn't actually resume...
                // So loop repeatedly until it does!
                try
                {
                    NotRunning = (status() != WiiStatus.Running);
                }
                catch (TCPTCPGecko.ETCPGeckoException ex)
                {
                    NotRunning = true;
                    failCounter++;
                }
            }
        }

        //Sends a GCFAIL to the game.. in case the Gecko handler hangs.. sendfail might solve it!
        public void sendfail()
        {
            //Only needs to send a cmd_unfreeze to Wii
            //Ignores the reply, send this command multiple times!
            RawCommand(GCFAIL);
        }

        #region poke commands
        //Poke a 32 bit value - note: address and value must be all in endianness of sending platform
        public void poke(UInt32 address, UInt32 value)
        {
            lock (_networkInUse)
            {
                //Lower address
                address &= 0xFFFFFFFC;

                //value = send [address in big endian] [value in big endian]
                UInt32[] PokeVal = new UInt32[2] {address, value};

                //Send poke
                if (RawCommand(cmd_pokemem) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                //write value
                if (GeckoWrite(PokeVal) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);
            }
        }

        //Copy of poke, just poke32 to make clear it is a 32-bit poke
        public void poke32(UInt32 address, UInt32 value)
        {
            poke(address, value);
        }
        
        //Poke a 16 bit value - note: address and value must be all in endianness of sending platform
        public void poke16(UInt32 address, UInt16 value)
        {
            lock (_networkInUse)
            {
                //Lower address
                address &= 0xFFFFFFFE;

                UInt32[] PokeVal = new UInt32[2] { address, value };

                //Send poke16
                if (RawCommand(cmd_poke16) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                //write value
                if (GeckoWrite(PokeVal) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);
            }
        }

        //Poke a 08 bit value - note: address and value must be all in endianness of sending platform
        public void poke08(UInt32 address, Byte value)
        {
            lock (_networkInUse)
            {
                //value = send [address in big endian] [value in big endian]
                UInt32[] PokeVal = new UInt32[2] { address, value };

                //Send poke08
                if (RawCommand(cmd_poke08) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                //write value
                if (GeckoWrite(PokeVal) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);
            }
        }
        #endregion

        #region kern commands
        //Poke a 32 bit value to kernel. note: address and value must be all in endianness of sending platform
        public void poke_kern(UInt32 address, UInt32 value)
        {
            lock (_networkInUse)
            {
                //value = send [address in big endian] [value in big endian]
                UInt64 PokeVal = (((UInt64) address) << 32) | ((UInt64) value);

                PokeVal = ByteSwap.Swap(PokeVal);

                //Send poke
                if (RawCommand(cmd_writekern) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                //write value
                if (GeckoWrite(BitConverter.GetBytes(PokeVal), 8) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);
            }
        }

        //Read a 32 bit value from kernel. note: address must be all in endianness of sending platform
        public UInt32 peek_kern(UInt32 address)
        {
            lock (_networkInUse)
            {
                //value = send [address in big endian] [value in big endian]
                address = ByteSwap.Swap(address);

                //Send read
                if (RawCommand(cmd_readkern) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                //write value
                if (GeckoWrite(BitConverter.GetBytes(address), 4) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                Byte[] buffer = new Byte[4];
                if (GeckoRead(buffer, 4) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                return ByteSwap.Swap(BitConverter.ToUInt32(buffer, 0));
            }
        }
        #endregion

        //Returns the console status
        public WiiStatus status()
        {
            lock (_networkInUse)
            {
                System.Threading.Thread.Sleep(100);
                //Initialise Gecko
                if (!InitGecko())
                    throw new ETCPGeckoException(ETCPErrorCode.FTDIResetError);

                //Send status command
                if (RawCommand(cmd_status) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

//			System.Threading.Thread.Sleep(10);

                //Read status
                Byte[] buffer = new Byte[1];
                if (GeckoRead(buffer, 1) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDIReadDataError);

                //analyse reply
                switch (buffer[0])
                {
                    case 0:
                        return WiiStatus.Running;
                    case 1:
                        return WiiStatus.Paused;
                    case 2:
                        return WiiStatus.Breakpoint;
                    case 3:
                        return WiiStatus.Loader;
                    default:
                        return WiiStatus.Unknown;
                }
            }
        }

        //Step to the next frame
        public void Step() 
        {
            lock (_networkInUse)
            {
                //Reset buffers
                if (!InitGecko())
                    throw new ETCPGeckoException(ETCPErrorCode.FTDIResetError);

                //Send step command
                if (RawCommand(cmd_step) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);
            }
        }

        #region breakpoint crap
        //Initialise a basic data breakpoint
        //address = Which address should the breakpoint be added on
        //bptype = how many bytes need to be added to the 8 byte aligned address - 5 for read, 6 for write, 7 for rw
        //exact = only break if the exact address is being accessed
        protected void Breakpoint(UInt32 address, Byte bptype, bool exact)
        {
            InitGecko();

            UInt32 lowaddr = (address & 0xFFFFFFF8) | bptype; 
              //Actual address to put the breakpoint - the identity adder is applied to it

            bool useGeckoBP = false;
            if (exact)
                useGeckoBP = (VersionRequest() != GCNgcVer);

            if (!useGeckoBP) //classic PPC breakpoint
            {
                if (RawCommand(cmd_breakpoint) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                //Convert lowaddr to BigEndian
                UInt32 breakpaddr = ByteSwap.Swap(lowaddr);

                if (GeckoWrite(BitConverter.GetBytes(breakpaddr),4)!=FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);
            }
            else //advanced exact Gecko breakpoint
            {
                if (RawCommand(cmd_nbreakpoint) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                UInt64 breakpaddr = ((UInt64)lowaddr) << 32 | ((UInt64)address);
                breakpaddr = ByteSwap.Swap(breakpaddr);

                if (GeckoWrite(BitConverter.GetBytes(breakpaddr), 8) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);
            }
        }

        //Read breakpoint
        public void BreakpointR(UInt32 address, bool exact)
        {
            Breakpoint(address, BPRead, exact);
        }
        public void BreakpointR(UInt32 address)
        {
            Breakpoint(address, BPRead, true);
        }

        //Write breakpoint
        public void BreakpointW(UInt32 address, bool exact)
        {
            Breakpoint(address, BPWrite, exact);
        }
        public void BreakpointW(UInt32 address)
        {
            Breakpoint(address, BPWrite, true);
        }

        //Read/Write breakpoint
        public void BreakpointRW(UInt32 address, bool exact)
        {
            Breakpoint(address, BPReadWrite, exact);
        }
        public void BreakpointRW(UInt32 address)
        {
            Breakpoint(address, BPReadWrite, true);
        }

        
        //Execute breakpoints require a different command and different parameters
        //address = address to put the breakpoint on
        public void BreakpointX(UInt32 address)
        {
            InitGecko();

            //Unlike Data breakpoints Execute breakpoints are exact to 4 bytes
            UInt32 baddress = ByteSwap.Swap(((UInt32)(address & 0xFFFFFFFC) | BPExecute));

            //Send breakpoint execute command
            if (RawCommand(cmd_breakpointx) != FTDICommand.CmdOk)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

            //Send address to handler
            if(GeckoWrite(BitConverter.GetBytes(baddress),4) != FTDICommand.CmdOk)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);
        }

        //Returns true once a Breakpoint has hit
        //Function is depricated use status function instead - only for backwards compatibility with Delphi ports!
        public bool BreakpointHit()
        {
            Byte[] buffer = new Byte[1];
            
            if (GeckoRead(buffer, 1) != FTDICommand.CmdOk)
                return false;

            //did we receive a bphit signal?
            return (buffer[0] == GCBPHit);
        }

        //Cancels running breakpoints
        //doesn't work thanks to a malfunction of current gecko handlers!
        public void CancelBreakpoint()
        {
            if (RawCommand(cmd_cancelbp) != FTDICommand.CmdOk)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);
        }
#endregion

        //Is this version code a correct Gecko version?
        protected bool AllowedVersion(Byte version)
        {
            for (int i = 0; i < GCAllowedVersions.Length; i++)
                if (GCAllowedVersions[i] == version)
                    return true;
            return false;
        }

        public UInt32 VersionRequest()
        {
            lock (_networkInUse)
            {
                InitGecko();

                if (RawCommand(cmd_version) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                Byte[] buffer = new Byte[4];

                if (GeckoRead(buffer, 4) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                return BitConverter.ToUInt32(buffer, 0);
            }
        }

        public UInt32 OsVersionRequest()
        {
            lock (_networkInUse)
            {
                if (RawCommand(cmd_os_version) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                Byte[] buffer = new Byte[4];

                if (GeckoRead(buffer, 4) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                return ByteSwap.Swap(BitConverter.ToUInt32(buffer, 0));
            }
        }
        public UInt32 KernVersionRequest()
        {
            lock (_networkInUse)
            {
                if (RawCommand(cmd_kern_version) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                Byte[] buffer = new Byte[4];

                if (GeckoRead(buffer, 4) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                return ByteSwap.Swap(BitConverter.ToUInt32(buffer, 0));
            }
        }
        public UInt32 TitleTypeRequest()
        {
            lock (_networkInUse)
            {
                if (RawCommand(cmd_title_type) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                Byte[] buffer = new Byte[4];

                if (GeckoRead(buffer, 4) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                return BitConverter.ToUInt32(buffer, 0);
            }
        }
        public UInt32 TitleIDRequest()
        {
            lock (_networkInUse)
            {
                if (RawCommand(cmd_title_id) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                Byte[] buffer = new Byte[4];

                if (GeckoRead(buffer, 4) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                return BitConverter.ToUInt32(buffer, 0);
            }
        }

        public UInt32 GamePIDRequest()
        {
            lock (_networkInUse)
            {
                if (RawCommand(cmd_game_pid) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                Byte[] buffer = new Byte[4];

                if (GeckoRead(buffer, 4) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                return ByteSwap.Swap(BitConverter.ToUInt32(buffer, 0));
            }
        }
        public string GameNameRequest()
        {
            lock (_networkInUse)
            {
                string name = "";

                if (RawCommand(cmd_game_name) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                // Is there any log to fetch ?
                byte[] buffer = new byte[8];

                if (GeckoRead(buffer, 8) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                name = Encoding.UTF8.GetString(buffer);

                return name;
            }
        }

        public UInt32[] MemoryRegionRequest()
        {
            lock (_networkInUse)
            {
                // Fetch how many regions we have
                if (RawCommand(cmd_list_region) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                Byte[] buffer = new Byte[1];

                if (GeckoRead(buffer, 1) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                UInt32 count = buffer[0]; //ByteSwap.Swap(BitConverter.ToUInt32(buffer, 0));

                if (count <= 0)
                    return null;

                // Result format: UInt32[3] = {start_addr, size, type}
                UInt32[] regions = new UInt32[count*3];

                uint size = count*3*4;
                // Allocate buffer to receive data
                buffer = new byte[size];

                if (GeckoRead(buffer, size) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                for (int i = 0, j = 0; i < buffer.Length; i += 4, j++)
                {
                    regions[j] = BitConverter.ToUInt32(buffer, i);
                }

                return regions;
            }
        }

        public string LogRequest()
        {
            lock (_networkInUse)
            {
                string log = "";

                if (RawCommand(cmd_fetch_log) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                // Is there any log to fetch ?
                byte[] buffer = new byte[4];

                if (GeckoRead(buffer, 4) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                UInt32 logLen = BitConverter.ToUInt32(buffer, 0);
                if (logLen > 0)
                {
                    buffer = new byte[logLen];

                    if (GeckoRead(buffer, logLen) != FTDICommand.CmdOk)
                        throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                    log = Encoding.UTF8.GetString(buffer);
                }

                return log;
            }
        }
        public UInt32 peek(UInt32 address)
        {
            lock (_networkInUse)
            {
                if (!GeckoApp.ValidMemory.validAddress(address))
                {
                    return 0;
                }

                //address will be alligned to 4
                UInt32 paddress = address & 0xFFFFFFFC;

                //Create a memory stream for the actual dump
                MemoryStream stream = new MemoryStream();

                //make sure to not send data to the output
                GeckoProgress oldUpdate = PChunkUpdate;
                PChunkUpdate = null;

                try
                {
                    //dump data
                    Dump(paddress, paddress + 4, stream);

                    //go to beginning
                    stream.Seek(0, SeekOrigin.Begin);
                    Byte[] buffer = new Byte[4];
                    stream.Read(buffer, 0, 4);

                    //Read buffer
                    UInt32 result = BitConverter.ToUInt32(buffer, 0);

                    //Swap to machine endianness and return
                    //result = ByteSwap.Swap(result);

                    return result;
                }
                finally
                {
                    PChunkUpdate = oldUpdate;

                    //make sure the Stream is properly closed
                    stream.Close();
                }
            }
        }

        #region register operations
        //Read registers in breakpoint cases
        public void GetRegisters(Stream stream, uint contextAddress) 
        {
            UInt32 bytesExpected = 0x1B0;

            //Read registers
            MemoryStream buffer = new MemoryStream();
            Dump(contextAddress + 8, contextAddress + 8 + bytesExpected, buffer);

            byte[] bytes = buffer.ToArray();

            //Store registers to output stream!
            stream.Write(bytes, 0x80, 4); // cr
            stream.Write(bytes, 0x8c, 4); // xer
            stream.Write(bytes, 0x88, 4); // ctr
            stream.Write(new byte[8], 0, 8); // dsis, dar (dunno)
            stream.Write(bytes, 0x90, 8); // srr0, srr1
            stream.Write(bytes, 0x0, 4 * 32); // gprs
            stream.Write(bytes, 0x84, 4); // lr
            stream.Write(bytes, 0xb0, 8 * 32); // fprs
        }

        //Send registers
        public void SendRegisters(Stream sendStream, uint contextAddress)
        {
            MemoryStream buffer = new MemoryStream();
            byte[] bytes = new byte[0xA0];
            sendStream.Seek(0, SeekOrigin.Begin);
            sendStream.Read(bytes, 0, bytes.Length);
            buffer.Write(bytes, 0x1C, 4 * 32); // gprs
            buffer.Write(bytes, 0x0, 4); // cr
            buffer.Write(bytes, 0x9C, 4); // lr
            buffer.Write(bytes, 0x8, 4); //ctr
            buffer.Write(bytes, 0x4, 4); // xer
            buffer.Write(bytes, 0x14, 8); // srr0, srr1

            buffer.Seek(0, SeekOrigin.Begin);

            Upload(contextAddress + 8, contextAddress + 8 + 0x98, buffer);
        }
        #endregion

        #region Cheat related stuff
        private UInt64 readInt64(Stream inputstream)
        {
            Byte[] buffer = new Byte[8];
            inputstream.Read(buffer, 0, 8);
            UInt64 result = BitConverter.ToUInt64(buffer,0);
            result = ByteSwap.Swap(result);
            return result;
        }

        private void writeInt64(Stream outputstream, UInt64 value)
        {
            UInt64 bvalue = ByteSwap.Swap(value);
            Byte[] buffer = BitConverter.GetBytes(bvalue);
            outputstream.Write(buffer, 0, 8);
        }

        private void insertInto(Stream insertStream, UInt64 value)
        {
            MemoryStream tempstream = new MemoryStream();
            writeInt64(tempstream, value);
            insertStream.Seek(0, SeekOrigin.Begin);
            
            Byte[] streambuffer=new Byte[insertStream.Length];
            insertStream.Read(streambuffer,0, (Int32)insertStream.Length);
            tempstream.Write(streambuffer, 0, (Int32)insertStream.Length);

            insertStream.Seek(0, SeekOrigin.Begin);
            tempstream.Seek(0, SeekOrigin.Begin);

            streambuffer = new Byte[tempstream.Length];
            tempstream.Read(streambuffer, 0, (Int32)tempstream.Length);
            insertStream.Write(streambuffer, 0, (Int32)tempstream.Length);

            tempstream.Close();
        }

        public void sendCheats(Stream inputStream)
        {
            MemoryStream cheatStream = new MemoryStream();
            Byte[] orgData = new Byte[inputStream.Length];
            inputStream.Seek(0,SeekOrigin.Begin);
            inputStream.Read(orgData, 0, (Int32)inputStream.Length);
            cheatStream.Write(orgData, 0, (Int32)inputStream.Length);
            
            UInt32 length = (UInt32)cheatStream.Length;
            //Cheat stream length must be multiple of 8
            if (length % 8 != 0)
            {
                cheatStream.Close();
                throw new ETCPGeckoException(ETCPErrorCode.CheatStreamSizeInvalid);
            }

            //Reset buffers
            InitGecko();

            //Make sure the stream ends with F0/F1
            cheatStream.Seek(-8,SeekOrigin.End);
            UInt64 data = readInt64(cheatStream);
            data = data & 0xFE00000000000000;
            if ( (data != 0xF000000000000000) &&
                 (data != 0xFE00000000000000))
            {
                cheatStream.Seek(0, SeekOrigin.End);
                writeInt64(cheatStream, 0xF000000000000000);
            }

            //Make sure it starts with 00D0C0...
            cheatStream.Seek(0, SeekOrigin.Begin);
            data = readInt64(cheatStream);
            if (data != 0x00D0C0DE00D0C0DE)
            {
                insertInto(cheatStream, 0x00D0C0DE00D0C0DE);
            }

            cheatStream.Seek(0, SeekOrigin.Begin);

            length = (UInt32)cheatStream.Length;

            if (GeckoWrite(BitConverter.GetBytes(cmd_sendcheats), 1) != FTDICommand.CmdOk)
            {
                cheatStream.Close();
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);
            }
            
            //How many chunks do I need to split this data into
            //How big ist the last chunk
            UInt32 fullchunks = length / uplpacketsize;
            UInt32 lastchunk = length % uplpacketsize;

            //How many chunks do I need to transfer
            UInt32 allchunks = fullchunks;
            if (lastchunk > 0)
                allchunks++;

            //Read reply - expcecting GCACK
            Byte retry = 0;
            while (retry < 10)
            {
                Byte[] response = new Byte[1];
                if (GeckoRead(response, 1) != FTDICommand.CmdOk)
                {
                    cheatStream.Close();
                    throw new ETCPGeckoException(ETCPErrorCode.FTDIReadDataError);
                }
                Byte reply = response[0];
                if (reply == GCACK)
                    break;
                if (retry == 9)
                {
                    cheatStream.Close();
                    throw new ETCPGeckoException(ETCPErrorCode.FTDIInvalidReply);
                }
            }

            UInt32 blength = ByteSwap.Swap(length);
            if (GeckoWrite(BitConverter.GetBytes(blength), 4) != FTDICommand.CmdOk)
            {
                cheatStream.Close();
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);
            }

            //We start with chunk 0
            UInt32 chunk = 0;
            retry = 0;

            Byte[] buffer; //read buffer
            while (chunk < fullchunks)
            {
                //No output yet availible
                SendUpdate(0x00d0c0de, chunk, allchunks, chunk * packetsize, length, retry == 0, false);
                //Set buffer
                buffer = new Byte[uplpacketsize];
                //Read buffer from stream
                cheatStream.Read(buffer, 0, (int)uplpacketsize);
                FTDICommand returnvalue = GeckoWrite(buffer, (int)uplpacketsize);
                if (returnvalue == FTDICommand.CmdResultError)
                {
                    retry++;
                    if (retry >= 3)
                    {
                        //Give up, too many retries
                        GeckoWrite(BitConverter.GetBytes(GCFAIL), 1);
                        cheatStream.Close();
                        throw new ETCPGeckoException(ETCPErrorCode.TooManyRetries);
                    }
                    //Reset stream
                    cheatStream.Seek((-1) * ((int)uplpacketsize), SeekOrigin.Current);
                    GeckoWrite(BitConverter.GetBytes(GCRETRY), 1);
                    continue;
                }
                else if (returnvalue == FTDICommand.CmdFatalError)
                {
                    //Major fail, give it up
                    GeckoWrite(BitConverter.GetBytes(GCFAIL), 1);
                    cheatStream.Close();
                    throw new ETCPGeckoException(ETCPErrorCode.FTDIReadDataError);
                }

                Byte[] response = new Byte[1];
                returnvalue = GeckoRead(response, 1);
                if ((returnvalue == FTDICommand.CmdResultError) || (response[0] != GCACK))
                {
                    retry++;
                    if (retry >= 3)
                    {
                        //Give up, too many retries
                        GeckoWrite(BitConverter.GetBytes(GCFAIL), 1);
                        cheatStream.Close();
                        throw new ETCPGeckoException(ETCPErrorCode.TooManyRetries);
                    }
                    //Reset stream
                    cheatStream.Seek((-1) * ((int)uplpacketsize), SeekOrigin.Current);
                    GeckoWrite(BitConverter.GetBytes(GCRETRY), 1);
                    continue;
                }
                else if (returnvalue == FTDICommand.CmdFatalError)
                {
                    //Major fail, give it up
                    GeckoWrite(BitConverter.GetBytes(GCFAIL), 1);
                    cheatStream.Close();
                    throw new ETCPGeckoException(ETCPErrorCode.FTDIReadDataError);
                }
                
                //reset retry counter
                retry = 0;
                //next chunk
                chunk++;
                //ackowledge package
            }

            //Final package?
            while (lastchunk > 0)
            {
                //No output yet availible
                SendUpdate(0x00d0c0de, chunk, allchunks, chunk * packetsize, length, retry == 0,false);
                //Set buffer
                buffer = new Byte[lastchunk];
                //Read buffer from stream
                cheatStream.Read(buffer, 0, (int)lastchunk);
                FTDICommand returnvalue = GeckoWrite(buffer, (Int32)lastchunk);
                if (returnvalue == FTDICommand.CmdResultError)
                {
                    retry++;
                    if (retry >= 3)
                    {
                        //Give up, too many retries
                        GeckoWrite(BitConverter.GetBytes(GCFAIL), 1);
                        cheatStream.Close();
                        throw new ETCPGeckoException(ETCPErrorCode.TooManyRetries);
                    }
                    //Reset stream
                    cheatStream.Seek((-1) * ((int)lastchunk), SeekOrigin.Current);
                    GeckoWrite(BitConverter.GetBytes(GCRETRY), 1);
                    continue;
                }
                else if (returnvalue == FTDICommand.CmdFatalError)
                {
                    //Major fail, give it up
                    GeckoWrite(BitConverter.GetBytes(GCFAIL), 1);
                    cheatStream.Close();
                    throw new ETCPGeckoException(ETCPErrorCode.FTDIReadDataError);
                }
                //reset retry counter
                retry = 0;
                //cancel while loop
                lastchunk = 0;
                //ackowledge package
                //GeckoWrite(BitConverter.GetBytes(GCACK), 1);
            }
            SendUpdate(0x00d0c0de, allchunks, allchunks, length, length, true,false);
            cheatStream.Close();
        }

        //Execute cheats
        public void ExecuteCheats()
        {
            if (RawCommand(cmd_cheatexec) != FTDICommand.CmdOk)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);
        }
        #endregion

        #region hooking crap
        //Hook command:
        public void Hook(bool pause, WiiLanguage language, WiiPatches patches, WiiHookType hookType)
        {
            InitGecko();

            //Hookpause command or regular hook?
            Byte command;
            if (pause)
                command = cmd_hookpause;
            else
                command = cmd_hook;

            //Perform hook command
            command += (Byte)hookType;
            if (RawCommand(command) != FTDICommand.CmdOk)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

            //Send language
            if (language != WiiLanguage.NoOverride)
                command = (Byte)(language - 1);
            else
                command = 0xCD;

            if (RawCommand(command) != FTDICommand.CmdOk)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

            //Send patches
            command = (Byte)patches;
            if (RawCommand(command) != FTDICommand.CmdOk)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);
        }

        public void Hook()
        {
            Hook(false, WiiLanguage.NoOverride, WiiPatches.NoPatches, WiiHookType.VI);
        }
        #endregion

        #region Screenshot processing
        private static Byte ConvertSafely(double floatValue)
        {
            return (Byte)Math.Round(Math.Max(0, Math.Min(floatValue, 255)));
        }

        private static Bitmap ProcessImage(UInt32 width, UInt32 height, Stream analyze)
        {

            Bitmap BitmapRGB = new Bitmap((int)width, (int)height, PixelFormat.Format24bppRgb);
            BitmapData bData = BitmapRGB.LockBits(new Rectangle(0, 0, (int)width, (int)height),
                                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int size = bData.Stride * bData.Height;

            Byte[] data = new Byte[size];

            System.Runtime.InteropServices.Marshal.Copy(bData.Scan0, data, 0, size);

            Byte[] bufferBytes= new Byte[width * height * 2];

            int y = 0;
            int u = 0;
            int v = 0;
            int yvpos = 0;
            int rgbpos = 0;

            analyze.Read(bufferBytes, 0, (int)(width * height * 2));
            for (int i = 0; i < width*height; i++)
            {
                yvpos = i * 2;
                //YV encoding is a bit awkward!
                if (i % 2 == 0) //Even
                {
                    y = bufferBytes[yvpos];
                    u = bufferBytes[yvpos + 1]; //U value is taken from current V block
                    v = bufferBytes[yvpos + 3]; //Take V from next data YV block
                }
                else //Odd
                    y = bufferBytes[yvpos];
                    //u is taken from last pixel
                    //v too!

                rgbpos = (i * 3);
                    data[rgbpos] = ConvertSafely(1.164 * (y - 16) + 2.017 * (u - 128));                     //Blue pixel value
                data[rgbpos + 1] = ConvertSafely(1.164 * (y - 16) - 0.392 * (u - 128) - 0.813 * (v - 128)); //Greeen pixel value
                data[rgbpos + 2] = ConvertSafely(1.164 * (y - 16) + 1.596 * (v - 128));                     //Red pixel value
            }

            System.Runtime.InteropServices.Marshal.Copy(data, 0, bData.Scan0, data.Length);

            BitmapRGB.UnlockBits(bData);

            return BitmapRGB;
        }

        public Image Screenshot()
        {
            MemoryStream analyze;

            //Dump video registers
            analyze = new MemoryStream();
            Dump(0xCC002000, 0xCC002080, analyze);
            analyze.Seek(0, SeekOrigin.Begin);
            Byte[] viregs = new Byte[128];
            analyze.Read(viregs, 0, 128);
            analyze.Close();

            //Extract width, height and offset in memory
            UInt32  swidth = (UInt32)(viregs[0x49] << 3);
            UInt32 sheight = (UInt32)(((viregs[0] << 5) | (viregs[1] >> 3)) & 0x07FE);
            UInt32 soffset = (UInt32)((viregs[0x1D] << 16) | (viregs[0x1E] << 8) | viregs[0x1F]);
            if ( (viregs[0x1C] & 0x10) == 0x10)
                soffset <<= 5;
            soffset += 0x80000000;
            soffset -= (UInt32)((viregs[0x1C] & 0xF) << 3);

            //Dump video data
            analyze = new MemoryStream();
            Dump(soffset, soffset + sheight * swidth * 2, analyze);
            analyze.Seek(0, SeekOrigin.Begin);

            if (sheight > 600) //Progressive mode!
            {
                sheight = sheight / 2;
                swidth = swidth * 2;
            }

            Bitmap b = ProcessImage(swidth, sheight, analyze);
            analyze.Close();

            return b;
        }
        #endregion

        #region RPC

        /* values in host endianess. */
        public UInt32 rpc(UInt32 address, params UInt32[] args)
        {
            return (UInt32)(rpc64(address, args) >> 32);
        }

        /* values in host endianess. */
        public UInt64 rpc64(UInt32 address, params UInt32[] args)
        {
            Byte[] buffer = new Byte[4 + 8 * 4];

            //value = send [address in big endian] [value in big endian]
            address = ByteSwap.Swap(address);
            
            BitConverter.GetBytes(address).CopyTo(buffer, 0);

            for (int i = 0; i < 8; i++)
			{
                if (i < args.Length) {
                    BitConverter.GetBytes(ByteSwap.Swap(args[i])).CopyTo(buffer, 4 + i * 4);
                } else {
                    BitConverter.GetBytes(0xfecad0ba).CopyTo(buffer, 4 + i * 4);
                }
			}

            //Send read
            if (RawCommand(cmd_rpc) != FTDICommand.CmdOk)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);
            

            //write value
            if (GeckoWrite(buffer, buffer.Length) != FTDICommand.CmdOk)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

            if (GeckoRead(buffer, 8) != FTDICommand.CmdOk)
                throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

            return ByteSwap.Swap(BitConverter.ToUInt64(buffer, 0));
        }

        #endregion
        #region CHEATS
        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public int SendCheat(string cheat)
        {
            if (String.IsNullOrEmpty(cheat) || String.IsNullOrWhiteSpace(cheat))
                return (-1);

            uint[] c = new uint[0x100];
            uint size = 0;
            uint index = 0;
            string name = "";
            foreach (string line in cheat.Split(Environment.NewLine.ToCharArray()))
            {
                if (string.IsNullOrEmpty(line))
                    continue;
                if (line.Contains("["))
                {
                    name = line.Replace('[', ' ').Replace(']', ' ').Trim();
                    continue;
                }
                string[] commands = line.Split(' ');

                byte[] ar = StringToByteArray(commands[0]).Reverse().ToArray();
                uint u = BitConverter.ToUInt32(ar, 0);
                c[index++] = BitConverter.ToUInt32(StringToByteArray(commands[0]).Reverse().ToArray(), 0);
                c[index++] = BitConverter.ToUInt32(StringToByteArray(commands[1]).Reverse().ToArray(), 0);
                size += 8;
            }

            lock (_networkInUse)
            {
                // Send command byte
                if (RawCommand(cmd_add_cheat) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                // Send code's size
                if (GeckoWrite(BitConverter.GetBytes(size), 4) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDIWriteDataError);

                // Send code
                if (GeckoWrite(c, (size / 4)) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDIWriteDataError);

                byte[] bname = Encoding.UTF8.GetBytes(name);

                // Send name's size
                size = (uint) bname.Length;
                byte[] buffer = BitConverter.GetBytes(size);

                if (GeckoWrite(buffer, 4) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDIWriteDataError);

                // Send name
                if (size > 0)
                {
                    if (GeckoWrite(bname, (int)size) != FTDICommand.CmdOk)
                        throw new ETCPGeckoException(ETCPErrorCode.FTDIWriteDataError);
                }

                // Receive id of the cheat
                buffer = new byte[4];
                if (GeckoRead(buffer, 4) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDIReadDataError);

                return BitConverter.ToInt32(buffer, 0);

            }
        }

        public void RemoveCheat(int id)
        {
            lock (_networkInUse)
            {
                // Send command
                if (RawCommand(cmd_delete_cheat) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                // Send index
                if (GeckoWrite(BitConverter.GetBytes(id), 4) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDIWriteDataError);
            }
        }

        public void EnableCheat(int id)
        {
            lock (_networkInUse)
            {
                // Send command
                if (RawCommand(cmd_enable_cheat) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                // Send index
                if (GeckoWrite(BitConverter.GetBytes(id), 4) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDIWriteDataError);
            }
        }

        public void DisableCheat(int id)
        {
            lock (_networkInUse)
            {
                // Send command
                if (RawCommand(cmd_disable_cheat) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);

                // Send index
                if (GeckoWrite(BitConverter.GetBytes(id), 4) != FTDICommand.CmdOk)
                    throw new ETCPGeckoException(ETCPErrorCode.FTDIWriteDataError);
            }
        }

        public void ListCheats()
        {
            // Send command
           if (RawCommand(cmd_list_cheats) != FTDICommand.CmdOk)
           throw new ETCPGeckoException(ETCPErrorCode.FTDICommandSendError);
            
        }
        #endregion
    }
}