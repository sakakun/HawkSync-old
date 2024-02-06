using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace HawkSync_SM
{
    public class ServerManagement
    {
        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        const int PROCESS_WM_READ = 0x0010;
        const int PROCESS_VM_WRITE = 0x0020;
        const int PROCESS_VM_OPERATION = 0x0008;
        const int PROCESS_ALL_ACCESS = 0x1F0FFF;
        const int PROCESS_QUERY_INFORMATION = 0x0400;
        const uint WM_KEYDOWN = 0x0100;
        const uint WM_KEYUP = 0x0101;
        const int VK_ENTER = 0x0D;
        const uint console = 0x60;
        const uint GlobalChat = 0x54;

        public bool ProcessExist(int id)
        {
            Process[] processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                if (process.Id == id)
                {
                    return true;
                }
            }
            return false;
        }

        public void UpdateServerName(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            // Server Query Name
            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, (int)baseAddr + 0x001BF400, Ptr1, Ptr1.Length, ref Ptr1Read);
            int Ptr2 = BitConverter.ToInt32(Ptr1, 0);

            byte[] ServerName = new byte[31];
            int ServerNamePtrRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, (int)Ptr2 + 0x4, ServerName, ServerName.Length, ref ServerNamePtrRead);
            string ServerNameQuery = Encoding.Default.GetString(ServerName).Replace("\0", "");
            // end Server Query Name

            // Server Name Display
            byte[] Ptr3 = new byte[4];
            int Ptr3Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, (int)Ptr2 + 0x000A7088, Ptr3, Ptr3.Length, ref Ptr3Read);
            int ServerDisplayerName = BitConverter.ToInt32(Ptr3, 0);

            byte[] ServerNameDisplay = new byte[31];
            int ServerNameRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, (int)ServerDisplayerName + 0x30, ServerNameDisplay, ServerNameDisplay.Length, ref ServerNameRead);
            string ServerDisplayName = Encoding.Default.GetString(ServerNameDisplay).Replace("\0", "");
            // end Server Name Display

            // since either one or the other isn't what it should be.. just update them both. Call it a day.
            byte[] ServerNameBytes = Encoding.Default.GetBytes(_state.Instances[InstanceID].ServerName);
            int bytesWritten = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], ServerDisplayerName, ServerNameBytes, ServerNameBytes.Length, ref bytesWritten);
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr2, ServerNameBytes, ServerNameBytes.Length, ref bytesWritten);
        }
        public void UpdateCountryCode(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;


            // read country code Nova
            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, (int)baseAddr + 0x000AC7C8, Ptr1, Ptr1.Length, ref Ptr1Read);
            int Ptr2Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] Ptr2 = new byte[4];
            int Ptr2Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, (int)Ptr2Addr + 0x11, Ptr2, Ptr2.Length, ref Ptr2Read);
            string NovaCountryCode = Encoding.Default.GetString(Ptr2);


            byte[] Ptr3 = new byte[4];
            int Ptr3Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, (int)baseAddr + 0x000A7088, Ptr3, Ptr3.Length, ref Ptr3Read);
            int Ptr4 = BitConverter.ToInt32(Ptr3, 0);
            byte[] Ptr5Addr = new byte[4];
            int Ptr5AddrRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, (int)Ptr4 + 0x50 + 0x53, Ptr5Addr, Ptr5Addr.Length, ref Ptr5AddrRead);
            string QueryCountryCode = Encoding.Default.GetString(Ptr5Addr);


            byte[] CounryCode = Encoding.Default.GetBytes(_state.Instances[InstanceID].CountryCode);
            int bytesWritten = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr4 + 0xA3, CounryCode, CounryCode.Length, ref bytesWritten);
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr2Addr + 0x11, CounryCode, CounryCode.Length, ref bytesWritten);
        }
        public void UpdateServerPassword(AppState _state, int InstanceID, int oldPwLength)
        {

            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            byte[] Ptr2 = new byte[4];
            byte[] Ptr3 = new byte[4];
            int Ptr1Read = 0;
            int Ptr2Read = 0;
            int Ptr3Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, (int)baseAddr + 0x005f2028, Ptr1, Ptr1.Length, ref Ptr1Read);
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, (int)baseAddr + 0x006343A0, Ptr2, Ptr2.Length, ref Ptr2Read);
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, (int)baseAddr + 0x00ACE088, Ptr3, Ptr3.Length, ref Ptr3Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            int Ptr2Addr = BitConverter.ToInt32(Ptr2, 0);
            int Ptr3Addr = BitConverter.ToInt32(Ptr3, 0);
            byte[] ServerPasswordBytes = new byte[16];
            int ServerPasswordRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, Ptr1Addr, ServerPasswordBytes, ServerPasswordBytes.Length, ref ServerPasswordRead);
            string ServerPassword = Encoding.Default.GetString(ServerPasswordBytes).Replace("\0", "");

            int ServerPasswordWritten = 0;
            byte[] ServerPasswordWrite = Encoding.Default.GetBytes(_state.Instances[InstanceID].Password);
            if (ServerPasswordWrite.Length == 0)
            {
                ServerPasswordWrite = new byte[oldPwLength];
            }
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, ServerPasswordWrite, ServerPasswordWrite.Length, ref ServerPasswordWritten);
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr2Addr, ServerPasswordWrite, ServerPasswordWrite.Length, ref ServerPasswordWritten);
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr3Addr, ServerPasswordWrite, ServerPasswordWrite.Length, ref ServerPasswordWritten);

        }
        public void UpdateSessionType(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, (int)baseAddr + 0x000DDC3C, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] SessionTypeBytes = new byte[4];
            int SessionTypeRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, Ptr1Addr, SessionTypeBytes, SessionTypeBytes.Length, ref SessionTypeRead);
            int SessionType = BitConverter.ToInt32(SessionTypeBytes, 0);

            int SessionTypeWritten = 0;
            byte[] SessionTypeBytesWrite = BitConverter.GetBytes(_state.Instances[InstanceID].SessionType);
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, SessionTypeBytesWrite, SessionTypeBytesWrite.Length, ref SessionTypeWritten);
        }
        public void UpdateMaxSlots(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, (int)baseAddr + 0x000D97A0, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] MaxSlotsBytes = new byte[4];
            int MaxSlotsRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, Ptr1Addr, MaxSlotsBytes, MaxSlotsBytes.Length, ref MaxSlotsRead);
            int MaxSlots = BitConverter.ToInt32(MaxSlotsBytes, 0);

            int MaxSlotsWritten = 0;
            byte[] MaxSlotsWrite = BitConverter.GetBytes(_state.Instances[InstanceID].MaxSlots);
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, MaxSlotsWrite, MaxSlotsWrite.Length, ref MaxSlotsWritten);
        }
        public void UpdateTimeLimit(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, (int)baseAddr + 0x000DD1DC, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] TimeLimitBytes = new byte[4];
            int TimeLimitRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, Ptr1Addr, TimeLimitBytes, TimeLimitBytes.Length, ref TimeLimitRead);
            int TimeLimit = BitConverter.ToInt32(TimeLimitBytes, 0);

            int TimeLimitWritten = 0;
            byte[] TimeLimitWrite = BitConverter.GetBytes(_state.Instances[InstanceID].TimeLimit);
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, TimeLimitWrite, TimeLimitWrite.Length, ref TimeLimitWritten);
        }
        public void UpdateStartDelay(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, (int)baseAddr + 0x000D7F00, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] StartDelayBytes = new byte[4];
            int StartDelayRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, Ptr1Addr, StartDelayBytes, StartDelayBytes.Length, ref StartDelayRead);
            int StartDelay = BitConverter.ToInt32(StartDelayBytes, 0);

            int StartDelayWritten = 0;
            byte[] StartDelayWrite = BitConverter.GetBytes(_state.Instances[InstanceID].StartDelay);
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, StartDelayWrite, StartDelayWrite.Length, ref StartDelayWritten);
        }
        public void UpdateLoopMaps(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, (int)baseAddr + 0x000DB6A0, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] LoopMapsBytes = new byte[4];
            int LoopMapsRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, Ptr1Addr, LoopMapsBytes, LoopMapsBytes.Length, ref LoopMapsRead);
            int LoopMaps = BitConverter.ToInt32(LoopMapsBytes, 0);

            int LoopMapsWritten = 0;
            byte[] LoopMapsWrite = BitConverter.GetBytes(_state.Instances[InstanceID].LoopMaps);
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, LoopMapsWrite, LoopMapsWrite.Length, ref LoopMapsWritten);
        }
        public void UpdateRespawnTime(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, (int)baseAddr + 0x000DD4E8, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] RespawnTimeBytes = new byte[4];
            int RespawnTimeRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, Ptr1Addr, RespawnTimeBytes, RespawnTimeBytes.Length, ref RespawnTimeRead);
            int RespawnTime = BitConverter.ToInt32(RespawnTimeBytes, 0);

            int RespawnTimeWritten = 0;
            byte[] RespawnTimeWrite = BitConverter.GetBytes(_state.Instances[InstanceID].RespawnTime);
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, RespawnTimeWrite, RespawnTimeWrite.Length, ref RespawnTimeWritten);
        }
        public void UpdateRequireNovaLogin(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, (int)baseAddr + 0x000D9960, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] RequireNovaBytes = new byte[4];
            int RequireNovaRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, Ptr1Addr, RequireNovaBytes, RequireNovaBytes.Length, ref RequireNovaRead);
            int RequireNova = BitConverter.ToInt32(RequireNovaBytes, 0);

            int RequireNovaWritten = 0;
            byte[] RequireNovaWrite = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[InstanceID].RequireNovaLogin));
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, RequireNovaWrite, RequireNovaWrite.Length, ref RequireNovaWritten);
        }
        public void UpdateAllowCustomSkins(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, (int)baseAddr + 0x000AD760, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] AllowCustomSkinsBytes = new byte[4];
            int AllowCustomSkinsRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, Ptr1Addr, AllowCustomSkinsBytes, AllowCustomSkinsBytes.Length, ref AllowCustomSkinsRead);
            int AllowCustomSkins = BitConverter.ToInt32(AllowCustomSkinsBytes, 0);

            int AllowCustomSkinsWritten = 0;
            byte[] AllowCustomSkinsWrite = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[InstanceID].AllowCustomSkins));
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, AllowCustomSkinsWrite, AllowCustomSkinsWrite.Length, ref AllowCustomSkinsWritten);
        }
        public void UpdateMOTD(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, (int)baseAddr + 0x000D9AAC, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] MOTDBytes = new byte[85];
            int MOTDRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, Ptr1Addr, MOTDBytes, MOTDBytes.Length, ref MOTDRead);
            string MOTD = Encoding.Default.GetString(MOTDBytes).Replace("\0", "");

            int MOTDWritten = 0;
            byte[] MOTDWrite = Encoding.Default.GetBytes(_state.Instances[InstanceID].MOTD);
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, MOTDWrite, MOTDWrite.Length, ref MOTDWritten);
        }
        public void UpdateMinPing(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, (int)baseAddr + 0x000DB628, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] MinPingBytes = new byte[4];
            int MinPingRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, Ptr1Addr, MinPingBytes, MinPingBytes.Length, ref MinPingRead);
            int MinPing = BitConverter.ToInt32(MinPingBytes, 0);

            byte[] MinPingWrite = new byte[4];
            MinPingWrite = BitConverter.GetBytes(_state.Instances[InstanceID].MinPing);

            int MinPingWritten = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, MinPingWrite, MinPingWrite.Length, ref MinPingWritten);
        }
        public void UpdateMinPingValue(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, (int)baseAddr + 0x000D7FB8, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] MinPingValueBytes = new byte[4];
            int MinPingValueRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, Ptr1Addr, MinPingValueBytes, MinPingValueBytes.Length, ref MinPingValueRead);
            int MinPingValue = BitConverter.ToInt32(MinPingValueBytes, 0);

            int MinPingValueWritten = 0;
            byte[] MinPingValueWrite = BitConverter.GetBytes(_state.Instances[InstanceID].MinPingValue);
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, MinPingValueWrite, MinPingValueWrite.Length, ref MinPingValueWritten);
        }
        public void UpdateMaxPing(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, (int)baseAddr + 0x000DB634, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] MaxPingBytes = new byte[4];
            int MaxPingRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, Ptr1Addr + 0x4, MaxPingBytes, MaxPingBytes.Length, ref MaxPingRead);
            int MaxPing = BitConverter.ToInt32(MaxPingBytes, 0);

            byte[] MaxPingWrite = new byte[4];
            MaxPingWrite = BitConverter.GetBytes(_state.Instances[InstanceID].MaxPing);

            int MaxPingWritten = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr + 0x4, MaxPingWrite, MaxPingWrite.Length, ref MaxPingWritten);
        }
        public void UpdateMaxPingValue(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, (int)baseAddr + 0x000DB634, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] MaxPingValueBytes = new byte[4];
            int MaxPingValueRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, Ptr1Addr, MaxPingValueBytes, MaxPingValueBytes.Length, ref MaxPingValueRead);
            int MaxPingValue = BitConverter.ToInt32(MaxPingValueBytes, 0);

            int MaxPingValueWritten = 0;
            byte[] MaxPingValueWrite = BitConverter.GetBytes(_state.Instances[InstanceID].MaxPingValue);
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, MaxPingValueWrite, MaxPingValueWrite.Length, ref MaxPingValueWritten);
        }
        public void UpdateOneShotKills(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, (int)baseAddr + 0x000D8580, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] OneShotKillsBytes = new byte[4];
            int OneShotKillsRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, Ptr1Addr, OneShotKillsBytes, OneShotKillsBytes.Length, ref OneShotKillsRead);
            int OneShotKills = BitConverter.ToInt32(OneShotKillsBytes, 0);

            int OneShotKillsWritten = 0;
            byte[] OneShotKillsWrite = new byte[4];
            OneShotKillsWrite = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[InstanceID].OneShotKills));

            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, OneShotKillsWrite, OneShotKillsWrite.Length, ref OneShotKillsWritten);
        }
        public void UpdateFatBullets(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, (int)baseAddr + 0x000D7F14, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] FatBulletsBytes = new byte[4];
            int FatBulletsRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, Ptr1Addr, FatBulletsBytes, FatBulletsBytes.Length, ref FatBulletsRead);
            int FatBullets = BitConverter.ToInt32(FatBulletsBytes, 0);

            int FatBulletsWritten = 0;
            byte[] FatBulletsWrite = new byte[4];
            FatBulletsWrite = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[InstanceID].FatBullets));

            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, FatBulletsWrite, FatBulletsWrite.Length, ref FatBulletsWritten);
        }
        public void UpdateDestroyBuildings(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, (int)baseAddr + 0x000D99B8, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] DestroyBuildingsBytes = new byte[4];
            int DestroyBuildingsRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, Ptr1Addr, DestroyBuildingsBytes, DestroyBuildingsBytes.Length, ref DestroyBuildingsRead);
            int DestroyBuildings = BitConverter.ToInt32(DestroyBuildingsBytes, 0);

            int DestroyBuildingsWritten = 0;
            byte[] DestroyBuildingsWrite = new byte[4];
            DestroyBuildingsWrite = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[InstanceID].DestroyBuildings));

            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, DestroyBuildingsWrite, DestroyBuildingsWrite.Length, ref DestroyBuildingsWritten);
        }
        public void UpdateBluePassword(AppState _state, int InstanceID, int oldPwLength)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, (int)baseAddr + 0x005F204A, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] BluePasswordBytes = new byte[16];
            int BluePasswordRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, Ptr1Addr, BluePasswordBytes, BluePasswordBytes.Length, ref BluePasswordRead);
            string BluePassword = Encoding.Default.GetString(BluePasswordBytes).Replace("\0", "");

            int BluePasswordWritten = 0;
            byte[] BluePasswordWrite = Encoding.Default.GetBytes(_state.Instances[InstanceID].BluePassword);
            if (BluePasswordWrite.Length == 0)
            {
                BluePasswordWrite = new byte[oldPwLength];
            }
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, BluePasswordWrite, BluePasswordWrite.Length, ref BluePasswordWritten);
        }
        public void UpdateRedPassword(AppState _state, int InstanceID, int oldPwLength)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, (int)baseAddr + 0x006343D3, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] RedPasswordBytes = new byte[16];
            int RedPasswordRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, Ptr1Addr + 0x33, RedPasswordBytes, RedPasswordBytes.Length, ref RedPasswordRead);
            string RedPassword = Encoding.Default.GetString(RedPasswordBytes).Replace("\0", "");

            int RedPasswordWritten = 0;
            byte[] RedPasswordWrite = Encoding.Default.GetBytes(_state.Instances[InstanceID].RedPassword);
            if (RedPasswordWrite.Length == 0)
            {
                RedPasswordWrite = new byte[oldPwLength];
            }
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr + 0x33, RedPasswordWrite, RedPasswordWrite.Length, ref RedPasswordWritten);
        }
        public void UpdatePSPTakeOverTime(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, (int)baseAddr + 0x000DB6FC, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] PSPTakeOverTimeBytes = new byte[4];
            int PSPTakeOverTimeRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, Ptr1Addr + 0x4, PSPTakeOverTimeBytes, PSPTakeOverTimeBytes.Length, ref PSPTakeOverTimeRead);
            int PSPTakeOverTimeValue = BitConverter.ToInt32(PSPTakeOverTimeBytes, 0);

            int PSPTakeOverTimeWritten = 0;
            byte[] PSPTakeOverTimeWrite = BitConverter.GetBytes(_state.Instances[InstanceID].PSPTakeOverTime);
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr + 0x4, PSPTakeOverTimeWrite, PSPTakeOverTimeWrite.Length, ref PSPTakeOverTimeWritten);
        }
        public void UpdateFlagReturnTime(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, (int)baseAddr + 0x000DB6AC, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] FlagReturnTimeBytes = new byte[4];
            int FlagReturnTimeRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, Ptr1Addr, FlagReturnTimeBytes, FlagReturnTimeBytes.Length, ref FlagReturnTimeRead);
            int FlagReturnTime = BitConverter.ToInt32(FlagReturnTimeBytes, 0);

            int FlagReturnTimeWritten = 0;
            byte[] FlagReturnTimeWrite = BitConverter.GetBytes(_state.Instances[InstanceID].FlagReturnTime);
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, FlagReturnTimeWrite, FlagReturnTimeWrite.Length, ref FlagReturnTimeWritten);
        }
        public void UpdateMaxTeamLives(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, (int)baseAddr + 0x000D8554, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] MaxTeamLivesBytes = new byte[4];
            int MaxTeamLivesRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, Ptr1Addr, MaxTeamLivesBytes, MaxTeamLivesBytes.Length, ref MaxTeamLivesRead);
            int MaxTeamLives = BitConverter.ToInt32(MaxTeamLivesBytes, 0);

            int MaxTeamLivesWritten = 0;
            byte[] MaxTeamLivesWrite = BitConverter.GetBytes(_state.Instances[InstanceID].MaxTeamLives);
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, MaxTeamLivesWrite, MaxTeamLivesWrite.Length, ref MaxTeamLivesWritten);
        }
        public void UpdateFriendlyFireKills(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, (int)baseAddr + 0x000DB684, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] FriendlyFireKillsBytes = new byte[4];
            int FriendlyFireKillsRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, Ptr1Addr, FriendlyFireKillsBytes, FriendlyFireKillsBytes.Length, ref FriendlyFireKillsRead);
            int FriendlyFireKills = BitConverter.ToInt32(FriendlyFireKillsBytes, 0);

            int FriendlyFireKillsWritten = 0;
            byte[] FriendlyFireKillsWrite = BitConverter.GetBytes(_state.Instances[InstanceID].FriendlyFireKills);
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, FriendlyFireKillsWrite, FriendlyFireKillsWrite.Length, ref FriendlyFireKillsWritten);
        }
        public void GamePlayOptions(AppState _state, int InstanceID)
        {
            int gameOptions = CalulateGameOptions(_state, InstanceID);
            byte[] gameOptionsBytes = BitConverter.GetBytes(gameOptions);
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, (int)baseAddr + 0x000D7D6C, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] GamePlayOptionsOneBytes = new byte[4];
            int GamePlayOptionsOneRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, Ptr1Addr, GamePlayOptionsOneBytes, GamePlayOptionsOneBytes.Length, ref GamePlayOptionsOneRead);
            int GamePlayOptionsOne = BitConverter.ToInt32(GamePlayOptionsOneBytes, 0);

            int GamePlayOptionsOneWritten = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, gameOptionsBytes, gameOptionsBytes.Length, ref GamePlayOptionsOneWritten);
        }
        public int CalulateGameOptions(AppState _state, int instanceID)
        {
            bool autoBalance = _state.Instances[instanceID].AutoBalance;
            bool friendlyFire = _state.Instances[instanceID].FriendlyFire;
            bool friendlyTags = _state.Instances[instanceID].FriendlyTags;
            bool friendlyFireWarning = _state.Instances[instanceID].FriendlyFireWarning;
            bool showTracers = _state.Instances[instanceID].ShowTracers;
            bool showTeamClays = _state.Instances[instanceID].ShowTeamClays;
            bool allowAutoRange = _state.Instances[instanceID].AllowAutoRange;
            if (autoBalance == true && friendlyFire == false && friendlyTags == false && friendlyFireWarning == false && showTracers == false && showTeamClays == false && allowAutoRange == false)
            {
                return 15883;
            }
            else if (autoBalance == true && friendlyFire == true && friendlyTags == false && friendlyFireWarning == false && showTracers == false && showTeamClays == false && allowAutoRange == false)
            {
                return 15371;
            }
            else if (autoBalance == true && friendlyFire == true && friendlyTags == true && friendlyFireWarning == false && showTracers == false && showTeamClays == false && allowAutoRange == false)
            {
                return 14347;
            }
            else if (autoBalance == true && friendlyFire == true && friendlyTags == true && friendlyFireWarning == true && showTracers == false && showTeamClays == false && allowAutoRange == false)
            {
                return 14339;
            }
            else if (autoBalance == true && friendlyFire == true && friendlyTags == true && friendlyFireWarning == true && showTracers == true && showTeamClays == false && allowAutoRange == false)
            {
                return 14338;
            }
            else if (autoBalance == true && friendlyFire == true && friendlyTags == true && friendlyFireWarning == true && showTracers == true && showTeamClays == true && allowAutoRange == false)
            {
                return 47106;
            }
            else if (autoBalance == true && friendlyFire == true && friendlyTags == true && friendlyFireWarning == true && showTracers == true && showTeamClays == true && allowAutoRange == true)
            {
                return 112642; // all options on
            }
            else if (autoBalance == false && friendlyFire == true && friendlyTags == false && friendlyFireWarning == false && showTracers == false && showTeamClays == false && allowAutoRange == false)
            {
                return 15375;
            }
            else if (autoBalance == false && friendlyFire == true && friendlyTags == true && friendlyFireWarning == false && showTracers == false && showTeamClays == false && allowAutoRange == false)
            {
                return 14351;
            }
            else if (autoBalance == false && friendlyFire == true && friendlyTags == true && friendlyFireWarning == true && showTracers == false && showTeamClays == false && allowAutoRange == false)
            {
                return 14343;
            }
            else if (autoBalance == false && friendlyFire == true && friendlyTags == true && friendlyFireWarning == true && showTracers == true && showTeamClays == false && allowAutoRange == false)
            {
                return 14342;
            }
            else if (autoBalance == false && friendlyFire == true && friendlyTags == true && friendlyFireWarning == true && showTracers == true && showTeamClays == true && allowAutoRange == false)
            {
                return 47110;
            }
            else if (autoBalance == false && friendlyFire == true && friendlyTags == true && friendlyFireWarning == true && showTracers == true && showTeamClays == true && allowAutoRange == true)
            {
                return 112646;
            }
            else if (autoBalance == false && friendlyFire == false && friendlyTags == true && friendlyFireWarning == false && showTracers == false && showTeamClays == false && allowAutoRange == false)
            {
                return 14863;
            }
            else if (autoBalance == false && friendlyFire == false && friendlyTags == true && friendlyFireWarning == true && showTracers == false && showTeamClays == false && allowAutoRange == false)
            {
                return 14855;
            }
            else if (autoBalance == false && friendlyFire == false && friendlyTags == true && friendlyFireWarning == true && showTracers == true && showTeamClays == false && allowAutoRange == false)
            {
                return 14854;
            }
            else if (autoBalance == false && friendlyFire == false && friendlyTags == true && friendlyFireWarning == true && showTracers == true && showTeamClays == true && allowAutoRange == false)
            {
                return 47622;
            }
            else if (autoBalance == false && friendlyFire == false && friendlyTags == true && friendlyFireWarning == true && showTracers == true && showTeamClays == true && allowAutoRange == true)
            {
                return 113158;
            }
            else if (autoBalance == false && friendlyFire == false && friendlyTags == false && friendlyFireWarning == true && showTracers == false && showTeamClays == false && allowAutoRange == false)
            {
                return 15879;
            }
            else if (autoBalance == false && friendlyFire == false && friendlyTags == false && friendlyFireWarning == true && showTracers == true && showTeamClays == false && allowAutoRange == false)
            {
                return 15878;
            }
            else if (autoBalance == false && friendlyFire == false && friendlyTags == false && friendlyFireWarning == true && showTracers == true && showTeamClays == true && allowAutoRange == false)
            {
                return 48646;
            }
            else if (autoBalance == false && friendlyFire == false && friendlyTags == false && friendlyFireWarning == true && showTracers == true && showTeamClays == true && allowAutoRange == true)
            {
                return 114182;
            }
            else if (autoBalance == false && friendlyFire == false && friendlyTags == false && friendlyFireWarning == false && showTracers == true && showTeamClays == false && allowAutoRange == false)
            {
                return 15886;
            }
            else if (autoBalance == false && friendlyFire == false && friendlyTags == false && friendlyFireWarning == false && showTracers == true && showTeamClays == true && allowAutoRange == false)
            {
                return 48654;
            }
            else if (autoBalance == false && friendlyFire == false && friendlyTags == false && friendlyFireWarning == false && showTracers == true && showTeamClays == true && allowAutoRange == true)
            {
                return 114190;
            }
            else if (autoBalance == false && friendlyFire == false && friendlyTags == false && friendlyFireWarning == false && showTracers == false && showTeamClays == true && allowAutoRange == false)
            {
                return 48655;
            }
            else if (autoBalance == false && friendlyFire == false && friendlyTags == false && friendlyFireWarning == false && showTracers == false && showTeamClays == true && allowAutoRange == true)
            {
                return 114191;
            }
            else if (autoBalance == false && friendlyFire == false && friendlyTags == false && friendlyFireWarning == false && showTracers == false && showTeamClays == false && allowAutoRange == true)
            {
                return 81423;
            }
            else if (autoBalance == false && friendlyFire == false && friendlyTags == false && friendlyFireWarning == false && showTracers == false && showTeamClays == false && allowAutoRange == false)
            {
                return 15887; // all off
            }
            else if (autoBalance == true && friendlyFire == false && friendlyTags == true && friendlyFireWarning == false && showTracers == false && showTeamClays == true && allowAutoRange == true)
            {
                return 113163;
            }
            else if (autoBalance == true && friendlyFire == false && friendlyTags == true && friendlyFireWarning == false && showTracers == false && showTeamClays == true && allowAutoRange == false)
            {
                return 47627;
            }
            else if (autoBalance == true && friendlyFire == true && friendlyTags == true && friendlyFireWarning == true && showTracers == false && showTeamClays == true && allowAutoRange == false)
            {
                return 47107;
            }
            else if (autoBalance == false && friendlyFire == false && friendlyTags == true && friendlyFireWarning == false && showTracers == false && showTeamClays == true && allowAutoRange == false)
            {
                return 47631;
            }
            else if (autoBalance == false && friendlyFire == false && friendlyTags == true && friendlyFireWarning == false && showTracers == false && showTeamClays == true && allowAutoRange == true)
            {
                return 113167;
            }
            else if (autoBalance == true && friendlyFire == false && friendlyTags == true && friendlyFireWarning == true && showTracers == false && showTeamClays == true && allowAutoRange == true)
            {
                return 80387;
                // taz check
            }
            else if (autoBalance == true && friendlyFire == true && friendlyTags == true && friendlyFireWarning == false && showTracers == false && showTeamClays == false && allowAutoRange == false)
            {
                return 14339;
            }
            else if (autoBalance == false && friendlyFire == false && friendlyTags == true && friendlyFireWarning == true && showTracers == false && showTeamClays == true && allowAutoRange == true)
            {
                return 113159;
            }
            else if (autoBalance == true && friendlyFire == false && friendlyTags == true && friendlyFireWarning == true && showTracers == false && showTeamClays == true && allowAutoRange == true)
            {
                return 113155;
            }
            else if (autoBalance == false && friendlyFire == true && friendlyTags == true && friendlyFireWarning == true && showTracers == true && showTeamClays == true && allowAutoRange == true)
            {
                return 112647;
            }
            else if (autoBalance == false && friendlyFire == true && friendlyTags == true && friendlyFireWarning == true && showTracers == true && showTeamClays == true && allowAutoRange == true)
            {
                return 112643;
            }
            else if (autoBalance == false && friendlyFire == false && friendlyTags == true && friendlyFireWarning == true && showTracers == true && showTeamClays == true && allowAutoRange == true)
            {
                return 113158;
            }
            else if (autoBalance == true && friendlyFire == false && friendlyTags == true && friendlyFireWarning == true && showTracers == true && showTeamClays == true && allowAutoRange == true)
            {
                return 113154;
            }
            else if (autoBalance == false && friendlyFire == true && friendlyTags == true && friendlyFireWarning == true && showTracers == true && showTeamClays == false && allowAutoRange == true)
            {
                return 79878;
            }
            else if (autoBalance == true && friendlyFire == true && friendlyTags == true && friendlyFireWarning == true && showTracers == true && showTeamClays == false && allowAutoRange == true)
            {
                return 79874;
            }
            else if (autoBalance == false && friendlyFire == false && friendlyTags == true && friendlyFireWarning == false && showTracers == true && showTeamClays == true && allowAutoRange == true)
            {
                return 113166;
            }
            else if (autoBalance == true && friendlyFire == false && friendlyTags == true && friendlyFireWarning == false && showTracers == true && showTeamClays == true && allowAutoRange == true)
            {
                return 113162;
            }
            else if (autoBalance == false && friendlyFire == true && friendlyTags == false && friendlyFireWarning == false && showTracers == false && showTeamClays == false && allowAutoRange == false)
            {
                return 15375;
            }
            else if (autoBalance == false && friendlyFire == false && friendlyTags == true && friendlyFireWarning == false && showTracers == false && showTeamClays == false && allowAutoRange == false)
            {
                return 14863;
            }
            else if (autoBalance == false && friendlyFire == false && friendlyTags == false && friendlyFireWarning == true && showTracers == false && showTeamClays == false && allowAutoRange == false)
            {
                return 15879;
            }
            else if (autoBalance == false && friendlyFire == false && friendlyTags == false && friendlyFireWarning == false && showTracers == true && showTeamClays == false && allowAutoRange == false)
            {
                return 15886;
            }
            else if (autoBalance == false && friendlyFire == false && friendlyTags == false && friendlyFireWarning == false && showTracers == false && showTeamClays == true && allowAutoRange == false)
            {
                return 48655;
            }
            else if (autoBalance == false && friendlyFire == false && friendlyTags == false && friendlyFireWarning == false && showTracers == false && showTeamClays == false && allowAutoRange == true)
            {
                return 81423;
            }
            else if (autoBalance == false && friendlyFire == false && friendlyTags == true && friendlyFireWarning == false && showTracers == false && showTeamClays == false && allowAutoRange == true)
            {
                return 80399;
            }
            else
            {
                throw new Exception("Something went VERY VERY WRONG. #91");
            }
        }
        public void UpdateWeaponRestrictions(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1Bytes = new byte[4];
            int Ptr1Read = 0;
            int Ptr1Location = baseAddr + 0x0015C4B0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, Ptr1Location, Ptr1Bytes, Ptr1Bytes.Length, ref Ptr1Read);

            int Ptr1 = BitConverter.ToInt32(Ptr1Bytes, 0);
            int WeaponEntry = Ptr1 + 0x268;

            byte[] WPN_COLT45Bytes = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[InstanceID].WeaponRestrictions.WPN_COLT45));
            int WPN_COLT45Written = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], WeaponEntry, WPN_COLT45Bytes, WPN_COLT45Bytes.Length, ref WPN_COLT45Written);

            int WeaponEntry_WPN_M9BERETTA = WeaponEntry + 0x4;
            byte[] WPN_M9BERETTABytes = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[InstanceID].WeaponRestrictions.WPN_M9BERETTA));
            int WPN_M9BERETTAWritten = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], WeaponEntry_WPN_M9BERETTA, WPN_M9BERETTABytes, WPN_M9BERETTABytes.Length, ref WPN_M9BERETTAWritten);

            int WeaponEntry_WPN_REMMINGTONSG = WeaponEntry + 0x8;
            byte[] WPN_REMMINGTONSGBytes = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[InstanceID].WeaponRestrictions.WPN_REMMINGTONSG));
            int WPN_REMMINGTONSGWritten = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], WeaponEntry_WPN_REMMINGTONSG, WPN_REMMINGTONSGBytes, WPN_REMMINGTONSGBytes.Length, ref WPN_REMMINGTONSGWritten);

            int WeaponEntry_WPN_CAR15 = WeaponEntry + 0xC;
            byte[] WPN_CAR15Bytes = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[InstanceID].WeaponRestrictions.WPN_CAR15));
            int WPN_CAR15Written = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], WeaponEntry_WPN_CAR15, WPN_CAR15Bytes, WPN_CAR15Bytes.Length, ref WPN_CAR15Written);

            int WeaponEntry_WPN_CAR15_203 = WeaponEntry + 0x14;
            byte[] WPN_CAR15_203Bytes = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[InstanceID].WeaponRestrictions.WPN_CAR15_203));
            int WPN_CAR15_203Written = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], WeaponEntry_WPN_CAR15_203, WPN_CAR15_203Bytes, WPN_CAR15_203Bytes.Length, ref WPN_CAR15_203Written);

            int WeaponEntry_WPN_M16_BURST = WeaponEntry + 0x20;
            byte[] WPN_M16_BURSTBytes = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[InstanceID].WeaponRestrictions.WPN_M16_BURST));
            int WPN_M16_BURSTWritten = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], WeaponEntry_WPN_M16_BURST, WPN_M16_BURSTBytes, WPN_M16_BURSTBytes.Length, ref WPN_M16_BURSTWritten);

            int WeaponEntry_WPN_M16_BURST_203 = WeaponEntry + 0x28;
            byte[] WPN_M16_BURST_203Bytes = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[InstanceID].WeaponRestrictions.WPN_M16_BURST_203));
            int WPN_M16_BURST_203Written = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], WeaponEntry_WPN_M16_BURST_203, WPN_M16_BURST_203Bytes, WPN_M16_BURST_203Bytes.Length, ref WPN_M16_BURST_203Written);

            int WeaponEntry_WPN_M21 = WeaponEntry + 0x34;
            byte[] WPN_M21Bytes = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[InstanceID].WeaponRestrictions.WPN_M21));
            int WPN_M21Written = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], WeaponEntry_WPN_M21, WPN_M21Bytes, WPN_M21Bytes.Length, ref WPN_M21Written);

            int WeaponEntry_WPN_M24 = WeaponEntry + 0x38;
            byte[] WPN_M24Bytes = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[InstanceID].WeaponRestrictions.WPN_M24));
            int WPN_M24Written = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], WeaponEntry_WPN_M24, WPN_M24Bytes, WPN_M24Bytes.Length, ref WPN_M24Written);

            int WeaponEntry_WPN_MCRT_300_TACTICAL = WeaponEntry + 0x3C;
            byte[] WPN_MCRT_300_TACTICALBytes = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[InstanceID].WeaponRestrictions.WPN_MCRT_300_TACTICAL));
            int WPN_MCRT_300_TACTICALWritten = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], WeaponEntry_WPN_MCRT_300_TACTICAL, WPN_MCRT_300_TACTICALBytes, WPN_MCRT_300_TACTICALBytes.Length, ref WPN_MCRT_300_TACTICALWritten);

            int WeaponEntry_WPN_BARRETT = WeaponEntry + 0x40;
            byte[] WPN_BARRETTBytes = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[InstanceID].WeaponRestrictions.WPN_BARRETT));
            int WPN_BARRETTWritten = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], WeaponEntry_WPN_BARRETT, WPN_BARRETTBytes, WPN_BARRETTBytes.Length, ref WPN_BARRETTWritten);

            int WeaponEntry_WPN_SAW = WeaponEntry + 0x44;
            byte[] WPN_SAWBytes = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[InstanceID].WeaponRestrictions.WPN_SAW));
            int WPN_SAWWritten = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], WeaponEntry_WPN_SAW, WPN_SAWBytes, WPN_SAWBytes.Length, ref WPN_SAWWritten);

            int WeaponEntry_WPN_M60 = WeaponEntry + 0x48;
            byte[] WPN_M60Bytes = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[InstanceID].WeaponRestrictions.WPN_M60));
            int WPN_M60Written = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], WeaponEntry_WPN_M60, WPN_M60Bytes, WPN_M60Bytes.Length, ref WPN_M60Written);

            int WeaponEntry_WPN_M240 = WeaponEntry + 0x4C;
            byte[] WPN_M240Bytes = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[InstanceID].WeaponRestrictions.WPN_M240));
            int WPN_M240Written = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], WeaponEntry_WPN_M240, WPN_M240Bytes, WPN_M240Bytes.Length, ref WPN_M240Written);

            int WeaponEntry_WPN_MP5 = WeaponEntry + 0x50;
            byte[] WPN_MP5Bytes = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[InstanceID].WeaponRestrictions.WPN_MP5));
            int WPN_MP5Written = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], WeaponEntry_WPN_MP5, WPN_MP5Bytes, WPN_MP5Bytes.Length, ref WPN_MP5Written);

            int WeaponEntry_WPN_G3 = WeaponEntry + 0x54;
            byte[] WPN_G3Bytes = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[InstanceID].WeaponRestrictions.WPN_G3));
            int WPN_G3Written = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], WeaponEntry_WPN_G3, WPN_G3Bytes, WPN_G3Bytes.Length, ref WPN_G3Written);

            int WeaponEntry_WPN_G36 = WeaponEntry + 0x5C;
            byte[] WPN_G36Bytes = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[InstanceID].WeaponRestrictions.WPN_G36));
            int WPN_G36Written = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], WeaponEntry_WPN_G36, WPN_G36Bytes, WPN_G36Bytes.Length, ref WPN_G36Written);

            int WeaponEntry_WPN_PSG1 = WeaponEntry + 0x64;
            byte[] WPN_PSG1Bytes = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[InstanceID].WeaponRestrictions.WPN_PSG1));
            int WPN_PSG1Written = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], WeaponEntry_WPN_PSG1, WPN_PSG1Bytes, WPN_PSG1Bytes.Length, ref WPN_PSG1Written);

            int WeaponEntry_WPN_XM84_STUN = WeaponEntry + 0x68;
            byte[] WPN_XM84_STUNBytes = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[InstanceID].WeaponRestrictions.WPN_XM84_STUN));
            int WPN_XM84_STUNWritten = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], WeaponEntry_WPN_XM84_STUN, WPN_XM84_STUNBytes, WPN_XM84_STUNBytes.Length, ref WPN_XM84_STUNWritten);

            int WeaponEntry_WPN_M67_FRAG = WeaponEntry + 0x6C;
            byte[] WPN_M67_FRAGBytes = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[InstanceID].WeaponRestrictions.WPN_M67_FRAG));
            int WPN_M67_FRAGWritten = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], WeaponEntry_WPN_M67_FRAG, WPN_M67_FRAGBytes, WPN_M67_FRAGBytes.Length, ref WPN_M67_FRAGWritten);

            int WeaponEntry_WPN_AN_M8_SMOKE = WeaponEntry + 0x70;
            byte[] WPN_AN_M8_SMOKEBytes = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[InstanceID].WeaponRestrictions.WPN_AN_M8_SMOKE));
            int WPN_AN_M8_SMOKEWritten = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], WeaponEntry_WPN_AN_M8_SMOKE, WPN_AN_M8_SMOKEBytes, WPN_AN_M8_SMOKEBytes.Length, ref WPN_AN_M8_SMOKEWritten);

            int WeaponEntry_WPN_SATCHEL_CHARGE = WeaponEntry + 0x78;
            byte[] WPN_SATCHEL_CHARGEBytes = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[InstanceID].WeaponRestrictions.WPN_SATCHEL_CHARGE));
            int WPN_SATCHEL_CHARGEWritten = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], WeaponEntry_WPN_SATCHEL_CHARGE, WPN_SATCHEL_CHARGEBytes, WPN_SATCHEL_CHARGEBytes.Length, ref WPN_SATCHEL_CHARGEWritten);

            int WeaponEntry_WPN_CLAYMORE = WeaponEntry + 0x80;
            byte[] WPN_CLAYMOREBytes = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[InstanceID].WeaponRestrictions.WPN_CLAYMORE));
            int WPN_CLAYMOREWritten = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], WeaponEntry_WPN_CLAYMORE, WPN_CLAYMOREBytes, WPN_CLAYMOREBytes.Length, ref WPN_CLAYMOREWritten);

            int WeaponEntry_WPN_AT4 = WeaponEntry + 0x84;
            byte[] WPN_AT4Bytes = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[InstanceID].WeaponRestrictions.WPN_AT4));
            int WPN_AT4Written = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], WeaponEntry_WPN_AT4, WPN_AT4Bytes, WPN_AT4Bytes.Length, ref WPN_AT4Written);
        }
        public void UpdateMapCycle(AppState _state, int InstanceID)
        {
            if (_state.Instances[InstanceID].previousMapList.Count > 128)
            {
                throw new Exception("Someway, somehow, someone bypassed the maplist checks. You are NOT allowed to have more than 128 maps. #88");
            }
            int appEntry = 0x400000;

            byte[] ServerMapCyclePtr = new byte[4];
            int Pointer2Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, appEntry + 0x005ED5F8, ServerMapCyclePtr, ServerMapCyclePtr.Length, ref Pointer2Read);
            int mapCycleServerAddress = BitConverter.ToInt32(ServerMapCyclePtr, 0);

            int mapCycleTotalAddress = mapCycleServerAddress + 0x4;
            byte[] mapTotal = BitConverter.GetBytes(_state.Instances[InstanceID].MapList.Count);
            int mapTotalWritten = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], mapCycleTotalAddress, mapTotal, mapTotal.Length, ref mapTotalWritten);


            int mapCycleCurrentIndex = mapCycleServerAddress + 0xC;
            byte[] resetMapIndex = BitConverter.GetBytes(_state.Instances[InstanceID].MapList.Count);
            int resetMapIndexWritten = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], mapCycleCurrentIndex, resetMapIndex, resetMapIndex.Length, ref resetMapIndexWritten);


            byte[] mapCycleListAddress = new byte[4];
            int mapCycleListAddressRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, mapCycleServerAddress, mapCycleListAddress, mapCycleListAddress.Length, ref mapCycleListAddressRead);
            int mapCycleList = BitConverter.ToInt32(mapCycleListAddress, 0);

            foreach (var entry in _state.Instances[InstanceID].previousMapList)
            {
                int mapFileIndexLocation = mapCycleList;
                string mapFileName = "";
                foreach (var map in entry.Value.MapFile)
                {
                    mapFileName += "00 ";
                }
                byte[] mapFileBytes = HexConverter.ToByteArray(mapFileName.Replace(" ", ""));
                int mapFileBytesWritten = 0;
                MemoryProcessor.Write(_state.Instances[InstanceID], mapFileIndexLocation, mapFileBytes, mapFileBytes.Length, ref mapFileBytesWritten);
                mapFileIndexLocation += 0x23;

                byte[] customMapFlag = BitConverter.GetBytes(Convert.ToInt32(false));
                int customMapFlagWritten = 0;
                MemoryProcessor.Write(_state.Instances[InstanceID], mapFileIndexLocation, customMapFlag, customMapFlag.Length, ref customMapFlagWritten);
                mapCycleList += 0x24;
            }
        }
        public void UpdateMapCycle2(AppState _state, int InstanceID)
        {
            if (_state.Instances[InstanceID].MapList.Count > 128)
            {
                throw new Exception("Someway, somehow, someone bypassed the maplist checks. You are NOT allowed to have more than 128 maps. #89");
            }
            int appEntry = 0x400000;

            byte[] Pointer1Bytes = new byte[4];
            int Pointer1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, appEntry + 0x000DC6D8, Pointer1Bytes, Pointer1Bytes.Length, ref Pointer1Read);
            int mapCycleClientAddress = BitConverter.ToInt32(Pointer1Bytes, 0);

            string mapCycleClientString = "";

            mapCycleClientString += _state.autoRes.StringToHex(_state.Instances[InstanceID].MapList[0].MapFile, 28) + " 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ";
            if (_state.Instances[InstanceID].MapList[0].MapName.Length > 31)
            {
                string ModifiedmapTitle = "";
                for (int i = 0; i < 31; i++)
                {
                    string mapTitle = _state.Instances[InstanceID].MapList[0].MapName;
                    ModifiedmapTitle += mapTitle[i];
                }
                mapCycleClientString += _state.autoRes.StringToHex(ModifiedmapTitle, 28) + " 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 20 B5 B6 01 00 00 00 00 53 00 00 00 00 ";
            }
            else
            {
                mapCycleClientString += _state.autoRes.StringToHex(_state.Instances[InstanceID].MapList[0].MapName, 28) + " 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 20 B5 B6 01 00 00 00 00 53 00 00 00 00 ";
            }
            mapCycleClientString += _state.autoRes.IntToHex(_state.Instances[InstanceID].MaxKills) + " 14 00 00 00 05 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ";
            mapCycleClientString += _state.autoRes.BoolToHex(_state.Instances[InstanceID].MapList[0].CustomMap) + " 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 "; // custom map flag for FIRST MAP IN ROTATION

            for (int Index = 0; Index < _state.Instances[InstanceID].MapList.Count; Index++)
            {
                if (Index != 0)
                {
                    mapCycleClientString += _state.autoRes.StringToHex(_state.Instances[InstanceID].MapList[Index].MapFile, 28) + " 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ";
                    if (_state.Instances[InstanceID].MapList[Index].MapName.Length > 31)
                    {
                        string ModifiedmapTitleLoop = "";
                        for (int iMap = 0; iMap < 31; iMap++)
                        {
                            ModifiedmapTitleLoop += _state.Instances[InstanceID].MapList[Index].MapName[iMap];
                        }
                        mapCycleClientString += _state.autoRes.StringToHex(ModifiedmapTitleLoop, 28) + " 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 20 B5 B6 01 00 00 00 00 53 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ";
                    }
                    else
                    {
                        mapCycleClientString += _state.autoRes.StringToHex(_state.Instances[InstanceID].MapList[Index].MapName, 28) + " 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 20 B5 B6 01 00 00 00 00 53 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ";
                    }
                    mapCycleClientString += _state.autoRes.BoolToHex(_state.Instances[InstanceID].MapList[Index].CustomMap) + " 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ";
                }
            }

            byte[] mapCycleClientBytes = HexConverter.ToByteArray(mapCycleClientString.Replace(" ", ""));
            int mapCycleClientWritten = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], mapCycleClientAddress, mapCycleClientBytes, mapCycleClientBytes.Length, ref mapCycleClientWritten);
            UpdateSecondaryMapList(_state, _state.Instances[InstanceID].ProcessHandle, InstanceID);
        }
        public void UpdateSecondaryMapList(AppState _state, IntPtr handle, int InstanceID)
        {
            int appEntry = 0x400000;
            byte[] ServerMapCyclePtr = new byte[4];
            int Pointer2Read = 0;
            ReadProcessMemory((int)handle, appEntry + 0x005ED5F8, ServerMapCyclePtr, ServerMapCyclePtr.Length, ref Pointer2Read);
            int mapCycleServerAddress = BitConverter.ToInt32(ServerMapCyclePtr, 0);

            int mapCycleTotalAddress = mapCycleServerAddress + 0x4;
            byte[] mapTotal = BitConverter.GetBytes(_state.Instances[InstanceID].MapList.Count);
            int mapTotalWritten = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], mapCycleTotalAddress, mapTotal, mapTotal.Length, ref mapTotalWritten);


            int mapCycleCurrentIndex = mapCycleServerAddress + 0xC;
            byte[] resetMapIndex = BitConverter.GetBytes(_state.Instances[InstanceID].MapList.Count);
            int resetMapIndexWritten = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], mapCycleCurrentIndex, resetMapIndex, resetMapIndex.Length, ref resetMapIndexWritten);


            byte[] mapCycleListAddress = new byte[4];
            int mapCycleListAddressRead = 0;
            ReadProcessMemory((int)handle, mapCycleServerAddress, mapCycleListAddress, mapCycleListAddress.Length, ref mapCycleListAddressRead);
            int mapCycleList = BitConverter.ToInt32(mapCycleListAddress, 0);


            for (int i = 0; i < _state.Instances[InstanceID].MapList.Count; i++)
            {
                int mapFileIndexLocation = mapCycleList;
                byte[] mapFileBytes = Encoding.ASCII.GetBytes(_state.Instances[InstanceID].MapList[i].MapFile);
                int mapFileBytesWritten = 0;
                MemoryProcessor.Write(_state.Instances[InstanceID], mapFileIndexLocation, mapFileBytes, mapFileBytes.Length, ref mapFileBytesWritten);
                mapFileIndexLocation += 0x20;

                byte[] customMapFlag = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[InstanceID].MapList[i].CustomMap));
                int customMapFlagWritten = 0;
                MemoryProcessor.Write(_state.Instances[InstanceID], mapFileIndexLocation, customMapFlag, customMapFlag.Length, ref customMapFlagWritten);
                mapCycleList += 0x24;
            }

            /*if (_state.Instances[InstanceID].MapList.Count != _state.Instances[InstanceID].mapListCount)
            {
                for (int i = 0; i < _state.Instances[InstanceID].mapListCount; i++)
                {
                    if (_state.Instances[InstanceID].MapList.ContainsKey(i))
                    {
                        byte[] mapFileBytes = Encoding.ASCII.GetBytes(_state.Instances[InstanceID].MapList[i].MapFile);
                        int mapFileBytesWritten = 0;
                        WriteProcessMemory((int)handle, mapCycleList, mapFileBytes, mapFileBytes.Length, ref mapFileBytesWritten);


                    }

                }
            }
            else if (_state.Instances[InstanceID].MapList.Count == _state.Instances[InstanceID].mapListCount)
            {
                for (int i = 0; i < _state.Instances[InstanceID].MapList.Count; i++)
                {

                }
            }*/
        }
        public void UpdateNextMap(AppState _state, int InstanceID, int NextMapIndex)
        {
            int appEntry = 0x400000;
            byte[] ServerMapCyclePtr = new byte[4];
            int Pointer2Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].ProcessHandle, appEntry + 0x005ED5F8, ServerMapCyclePtr, ServerMapCyclePtr.Length, ref Pointer2Read);
            int MapCycleIndex = BitConverter.ToInt32(ServerMapCyclePtr, 0) + 0xC;
            int NewMapIndex = NextMapIndex;
            if (NewMapIndex - 1 == -1)
            {
                NewMapIndex = _state.Instances[InstanceID].MapList.Count;
            }
            else if (_state.Instances[InstanceID].MapList.ContainsKey(NewMapIndex - 1))
            {
                NewMapIndex--;
            }
            byte[] newMapIndexBytes = BitConverter.GetBytes(NewMapIndex);
            int newMapIndexBytesWritten = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], MapCycleIndex, newMapIndexBytes, newMapIndexBytes.Length, ref newMapIndexBytesWritten);
        }
        public void SetHostnames(Instance _instance)
        {
            bool processExists = ProcessExist(_instance.PID.GetValueOrDefault());
            if (processExists == true)
            {
                int buffer = 0;
                byte[] PointerAddr = new byte[4];
                var baseAddr = 0x400000;
                var Pointer = baseAddr + 0x005ED600;
                ReadProcessMemory((int)_instance.ProcessHandle, (int)Pointer, PointerAddr, PointerAddr.Length, ref buffer);
                int buffer2 = 0;
                byte[] Hostname = Encoding.Default.GetBytes(_instance.HostName + "\0");
                var Address2HostName = BitConverter.ToInt32(PointerAddr, 0);
                MemoryProcessor.Write(_instance, (int)Address2HostName + 0x3C, Hostname, Hostname.Length, ref buffer2);
            }
        }

    }
}
