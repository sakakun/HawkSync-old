using HawkSync_SM.classes.ChatManagement;
using HawkSync_SM.classes.StatManagement;
using Newtonsoft.Json;
using Salaros.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using static System.Windows.Forms.AxHost;

namespace HawkSync_SM
{
    public class ServerManagement
    {
        // Logging
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // Import of Dynamic Link Libraries
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);
        [DllImport("user32.dll")]
        static extern int SetWindowText(IntPtr hWnd, string text);
        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);

        const int PROCESS_WM_READ = 0x0010;
        const int PROCESS_VM_WRITE = 0x0020;
        const int PROCESS_VM_OPERATION = 0x0008;
        const int PROCESS_ALL_ACCESS = 0x1F0FFF;
        const int PROCESS_QUERY_INFORMATION = 0x0400;
        const uint WM_KEYDOWN = 0x0100;
        const uint WM_KEYUP = 0x0101;
        const int VK_ENTER = 0x0D;
        const int cmdConsole = 0xC0;
        const int chatConsole = 0x54;

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
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)baseAddr + 0x001BF400, Ptr1, Ptr1.Length, ref Ptr1Read);
            int Ptr2 = BitConverter.ToInt32(Ptr1, 0);

            byte[] ServerName = new byte[31];
            int ServerNamePtrRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)Ptr2 + 0x4, ServerName, ServerName.Length, ref ServerNamePtrRead);
            string ServerNameQuery = Encoding.Default.GetString(ServerName).Replace("\0", "");
            // end Server Query Name

            // Server Name Display
            byte[] Ptr3 = new byte[4];
            int Ptr3Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)Ptr2 + 0x000A7088, Ptr3, Ptr3.Length, ref Ptr3Read);
            int ServerDisplayerName = BitConverter.ToInt32(Ptr3, 0);

            byte[] ServerNameDisplay = new byte[31];
            int ServerNameRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)ServerDisplayerName + 0x30, ServerNameDisplay, ServerNameDisplay.Length, ref ServerNameRead);
            string ServerDisplayName = Encoding.Default.GetString(ServerNameDisplay).Replace("\0", "");
            // end Server Name Display

            // since either one or the other isn't what it should be.. just update them both. Call it a day.
            byte[] ServerNameBytes = Encoding.Default.GetBytes(_state.Instances[InstanceID].gameServerName);
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
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)baseAddr + 0x000AC7C8, Ptr1, Ptr1.Length, ref Ptr1Read);
            int Ptr2Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] Ptr2 = new byte[4];
            int Ptr2Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)Ptr2Addr + 0x11, Ptr2, Ptr2.Length, ref Ptr2Read);
            string NovaCountryCode = Encoding.Default.GetString(Ptr2);


            byte[] Ptr3 = new byte[4];
            int Ptr3Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)baseAddr + 0x000A7088, Ptr3, Ptr3.Length, ref Ptr3Read);
            int Ptr4 = BitConverter.ToInt32(Ptr3, 0);
            byte[] Ptr5Addr = new byte[4];
            int Ptr5AddrRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)Ptr4 + 0x50 + 0x53, Ptr5Addr, Ptr5Addr.Length, ref Ptr5AddrRead);
            string QueryCountryCode = Encoding.Default.GetString(Ptr5Addr);


            byte[] CounryCode = Encoding.Default.GetBytes(_state.Instances[InstanceID].gameCountryCode);
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
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)baseAddr + 0x005f2028, Ptr1, Ptr1.Length, ref Ptr1Read);
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)baseAddr + 0x006343A0, Ptr2, Ptr2.Length, ref Ptr2Read);
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)baseAddr + 0x00ACE088, Ptr3, Ptr3.Length, ref Ptr3Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            int Ptr2Addr = BitConverter.ToInt32(Ptr2, 0);
            int Ptr3Addr = BitConverter.ToInt32(Ptr3, 0);
            byte[] ServerPasswordBytes = new byte[16];
            int ServerPasswordRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, Ptr1Addr, ServerPasswordBytes, ServerPasswordBytes.Length, ref ServerPasswordRead);
            string ServerPassword = Encoding.Default.GetString(ServerPasswordBytes).Replace("\0", "");

            int ServerPasswordWritten = 0;
            byte[] ServerPasswordWrite = Encoding.Default.GetBytes(_state.Instances[InstanceID].gamePasswordLobby);
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
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)baseAddr + 0x000DDC3C, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] SessionTypeBytes = new byte[4];
            int SessionTypeRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, Ptr1Addr, SessionTypeBytes, SessionTypeBytes.Length, ref SessionTypeRead);
            int SessionType = BitConverter.ToInt32(SessionTypeBytes, 0);

            int SessionTypeWritten = 0;
            byte[] SessionTypeBytesWrite = BitConverter.GetBytes(_state.Instances[InstanceID].gameSessionType);
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, SessionTypeBytesWrite, SessionTypeBytesWrite.Length, ref SessionTypeWritten);
        }
        public void UpdateMaxSlots(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)baseAddr + 0x000D97A0, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] MaxSlotsBytes = new byte[4];
            int MaxSlotsRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, Ptr1Addr, MaxSlotsBytes, MaxSlotsBytes.Length, ref MaxSlotsRead);
            int MaxSlots = BitConverter.ToInt32(MaxSlotsBytes, 0);

            int MaxSlotsWritten = 0;
            byte[] MaxSlotsWrite = BitConverter.GetBytes(_state.Instances[InstanceID].gameMaxSlots);
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, MaxSlotsWrite, MaxSlotsWrite.Length, ref MaxSlotsWritten);
        }
        public void UpdateTimeLimit(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)baseAddr + 0x000DD1DC, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] TimeLimitBytes = new byte[4];
            int TimeLimitRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, Ptr1Addr, TimeLimitBytes, TimeLimitBytes.Length, ref TimeLimitRead);
            int TimeLimit = BitConverter.ToInt32(TimeLimitBytes, 0);

            int TimeLimitWritten = 0;
            byte[] TimeLimitWrite = BitConverter.GetBytes(_state.Instances[InstanceID].gameTimeLimit);
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, TimeLimitWrite, TimeLimitWrite.Length, ref TimeLimitWritten);
        }
        public void UpdateStartDelay(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)baseAddr + 0x000D7F00, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] StartDelayBytes = new byte[4];
            int StartDelayRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, Ptr1Addr, StartDelayBytes, StartDelayBytes.Length, ref StartDelayRead);
            int StartDelay = BitConverter.ToInt32(StartDelayBytes, 0);

            int StartDelayWritten = 0;
            byte[] StartDelayWrite = BitConverter.GetBytes(_state.Instances[InstanceID].gameStartDelay);
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, StartDelayWrite, StartDelayWrite.Length, ref StartDelayWritten);
        }
        public void UpdateLoopMaps(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)baseAddr + 0x000DB6A0, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] LoopMapsBytes = new byte[4];
            int LoopMapsRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, Ptr1Addr, LoopMapsBytes, LoopMapsBytes.Length, ref LoopMapsRead);
            int LoopMaps = BitConverter.ToInt32(LoopMapsBytes, 0);

            int LoopMapsWritten = 0;
            byte[] LoopMapsWrite = BitConverter.GetBytes(_state.Instances[InstanceID].gameLoopMaps);
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, LoopMapsWrite, LoopMapsWrite.Length, ref LoopMapsWritten);
        }
        public void UpdateRespawnTime(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)baseAddr + 0x000DD4E8, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] RespawnTimeBytes = new byte[4];
            int RespawnTimeRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, Ptr1Addr, RespawnTimeBytes, RespawnTimeBytes.Length, ref RespawnTimeRead);
            int RespawnTime = BitConverter.ToInt32(RespawnTimeBytes, 0);

            int RespawnTimeWritten = 0;
            byte[] RespawnTimeWrite = BitConverter.GetBytes(_state.Instances[InstanceID].gameRespawnTime);
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, RespawnTimeWrite, RespawnTimeWrite.Length, ref RespawnTimeWritten);
        }
        public void UpdateRequireNovaLogin(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)baseAddr + 0x000D9960, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] RequireNovaBytes = new byte[4];
            int RequireNovaRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, Ptr1Addr, RequireNovaBytes, RequireNovaBytes.Length, ref RequireNovaRead);
            int RequireNova = BitConverter.ToInt32(RequireNovaBytes, 0);

            int RequireNovaWritten = 0;
            byte[] RequireNovaWrite = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[InstanceID].gameRequireNova));
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, RequireNovaWrite, RequireNovaWrite.Length, ref RequireNovaWritten);
        }
        public void UpdateAllowCustomSkins(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)baseAddr + 0x000AD760, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] AllowCustomSkinsBytes = new byte[4];
            int AllowCustomSkinsRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, Ptr1Addr, AllowCustomSkinsBytes, AllowCustomSkinsBytes.Length, ref AllowCustomSkinsRead);
            int AllowCustomSkins = BitConverter.ToInt32(AllowCustomSkinsBytes, 0);

            int AllowCustomSkinsWritten = 0;
            byte[] AllowCustomSkinsWrite = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[InstanceID].gameCustomSkins));
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, AllowCustomSkinsWrite, AllowCustomSkinsWrite.Length, ref AllowCustomSkinsWritten);
        }
        public void UpdateMOTD(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)baseAddr + 0x000D9AAC, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] MOTDBytes = new byte[85];
            int MOTDRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, Ptr1Addr, MOTDBytes, MOTDBytes.Length, ref MOTDRead);
            string MOTD = Encoding.Default.GetString(MOTDBytes).Replace("\0", "");

            int MOTDWritten = 0;
            byte[] MOTDWrite = Encoding.Default.GetBytes(_state.Instances[InstanceID].gameMOTD);
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, MOTDWrite, MOTDWrite.Length, ref MOTDWritten);
        }
        public void UpdateMinPing(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)baseAddr + 0x000DB628, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] MinPingBytes = new byte[4];
            int MinPingRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, Ptr1Addr, MinPingBytes, MinPingBytes.Length, ref MinPingRead);
            int MinPing = BitConverter.ToInt32(MinPingBytes, 0);

            byte[] MinPingWrite = new byte[4];
            MinPingWrite = BitConverter.GetBytes(_state.Instances[InstanceID].gameMinPing);

            int MinPingWritten = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, MinPingWrite, MinPingWrite.Length, ref MinPingWritten);
        }
        public void UpdateMinPingValue(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)baseAddr + 0x000D7FB8, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] MinPingValueBytes = new byte[4];
            int MinPingValueRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, Ptr1Addr, MinPingValueBytes, MinPingValueBytes.Length, ref MinPingValueRead);
            int MinPingValue = BitConverter.ToInt32(MinPingValueBytes, 0);

            int MinPingValueWritten = 0;
            byte[] MinPingValueWrite = BitConverter.GetBytes(_state.Instances[InstanceID].gameMinPingValue);
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, MinPingValueWrite, MinPingValueWrite.Length, ref MinPingValueWritten);
        }
        public void UpdateMaxPing(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)baseAddr + 0x000DB634, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] MaxPingBytes = new byte[4];
            int MaxPingRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, Ptr1Addr + 0x4, MaxPingBytes, MaxPingBytes.Length, ref MaxPingRead);
            int MaxPing = BitConverter.ToInt32(MaxPingBytes, 0);

            byte[] MaxPingWrite = new byte[4];
            MaxPingWrite = BitConverter.GetBytes(_state.Instances[InstanceID].gameMaxPing);

            int MaxPingWritten = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr + 0x4, MaxPingWrite, MaxPingWrite.Length, ref MaxPingWritten);
        }
        public void UpdateMaxPingValue(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)baseAddr + 0x000DB634, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] MaxPingValueBytes = new byte[4];
            int MaxPingValueRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, Ptr1Addr, MaxPingValueBytes, MaxPingValueBytes.Length, ref MaxPingValueRead);
            int MaxPingValue = BitConverter.ToInt32(MaxPingValueBytes, 0);

            int MaxPingValueWritten = 0;
            byte[] MaxPingValueWrite = BitConverter.GetBytes(_state.Instances[InstanceID].gameMaxPingValue);
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, MaxPingValueWrite, MaxPingValueWrite.Length, ref MaxPingValueWritten);
        }
        public void UpdateOneShotKills(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)baseAddr + 0x000D8580, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] OneShotKillsBytes = new byte[4];
            int OneShotKillsRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, Ptr1Addr, OneShotKillsBytes, OneShotKillsBytes.Length, ref OneShotKillsRead);
            int OneShotKills = BitConverter.ToInt32(OneShotKillsBytes, 0);

            int OneShotKillsWritten = 0;
            byte[] OneShotKillsWrite = new byte[4];
            OneShotKillsWrite = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[InstanceID].gameOneShotKills));

            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, OneShotKillsWrite, OneShotKillsWrite.Length, ref OneShotKillsWritten);
        }
        public void UpdateFatBullets(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)baseAddr + 0x000D7F14, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] FatBulletsBytes = new byte[4];
            int FatBulletsRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, Ptr1Addr, FatBulletsBytes, FatBulletsBytes.Length, ref FatBulletsRead);
            int FatBullets = BitConverter.ToInt32(FatBulletsBytes, 0);

            int FatBulletsWritten = 0;
            byte[] FatBulletsWrite = new byte[4];
            FatBulletsWrite = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[InstanceID].gameFatBullets));

            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, FatBulletsWrite, FatBulletsWrite.Length, ref FatBulletsWritten);
        }
        public void UpdateDestroyBuildings(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)baseAddr + 0x000D99B8, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] DestroyBuildingsBytes = new byte[4];
            int DestroyBuildingsRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, Ptr1Addr, DestroyBuildingsBytes, DestroyBuildingsBytes.Length, ref DestroyBuildingsRead);
            int DestroyBuildings = BitConverter.ToInt32(DestroyBuildingsBytes, 0);

            int DestroyBuildingsWritten = 0;
            byte[] DestroyBuildingsWrite = new byte[4];
            DestroyBuildingsWrite = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[InstanceID].gameDestroyBuildings));

            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, DestroyBuildingsWrite, DestroyBuildingsWrite.Length, ref DestroyBuildingsWritten);
        }
        public void UpdateBluePassword(AppState _state, int InstanceID, int oldPwLength)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)baseAddr + 0x005F204A, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] BluePasswordBytes = new byte[16];
            int BluePasswordRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, Ptr1Addr, BluePasswordBytes, BluePasswordBytes.Length, ref BluePasswordRead);
            string BluePassword = Encoding.Default.GetString(BluePasswordBytes).Replace("\0", "");

            int BluePasswordWritten = 0;
            byte[] BluePasswordWrite = Encoding.Default.GetBytes(_state.Instances[InstanceID].gamePasswordBlue);
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
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)baseAddr + 0x006343D3, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] RedPasswordBytes = new byte[16];
            int RedPasswordRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, Ptr1Addr + 0x33, RedPasswordBytes, RedPasswordBytes.Length, ref RedPasswordRead);
            string RedPassword = Encoding.Default.GetString(RedPasswordBytes).Replace("\0", "");

            int RedPasswordWritten = 0;
            byte[] RedPasswordWrite = Encoding.Default.GetBytes(_state.Instances[InstanceID].gamePasswordRed);
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
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)baseAddr + 0x000DB6FC, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] PSPTakeOverTimeBytes = new byte[4];
            int PSPTakeOverTimeRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, Ptr1Addr + 0x4, PSPTakeOverTimeBytes, PSPTakeOverTimeBytes.Length, ref PSPTakeOverTimeRead);
            int PSPTakeOverTimeValue = BitConverter.ToInt32(PSPTakeOverTimeBytes, 0);

            int PSPTakeOverTimeWritten = 0;
            byte[] PSPTakeOverTimeWrite = BitConverter.GetBytes(_state.Instances[InstanceID].gamePSPTOTimer);
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr + 0x4, PSPTakeOverTimeWrite, PSPTakeOverTimeWrite.Length, ref PSPTakeOverTimeWritten);
        }
        public void UpdateFlagReturnTime(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)baseAddr + 0x000DB6AC, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] FlagReturnTimeBytes = new byte[4];
            int FlagReturnTimeRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, Ptr1Addr, FlagReturnTimeBytes, FlagReturnTimeBytes.Length, ref FlagReturnTimeRead);
            int FlagReturnTime = BitConverter.ToInt32(FlagReturnTimeBytes, 0);

            int FlagReturnTimeWritten = 0;
            byte[] FlagReturnTimeWrite = BitConverter.GetBytes(_state.Instances[InstanceID].gameFlagReturnTime);
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, FlagReturnTimeWrite, FlagReturnTimeWrite.Length, ref FlagReturnTimeWritten);
        }
        public void UpdateMaxTeamLives(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)baseAddr + 0x000D8554, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] MaxTeamLivesBytes = new byte[4];
            int MaxTeamLivesRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, Ptr1Addr, MaxTeamLivesBytes, MaxTeamLivesBytes.Length, ref MaxTeamLivesRead);
            int MaxTeamLives = BitConverter.ToInt32(MaxTeamLivesBytes, 0);

            int MaxTeamLivesWritten = 0;
            byte[] MaxTeamLivesWrite = BitConverter.GetBytes(_state.Instances[InstanceID].gameMaxTeamLives);
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, MaxTeamLivesWrite, MaxTeamLivesWrite.Length, ref MaxTeamLivesWritten);
        }
        public void UpdateFriendlyFireKills(AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)baseAddr + 0x000DB684, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] FriendlyFireKillsBytes = new byte[4];
            int FriendlyFireKillsRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, Ptr1Addr, FriendlyFireKillsBytes, FriendlyFireKillsBytes.Length, ref FriendlyFireKillsRead);
            int FriendlyFireKills = BitConverter.ToInt32(FriendlyFireKillsBytes, 0);

            int FriendlyFireKillsWritten = 0;
            byte[] FriendlyFireKillsWrite = BitConverter.GetBytes(_state.Instances[InstanceID].gameFriendlyFireKills);
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, FriendlyFireKillsWrite, FriendlyFireKillsWrite.Length, ref FriendlyFireKillsWritten);
        }
        public void GamePlayOptions(AppState _state, int InstanceID)
        {
            int gameOptions = CalulateGameOptions(_state.Instances[InstanceID]);
            byte[] gameOptionsBytes = BitConverter.GetBytes(gameOptions);
            var baseAddr = 0x400000;

            byte[] Ptr1 = new byte[4];
            int Ptr1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)baseAddr + 0x000D7D6C, Ptr1, Ptr1.Length, ref Ptr1Read);

            int Ptr1Addr = BitConverter.ToInt32(Ptr1, 0);
            byte[] GamePlayOptionsOneBytes = new byte[4];
            int GamePlayOptionsOneRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, Ptr1Addr, GamePlayOptionsOneBytes, GamePlayOptionsOneBytes.Length, ref GamePlayOptionsOneRead);
            int GamePlayOptionsOne = BitConverter.ToInt32(GamePlayOptionsOneBytes, 0);

            int GamePlayOptionsOneWritten = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], Ptr1Addr, gameOptionsBytes, gameOptionsBytes.Length, ref GamePlayOptionsOneWritten);
        }
        public int CalulateGameOptions(Instance theInstance)
        {
            bool autoBalance = theInstance.gameOptionAutoBalance;
            bool friendlyFire = theInstance.gameOptionFF;
            bool friendlyTags = theInstance.gameOptionFriendlyTags;
            bool friendlyFireWarning = theInstance.gameOptionFFWarn;
            bool showTracers = theInstance.gameOptionShowTracers;
            bool showTeamClays = theInstance.gameShowTeamClays;
            bool allowAutoRange = theInstance.gameOptionAutoRange;

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
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, Ptr1Location, Ptr1Bytes, Ptr1Bytes.Length, ref Ptr1Read);

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
            if (_state.Instances[InstanceID].MapListPrevious.Count > 128)
            {
                throw new Exception("Someway, somehow, someone bypassed the maplist checks. You are NOT allowed to have more than 128 maps. #88");
            }
            int appEntry = 0x400000;

            byte[] ServerMapCyclePtr = new byte[4];
            int Pointer2Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, appEntry + 0x005ED5F8, ServerMapCyclePtr, ServerMapCyclePtr.Length, ref Pointer2Read);
            int mapCycleServerAddress = BitConverter.ToInt32(ServerMapCyclePtr, 0);

            int mapCycleTotalAddress = mapCycleServerAddress + 0x4;
            byte[] mapTotal = BitConverter.GetBytes(_state.Instances[InstanceID].MapListCurrent.Count);
            int mapTotalWritten = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], mapCycleTotalAddress, mapTotal, mapTotal.Length, ref mapTotalWritten);


            int mapCycleCurrentIndex = mapCycleServerAddress + 0xC;
            byte[] resetMapIndex = BitConverter.GetBytes(_state.Instances[InstanceID].MapListCurrent.Count);
            int resetMapIndexWritten = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], mapCycleCurrentIndex, resetMapIndex, resetMapIndex.Length, ref resetMapIndexWritten);


            byte[] mapCycleListAddress = new byte[4];
            int mapCycleListAddressRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, mapCycleServerAddress, mapCycleListAddress, mapCycleListAddress.Length, ref mapCycleListAddressRead);
            int mapCycleList = BitConverter.ToInt32(mapCycleListAddress, 0);

            foreach (var entry in _state.Instances[InstanceID].MapListPrevious)
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
            if (_state.Instances[InstanceID].MapListCurrent.Count > 128)
            {
                throw new Exception("Someway, somehow, someone bypassed the maplist checks. You are NOT allowed to have more than 128 maps. #89");
            }
            int appEntry = 0x400000;

            byte[] Pointer1Bytes = new byte[4];
            int Pointer1Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, appEntry + 0x000DC6D8, Pointer1Bytes, Pointer1Bytes.Length, ref Pointer1Read);
            int mapCycleClientAddress = BitConverter.ToInt32(Pointer1Bytes, 0);

            string mapCycleClientString = "";

            mapCycleClientString += _state.autoRes.StringToHex(_state.Instances[InstanceID].MapListCurrent[0].MapFile, 28) + " 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ";
            if (_state.Instances[InstanceID].MapListCurrent[0].MapName.Length > 31)
            {
                string ModifiedmapTitle = "";
                for (int i = 0; i < 31; i++)
                {
                    string mapTitle = _state.Instances[InstanceID].MapListCurrent[0].MapName;
                    ModifiedmapTitle += mapTitle[i];
                }
                mapCycleClientString += _state.autoRes.StringToHex(ModifiedmapTitle, 28) + " 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 20 B5 B6 01 00 00 00 00 53 00 00 00 00 ";
            }
            else
            {
                mapCycleClientString += _state.autoRes.StringToHex(_state.Instances[InstanceID].MapListCurrent[0].MapName, 28) + " 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 20 B5 B6 01 00 00 00 00 53 00 00 00 00 ";
            }
            mapCycleClientString += _state.autoRes.IntToHex(_state.Instances[InstanceID].gameScoreKills) + " 14 00 00 00 05 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ";
            mapCycleClientString += _state.autoRes.BoolToHex(_state.Instances[InstanceID].MapListCurrent[0].CustomMap) + " 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 "; // custom map flag for FIRST MAP IN ROTATION

            for (int Index = 0; Index < _state.Instances[InstanceID].MapListCurrent.Count; Index++)
            {
                if (Index != 0)
                {
                    mapCycleClientString += _state.autoRes.StringToHex(_state.Instances[InstanceID].MapListCurrent[Index].MapFile, 28) + " 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ";
                    if (_state.Instances[InstanceID].MapListCurrent[Index].MapName.Length > 31)
                    {
                        string ModifiedmapTitleLoop = "";
                        for (int iMap = 0; iMap < 31; iMap++)
                        {
                            ModifiedmapTitleLoop += _state.Instances[InstanceID].MapListCurrent[Index].MapName[iMap];
                        }
                        mapCycleClientString += _state.autoRes.StringToHex(ModifiedmapTitleLoop, 28) + " 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 20 B5 B6 01 00 00 00 00 53 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ";
                    }
                    else
                    {
                        mapCycleClientString += _state.autoRes.StringToHex(_state.Instances[InstanceID].MapListCurrent[Index].MapName, 28) + " 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 20 B5 B6 01 00 00 00 00 53 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ";
                    }
                    mapCycleClientString += _state.autoRes.BoolToHex(_state.Instances[InstanceID].MapListCurrent[Index].CustomMap) + " 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ";
                }
            }

            byte[] mapCycleClientBytes = HexConverter.ToByteArray(mapCycleClientString.Replace(" ", ""));
            int mapCycleClientWritten = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], mapCycleClientAddress, mapCycleClientBytes, mapCycleClientBytes.Length, ref mapCycleClientWritten);
            UpdateSecondaryMapList(_state, _state.Instances[InstanceID].instanceProcessHandle, InstanceID);
        }
        public void UpdateSecondaryMapList(AppState _state, IntPtr handle, int InstanceID)
        {
            int appEntry = 0x400000;
            byte[] ServerMapCyclePtr = new byte[4];
            int Pointer2Read = 0;
            ReadProcessMemory((int)handle, appEntry + 0x005ED5F8, ServerMapCyclePtr, ServerMapCyclePtr.Length, ref Pointer2Read);
            int mapCycleServerAddress = BitConverter.ToInt32(ServerMapCyclePtr, 0);

            int mapCycleTotalAddress = mapCycleServerAddress + 0x4;
            byte[] mapTotal = BitConverter.GetBytes(_state.Instances[InstanceID].MapListCurrent.Count);
            int mapTotalWritten = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], mapCycleTotalAddress, mapTotal, mapTotal.Length, ref mapTotalWritten);


            int mapCycleCurrentIndex = mapCycleServerAddress + 0xC;
            byte[] resetMapIndex = BitConverter.GetBytes(_state.Instances[InstanceID].MapListCurrent.Count);
            int resetMapIndexWritten = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], mapCycleCurrentIndex, resetMapIndex, resetMapIndex.Length, ref resetMapIndexWritten);


            byte[] mapCycleListAddress = new byte[4];
            int mapCycleListAddressRead = 0;
            ReadProcessMemory((int)handle, mapCycleServerAddress, mapCycleListAddress, mapCycleListAddress.Length, ref mapCycleListAddressRead);
            int mapCycleList = BitConverter.ToInt32(mapCycleListAddress, 0);


            for (int i = 0; i < _state.Instances[InstanceID].MapListCurrent.Count; i++)
            {
                int mapFileIndexLocation = mapCycleList;
                byte[] mapFileBytes = Encoding.ASCII.GetBytes(_state.Instances[InstanceID].MapListCurrent[i].MapFile);
                int mapFileBytesWritten = 0;
                MemoryProcessor.Write(_state.Instances[InstanceID], mapFileIndexLocation, mapFileBytes, mapFileBytes.Length, ref mapFileBytesWritten);
                mapFileIndexLocation += 0x20;

                byte[] customMapFlag = BitConverter.GetBytes(Convert.ToInt32(_state.Instances[InstanceID].MapListCurrent[i].CustomMap));
                int customMapFlagWritten = 0;
                MemoryProcessor.Write(_state.Instances[InstanceID], mapFileIndexLocation, customMapFlag, customMapFlag.Length, ref customMapFlagWritten);
                mapCycleList += 0x24;
            }

            /*if (_state.Instances[InstanceID].MapListCurrent.Count != _state.Instances[InstanceID].infoCounterMaps)
            {
                for (int i = 0; i < _state.Instances[InstanceID].infoCounterMaps; i++)
                {
                    if (_state.Instances[InstanceID].MapListCurrent.ContainsKey(i))
                    {
                        byte[] mapFileBytes = Encoding.ASCII.GetBytes(_state.Instances[InstanceID].MapListCurrent[i].MapFile);
                        int mapFileBytesWritten = 0;
                        WriteProcessMemory((int)handle, mapCycleList, mapFileBytes, mapFileBytes.Length, ref mapFileBytesWritten);


                    }

                }
            }
            else if (_state.Instances[InstanceID].MapListCurrent.Count == _state.Instances[InstanceID].infoCounterMaps)
            {
                for (int i = 0; i < _state.Instances[InstanceID].MapListCurrent.Count; i++)
                {

                }
            }*/
        }
        public void UpdateNextMap(AppState _state, int InstanceID, int NextMapIndex)
        {
            int appEntry = 0x400000;
            byte[] ServerMapCyclePtr = new byte[4];
            int Pointer2Read = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, appEntry + 0x005ED5F8, ServerMapCyclePtr, ServerMapCyclePtr.Length, ref Pointer2Read);
            int MapCycleIndex = BitConverter.ToInt32(ServerMapCyclePtr, 0) + 0xC;
            int NewMapIndex = NextMapIndex;
            if (NewMapIndex - 1 == -1)
            {
                NewMapIndex = _state.Instances[InstanceID].MapListCurrent.Count;
            }
            else if (_state.Instances[InstanceID].MapListCurrent.ContainsKey(NewMapIndex - 1))
            {
                NewMapIndex--;
            }
            byte[] newMapIndexBytes = BitConverter.GetBytes(NewMapIndex);
            int newMapIndexBytesWritten = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], MapCycleIndex, newMapIndexBytes, newMapIndexBytes.Length, ref newMapIndexBytesWritten);
        }
        public void SetHostnames(Instance _instance)
        {
            bool processExists = ProcessExist(_instance.instanceAttachedPID.GetValueOrDefault());
            if (processExists == true)
            {
                int buffer = 0;
                byte[] PointerAddr = new byte[4];
                var baseAddr = 0x400000;
                var Pointer = baseAddr + 0x005ED600;
                ReadProcessMemory((int)_instance.instanceProcessHandle, (int)Pointer, PointerAddr, PointerAddr.Length, ref buffer);
                int buffer2 = 0;
                byte[] Hostname = Encoding.Default.GetBytes(_instance.gameHostName + "\0");
                var Address2HostName = BitConverter.ToInt32(PointerAddr, 0);
                MemoryProcessor.Write(_instance, (int)Address2HostName + 0x3C, Hostname, Hostname.Length, ref buffer2);
            }
        }
        public void SendChatMessage(ref AppState _state, int InstanceID, string ChatChannel, string Msg)
        {
            // Set Chat Color
            int colorbuffer_written = 0;
            byte[] colorcode = HexConverter.ToByteArray(ChatChannel);
            MemoryProcessor.Write(_state.Instances[InstanceID], 0x00462ABA, colorcode, colorcode.Length, ref colorbuffer_written);
            Thread.Sleep(100);

            // Open Console
            PostMessage(_state.ApplicationProcesses[InstanceID].MainWindowHandle, WM_KEYDOWN, chatConsole, 0);
            PostMessage(_state.ApplicationProcesses[InstanceID].MainWindowHandle, WM_KEYUP, chatConsole, 0);
            Thread.Sleep(100);
            int bytesWritten = 0;

            // Write Message
            byte[] buffer = Encoding.Default.GetBytes($"{Msg}\0"); // '\0' marks the end of string
            MemoryProcessor.Write(_state.Instances[InstanceID], 0x00879A14, buffer, buffer.Length, ref bytesWritten);
            Thread.Sleep(100);
            PostMessage(_state.ApplicationProcesses[InstanceID].MainWindowHandle, WM_KEYDOWN, VK_ENTER, 0);
            PostMessage(_state.ApplicationProcesses[InstanceID].MainWindowHandle, WM_KEYUP, VK_ENTER, 0);

            // Revert Chat Color
            Thread.Sleep(100);
            int revert_colorbuffer = 0;
            byte[] revert_colorcode = HexConverter.ToByteArray(ChatManagement.ChatChannels[0]);
            MemoryProcessor.Write(_state.Instances[InstanceID], 0x00462ABA, revert_colorcode, revert_colorcode.Length, ref revert_colorbuffer);

        }
        public void SendConsoleCommand(ref AppState _state, int InstanceID, string Command)
        {
            // open cmdConsole
            PostMessage(_state.ApplicationProcesses[InstanceID].MainWindowHandle, WM_KEYDOWN, cmdConsole, 0);
            PostMessage(_state.ApplicationProcesses[InstanceID].MainWindowHandle, WM_KEYUP, cmdConsole, 0);
            Thread.Sleep(100);

            // Write to cmdConsole
            int bytesWritten_kick = 0;
            byte[] buffer_kick = Encoding.Default.GetBytes($"{Command}\0"); // '\0' marks the end of string
            MemoryProcessor.Write(_state.Instances[InstanceID], 0x00879A14, buffer_kick, buffer_kick.Length, ref bytesWritten_kick);
            Thread.Sleep(100);
            PostMessage(_state.ApplicationProcesses[InstanceID].MainWindowHandle, WM_KEYDOWN, VK_ENTER, 0);
            PostMessage(_state.ApplicationProcesses[InstanceID].MainWindowHandle, WM_KEYUP, VK_ENTER, 0);
        }
        public int GetTimeLeft(ref AppState _state, int InstaceID)
        {
            Instance instance = _state.Instances[InstaceID];

            if (instance.instanceStatus == InstanceStatus.LOADINGMAP)
            {
                return (instance.gameStartDelay + instance.gameTimeLimit);
            }

            var baseAddr = 0x400000;
            byte[] Ptr = new byte[4];
            int ReadPtr = 0;

            ReadProcessMemory((int)instance.instanceProcessHandle, (int)baseAddr + 0x00061098, Ptr, Ptr.Length, ref ReadPtr);
            int MapTimeAddr = BitConverter.ToInt32(Ptr, 0);

            Stopwatch stopwatchProcessingTime = new Stopwatch();
            stopwatchProcessingTime.Start();

            byte[] MapTimeMs = new byte[4];
            int MapTimeRead = 0;
            ReadProcessMemory((int)instance.instanceProcessHandle, MapTimeAddr, MapTimeMs, MapTimeMs.Length, ref MapTimeRead);
            int MapTime = BitConverter.ToInt32(MapTimeMs, 0);
            int MapTimeInSeconds = MapTime / 60;

            DateTime MapStartTime = DateTime.Now - TimeSpan.FromSeconds(MapTimeInSeconds);
            DateTime MapEndTime = MapStartTime + TimeSpan.FromMinutes(instance.gameStartDelay + instance.gameTimeLimit);

            byte[] TimeOffset = new byte[4];
            int TimeOffsetRead = 0;
            ReadProcessMemory((int)instance.instanceProcessHandle, MapTimeAddr, TimeOffset, TimeOffset.Length, ref TimeOffsetRead);
            int intTimeOffset = BitConverter.ToInt32(TimeOffset, 0);

            TimeSpan TimeRemaining = MapEndTime - (DateTime.Now + TimeSpan.FromMilliseconds(stopwatchProcessingTime.ElapsedMilliseconds) - TimeSpan.FromMilliseconds(intTimeOffset));
            stopwatchProcessingTime.Stop();

            return Math.Max(TimeRemaining.Minutes, 0);

        }
        public InternalPlayerStats GetPlayerStats(ref AppState _state, int instanceid, int reqslot)
        {

            var baseaddr = 0x400000;
            var startList = baseaddr + 0x005ED600;

            byte[] startaddr = new byte[4];
            int startaddr_read = 0;
            ReadProcessMemory((int)_state.Instances[instanceid].instanceProcessHandle, (int)startList, startaddr, startaddr.Length, ref startaddr_read);
            var firstplayer = BitConverter.ToInt32(startaddr, 0) + 0x28;

            byte[] scanbeginaddr = new byte[4];
            ReadProcessMemory((int)_state.Instances[instanceid].instanceProcessHandle, (int)firstplayer, scanbeginaddr, scanbeginaddr.Length, ref startaddr_read);
            int beginaddr = BitConverter.ToInt32(scanbeginaddr, 0);

            if (reqslot != 1)
            {
                for (int i = 1; i < reqslot; i++)
                {
                    beginaddr += 0xAF33C;
                }
            }
            byte[] read_name = new byte[15];
            int bytesread = 0;

            ReadProcessMemory((int)_state.Instances[instanceid].instanceProcessHandle, (int)beginaddr + 0x1C, read_name, read_name.Length, ref bytesread);
            var PlayerName = Encoding.Default.GetString(read_name).Replace("\0", "");

            if (string.IsNullOrEmpty(PlayerName))
            {
                beginaddr += 0xAF33C;
                // Retry read if player name is empty
                ReadProcessMemory((int)_state.Instances[instanceid].instanceProcessHandle, (int)beginaddr + 0x1C, read_name, read_name.Length, ref bytesread);
                PlayerName = Encoding.Default.GetString(read_name).Replace("\0", "");
            }

            // Handle failure if still no player name found
            if (string.IsNullOrEmpty(PlayerName))
            {
                log.Debug("Something went wrong here. We can't find any player names.");
                return new InternalPlayerStats();
            }

            byte[] read_ping = new byte[4];
            ReadProcessMemory((int)_state.Instances[instanceid].instanceProcessHandle, (int)beginaddr + 0x000ADB40, read_ping, read_ping.Length, ref bytesread);

            int[] offsets = {
                0x000ADAB4, 0x000ADA94, 0x000ADA98, 0x000ADA8C, 0x000ADAD0,
                0x000ADA90, 0x000ADAD4, 0x000ADAF4, 0x000ADABC, 0x000ADAC0,
                0x000ADAC4, 0x000ADACC, 0x000ADACC, 0x000ADAA8, 0x000ADAC8,
                0x000ADAD4, 0x000ADAA4, 0x000ADADC, 0x000ADA94, 0x000ADAB0,
                0x000ADAAC, 0x000ADAD8, 0x000ADADC, 0x000ADAE0
            };

            var stats = new int[offsets.Length];

            for (int i = 0; i < offsets.Length; i++)
            {
                byte[] read_data = new byte[4];
                ReadProcessMemory((int)_state.Instances[instanceid].instanceProcessHandle, (int)beginaddr + offsets[i], read_data, read_data.Length, ref bytesread);
                stats[i] = BitConverter.ToInt32(read_data, 0);
            }

            // Trail Checks
            int[] offsets2 =
            {
                0x000ADA90, 0x000ADA94, 0x000ADA98, 0x000ADA9C,
                0x000ADAA8, 0x000ADAAC, 0x000ADAB0, 0x000ADAB4,
                0x000ADAB8, 0x000ADABC, 0x000ADAC0, 0x000ADAC4,
                0x000ADAC8, 0x000ADACC, 0x000ADAD0, 0x000ADAD4,
                0x000ADAE4, 0x000ADAE8, 0x000ADAEC
            };
            var stats2 = new int[offsets2.Length];
            for (int i = 0; i < offsets2.Length; i++)
            {
                byte[] read_data = new byte[4];
                ReadProcessMemory((int)_state.Instances[instanceid].instanceProcessHandle, (int)beginaddr + offsets2[i], read_data, read_data.Length, ref bytesread);
                stats2[i] = BitConverter.ToInt32(read_data, 0);
            }
            //Console.WriteLine(PlayerName);
            //Console.WriteLine(JsonConvert.SerializeObject(stats));
            //Console.WriteLine(JsonConvert.SerializeObject(stats2));

            byte[] read_playerObjectLocation = new byte[4];
            ReadProcessMemory((int)_state.Instances[instanceid].instanceProcessHandle, (int)beginaddr + 0x5E7C, read_playerObjectLocation, read_playerObjectLocation.Length, ref bytesread);
            int read_playerObject = BitConverter.ToInt32(read_playerObjectLocation, 0);

            byte[] read_selectedWeapon = new byte[4];
            ReadProcessMemory((int)_state.Instances[instanceid].instanceProcessHandle, (int)read_playerObject + 0x178, read_selectedWeapon, read_selectedWeapon.Length, ref bytesread);
            int SelectedWeapon = BitConverter.ToInt32(read_selectedWeapon, 0);

            byte[] read_selectedCharacterClass = new byte[4];
            ReadProcessMemory((int)_state.Instances[instanceid].instanceProcessHandle, (int)read_playerObject + 0x244, read_selectedCharacterClass, read_selectedCharacterClass.Length, ref bytesread);
            int SelectedCharacterClass = BitConverter.ToInt32(read_selectedCharacterClass, 0);

            byte[] read_weapons = new byte[250];
            ReadProcessMemory((int)_state.Instances[instanceid].instanceProcessHandle, (int)beginaddr + 0x000ADB70, read_weapons, read_weapons.Length, ref bytesread);
            var MemoryWeapons = Encoding.Default.GetString(read_weapons).Replace("\0", "|");
            string[] weapons = MemoryWeapons.Split('|');
            List<string> WeaponList = new List<string>();

            int failureCount = 0;
            foreach (var item in weapons)
            {
                if (!string.IsNullOrEmpty(item) && failureCount != 3)
                {
                    WeaponList.Add(item);
                }
                else
                {
                    if (failureCount == 3)
                    {
                        break;
                    }
                    else
                    {
                        failureCount++;
                    }
                }
            }
            
            return new InternalPlayerStats
            {
                PlayerName = PlayerName,
                ping = BitConverter.ToInt32(read_ping, 0),
                CharacterClass = SelectedCharacterClass,
                SelectedWeapon = SelectedWeapon,
                PlayerWeapons = WeaponList,
                TotalShotsFired = stats[0],
                kills = stats[1],
                deaths = stats[2],
                suicides = stats[3],
                headshots = stats[4],
                teamkills = stats[5],
                knifekills = stats[6],
                exp = stats[7],
                revives = stats[8],
                pspattempts = stats[9],
                psptakeover = stats[10],
                doublekills = stats[11],
                playerrevives = stats[12],
                FBCaptures = stats[13],
                FBCarrierKills = stats[14],
                FBCarrierDeaths = stats[15],
                ZoneTime = stats[16],
                ZoneKills = stats[17],
                ZoneDefendKills = stats[18],
                ADTargetsDestroyed = stats[19],
                FlagSaves = stats[20],
                sniperkills = stats[21],
                tkothdefensekills = stats[22],
                tkothattackkills = stats[23]
            };
        }
        public int GetCurrentPlayers(ref AppState _state, int instanceid)
        {
            int bytesRead = 0;
            byte[] buffer = new byte[4];
            ReadProcessMemory((int)_state.Instances[instanceid].instanceProcessHandle, 0x0065DCBC, buffer, buffer.Length, ref bytesRead);
            int CurrentPlayers = BitConverter.ToInt32(buffer, 0);
            return CurrentPlayers;
        }
        public int GetCurrentGameType(ref AppState _state, int instanceid)
        {
            // memory polling
            int bytesRead = 0;
            byte[] buffer = new byte[4];
            ReadProcessMemory((int)_state.Instances[instanceid].instanceProcessHandle, 0x009F21A4, buffer, buffer.Length, ref bytesRead);
            int GameType = BitConverter.ToInt32(buffer, 0);

            return GameType;
        }
        public string GetCurrentMission(ref AppState _state, int instanceid)
        {
            // memory polling
            int bytesRead = 0;
            byte[] buffer = new byte[26];
            ReadProcessMemory((int)_state.Instances[instanceid].instanceProcessHandle, 0x0071569C, buffer, buffer.Length, ref bytesRead);
            string MissionName = Encoding.Default.GetString(buffer);

            return MissionName.Replace("\0", "");
        }
        public string GetPlayerIpAddress(ref AppState _state, int instanceid, string playername)
        {
            const int baseAddr = 0x400000;
            const int playerIpAddressPointerOffset = 0x00ACE248;
            const int playernameOffset = 0xBC;

            int playerIpAddressPointerBuffer = 0;
            byte[] PointerAddr_2 = new byte[4];
            ReadProcessMemory((int)_state.Instances[instanceid].instanceProcessHandle, baseAddr + playerIpAddressPointerOffset, PointerAddr_2, PointerAddr_2.Length, ref playerIpAddressPointerBuffer);

            int IPList = BitConverter.ToInt32(PointerAddr_2, 0) + playernameOffset;

            int failureCounter = 0;
            while (failureCounter <= _state.Instances[instanceid].gameMaxSlots)
            {
                byte[] playername_bytes = new byte[15];
                int playername_buffer = 0;
                ReadProcessMemory((int)_state.Instances[instanceid].instanceProcessHandle, IPList, playername_bytes, playername_bytes.Length, ref playername_buffer);
                var currentPlayerName = Encoding.Default.GetString(playername_bytes).Replace("\0", "");

                if (currentPlayerName == playername)
                {
                    failureCounter = 0;
                    break;
                }

                IPList += playernameOffset;
                failureCounter++;
            }

            if (failureCounter > _state.Instances[instanceid].gameMaxSlots)
            {
                return null;
            }

            byte[] playerIPBytesPtr = new byte[4];
            int playerIPBufferPtr = 0;
            ReadProcessMemory((int)_state.Instances[instanceid].instanceProcessHandle, IPList + 0xA4, playerIPBytesPtr, playerIPBytesPtr.Length, ref playerIPBufferPtr);

            int PlayerIPLocation = BitConverter.ToInt32(playerIPBytesPtr, 0) + 4;
            byte[] playerIPAddressBytes = new byte[4];
            int playerIPAddressBuffer = 0;
            ReadProcessMemory((int)_state.Instances[instanceid].instanceProcessHandle, PlayerIPLocation, playerIPAddressBytes, playerIPAddressBytes.Length, ref playerIPAddressBuffer);

            IPAddress playerIp = new IPAddress(playerIPAddressBytes);
            return playerIp.ToString();
        }
        public Dictionary<int, ob_playerList> CurrentPlayerList(ref AppState _state, int instanceid)
        {
            Dictionary<int, ob_playerList> currentPlayerList = new Dictionary<int, ob_playerList>();
            int NumPlayers = GetCurrentPlayers(ref _state, instanceid);

            if (NumPlayers > 0)
            {
                int buffer = 0;
                var baseAddr = 0x400000;
                var Pointer = baseAddr + 0x005ED600;

                byte[] PointerAddr9 = new byte[4];
                ReadProcessMemory((int)_state.Instances[instanceid].instanceProcessHandle, (int)Pointer, PointerAddr9, PointerAddr9.Length, ref buffer);
                var playerlistStartingLocationPointer = BitConverter.ToInt32(PointerAddr9, 0) + 0x28;

                byte[] playerListStartingLocationByteArray = new byte[4];
                ReadProcessMemory((int)_state.Instances[instanceid].instanceProcessHandle, (int)playerlistStartingLocationPointer, playerListStartingLocationByteArray, playerListStartingLocationByteArray.Length, ref buffer);

                int playerlistStartingLocation = BitConverter.ToInt32(playerListStartingLocationByteArray, 0);
                int failureCount = 0;

                for (int i = 0; i < NumPlayers; i++)
                {
                    if (failureCount == _state.Instances[instanceid].gameMaxSlots)
                    {
                        break;
                    }

                    byte[] slotNumberValue = new byte[4];
                    int slotNumberLocation = playerlistStartingLocation + 0xC;
                    ReadProcessMemory((int)_state.Instances[instanceid].instanceProcessHandle, slotNumberLocation, slotNumberValue, slotNumberValue.Length, ref buffer);
                    int playerSlot = BitConverter.ToInt32(slotNumberValue, 0);

                    byte[] playerNameBytes = new byte[15];
                    int playerNameLocation = playerlistStartingLocation + 0x1C;
                    ReadProcessMemory((int)_state.Instances[instanceid].instanceProcessHandle, playerNameLocation, playerNameBytes, playerNameBytes.Length, ref buffer);
                    string formattedPlayerName = Encoding.Default.GetString(playerNameBytes).Replace("\0", "");

                    if (string.IsNullOrEmpty(formattedPlayerName) || string.IsNullOrWhiteSpace(formattedPlayerName))
                    {
                        playerlistStartingLocation += 0xAF33C;
                        i--;
                        failureCount++;
                        continue;
                    }

                    byte[] playerTeamBytes = new byte[4];
                    int playerTeamLocation = playerlistStartingLocation + 0x90;
                    ReadProcessMemory((int)_state.Instances[instanceid].instanceProcessHandle, playerTeamLocation, playerTeamBytes, playerTeamBytes.Length, ref buffer);
                    int playerTeam = BitConverter.ToInt32(playerTeamBytes, 0);

                    string playerIP = GetPlayerIpAddress(ref _state, instanceid, formattedPlayerName).ToString();
                    InternalPlayerStats PlayerStats = GetPlayerStats(ref _state, instanceid, playerSlot);
                    ob_playerList.CharacterClass PlayerCharacterClass = (ob_playerList.CharacterClass)PlayerStats.CharacterClass;
                    ob_playerList.WeaponStack PlayerSelectedWeapon = (ob_playerList.WeaponStack)PlayerStats.SelectedWeapon;

                    Dictionary<int, List<ob_playerList.WeaponStack>> PlayerWeapons = new Dictionary<int, List<ob_playerList.WeaponStack>>();

                    if (string.IsNullOrEmpty(formattedPlayerName) || string.IsNullOrWhiteSpace(formattedPlayerName))
                    {
                        if (currentPlayerList.Count >= NumPlayers)
                        {
                            break;
                        }
                        else
                        {
                            playerlistStartingLocation += 0xAF33C;
                            continue;
                        }
                    }
                    else
                    {
                        try
                        {
                            
                            currentPlayerList.Add(playerSlot, new ob_playerList
                            {
                                slot = playerSlot,
                                name = formattedPlayerName,
                                nameBase64 = Crypt.Base64Encode(formattedPlayerName),
                                team = playerTeam,
                                address = playerIP,
                                ping = PlayerStats.ping,
                                PlayerClass = PlayerCharacterClass.ToString(),
                                selectedWeapon = PlayerSelectedWeapon.ToString(),
                                weapons = PlayerStats.PlayerWeapons,
                                totalshots = PlayerStats.TotalShotsFired,
                                kills = PlayerStats.kills,
                                deaths = PlayerStats.deaths,
                                suicides = PlayerStats.suicides,
                                headshots = PlayerStats.headshots,
                                teamkills = PlayerStats.teamkills,
                                knifekills = PlayerStats.knifekills,
                                exp = PlayerStats.exp,
                                revives = PlayerStats.revives,
                                playerrevives = PlayerStats.playerrevives,
                                pspattempts = PlayerStats.pspattempts,
                                psptakeover = PlayerStats.psptakeover,
                                doublekills = PlayerStats.doublekills,
                                flagcaptures = PlayerStats.FBCaptures,
                                flagcarrierkills = PlayerStats.FBCarrierKills,
                                flagcarrierdeaths = PlayerStats.FBCarrierDeaths,
                                zonetime = PlayerStats.ZoneTime,
                                zonekills = PlayerStats.ZoneKills,
                                zonedefendkills = PlayerStats.ZoneDefendKills,
                                ADTargetsDestroyed = PlayerStats.ADTargetsDestroyed,
                                FlagSaves = PlayerStats.FlagSaves,
                                sniperkills = PlayerStats.sniperkills,
                                tkothdefensekills = PlayerStats.tkothdefensekills,
                                tkothattackkills = PlayerStats.tkothattackkills
                            });

                            playerlistStartingLocation += 0xAF33C;

                            if (currentPlayerList.Count >= NumPlayers)
                            {
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            _state.eventLog.WriteEntry("Detected an error!\n\n" + "Player Name: " + playerSlot + "\n\n" + formattedPlayerName + "\n\n" + e.ToString(), EventLogEntryType.Error);
                        }

                    }
                    
                }
            }
            return currentPlayerList;
        }
        public void GetCurrentMapIndex(ref AppState _state, int ArrayID)
        {
            int appEntry = 0x400000;
            byte[] ServerMapCyclePtr = new byte[4];
            int Pointer2Read = 0;
            ReadProcessMemory((int)_state.Instances[ArrayID].instanceProcessHandle, appEntry + 0x005ED5F8, ServerMapCyclePtr, ServerMapCyclePtr.Length, ref Pointer2Read);
            int MapCycleIndex = BitConverter.ToInt32(ServerMapCyclePtr, 0) + 0xC;
            byte[] mapIndexBytes = new byte[4];
            int mapIndexRead = 0;
            ReadProcessMemory((int)_state.Instances[ArrayID].instanceProcessHandle, MapCycleIndex, mapIndexBytes, mapIndexBytes.Length, ref mapIndexRead);

            _state.Instances[ArrayID].infoCurrentMapIndex = BitConverter.ToInt32(mapIndexBytes, 0);
        }
        public scoreManagement GetCurrentGameScores(ref AppState _state, int ArrayID)
        {
            // Buffer Stuff
            int bytesRead = 0;
            byte[] buffer = new byte[4];

            // This numbers resets based on the type of game being played.
            var baseAddr = 0x400000;
            var startingPtr1 = baseAddr + 0x5DDCC4; // Total Games
            var startingPtr2 = baseAddr + 0x5DDCB4; // Blue Wins
            var startingPtr3 = baseAddr + 0x5DDCB8; // Red Wins
            
            // Grab total games played
            bytesRead = 0;
            buffer = new byte[4];
            ReadProcessMemory((int)_state.Instances[ArrayID].instanceProcessHandle, startingPtr1, buffer, buffer.Length, ref bytesRead);
            int totalGames = BitConverter.ToInt32(buffer, 0);

            // Grab blue player wins
            bytesRead = 0;
            buffer = new byte[4];
            ReadProcessMemory((int)_state.Instances[ArrayID].instanceProcessHandle, startingPtr2, buffer, buffer.Length, ref bytesRead);
            int blueWins = BitConverter.ToInt32(buffer, 0);

            // Grab red player wins
            bytesRead = 0;
            buffer = new byte[4];
            ReadProcessMemory((int)_state.Instances[ArrayID].instanceProcessHandle, startingPtr3, buffer, buffer.Length, ref bytesRead);
            int redWins = BitConverter.ToInt32(buffer, 0);

            return new scoreManagement(totalGames, blueWins, redWins);

        }
        public void ChangeGameScore(ref AppState _state, int ArrayID)
        {
            // This changes the score needed to win on the next map played.
            int nextGameScore = 0;
            var baseAddr = 0x400000;
            var startingPtr1 = 0;
            var startingPtr2 = 0;
            switch (_state.Instances[ArrayID].infoNextMapGameType)
            {
                // KOTH/TKOTH
                case 3:
                case 4:
                    startingPtr1 = baseAddr + 0x5F21B8;
                    startingPtr2 = baseAddr + 0x6344B4;
                    nextGameScore = _state.Instances[ArrayID].gameScoreZoneTime;
                    break;
                // flag ball
                case 8:
                    startingPtr1 = baseAddr + 0x5F21AC;
                    startingPtr2 = baseAddr + 0x6034B8;
                    nextGameScore = _state.Instances[ArrayID].gameScoreFlags;
                    break;
                // all other game types...
                default:
                    startingPtr1 = baseAddr + 0x5F21AC;
                    startingPtr2 = baseAddr + 0x6034B8;
                    nextGameScore = _state.Instances[ArrayID].gameScoreKills;
                    break;
            }
            byte[] nextGameScoreBytes = BitConverter.GetBytes(nextGameScore);
            int nextGameScoreWritten1 = 0;
            int nextGameScoreWritten2 = 0;
            MemoryProcessor.Write(_state.Instances[ArrayID], startingPtr1, nextGameScoreBytes, nextGameScoreBytes.Length, ref nextGameScoreWritten1);
            MemoryProcessor.Write(_state.Instances[ArrayID], startingPtr2, nextGameScoreBytes, nextGameScoreBytes.Length, ref nextGameScoreWritten2);
        }
        public int UpdateMapCycleCounter(ref AppState _state, int InstanceID)
        {
            byte[] currentMapCycleCountBytes = new byte[4];
            int currentMapCycleCountRead = 0;

            int baseAddr = 0x400000;
            int mapCycleCounterPtr = baseAddr + 0x5ED644;

            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, mapCycleCounterPtr, currentMapCycleCountBytes, currentMapCycleCountBytes.Length, ref currentMapCycleCountRead);

            int currentMapCycleCount = BitConverter.ToInt32(currentMapCycleCountBytes, 0);

            return currentMapCycleCount;
        }
        public void ProcessDisarmedPlayers(ref AppState _state, int rowId)
        {
            for (int i = 0; i < _state.Instances[rowId].PlayerListDisarm.Count; i++)
            {
                int playerId = _state.Instances[rowId].PlayerListDisarm[i];
                if (!_state.Instances[rowId].PlayerList.ContainsKey(playerId))
                {
                    _state.Instances[rowId].PlayerListDisarm.Remove(playerId);
                    continue;
                }

                int baseAddress = 0x400000;
                int pointerAddress = baseAddress + 0x005ED600;
                byte[] pointerBuffer = new byte[4];
                int bytesRead = 0;
                ReadProcessMemory((int)_state.Instances[rowId].instanceProcessHandle, pointerAddress, pointerBuffer, pointerBuffer.Length, ref bytesRead);
                int playerlistStartingLocationPointer = BitConverter.ToInt32(pointerBuffer, 0) + 0x28;

                byte[] playerListStartingLocationByteArray = new byte[4];
                ReadProcessMemory((int)_state.Instances[rowId].instanceProcessHandle, playerlistStartingLocationPointer, playerListStartingLocationByteArray, playerListStartingLocationByteArray.Length, ref bytesRead);

                int playerlistStartingLocation = BitConverter.ToInt32(playerListStartingLocationByteArray, 0);
                for (int slot = 1; slot < playerId; slot++)
                {
                    playerlistStartingLocation += 0xAF33C;
                }

                int weaponOffset = 0xADE08;
                byte[] disablePlayerWeapon = BitConverter.GetBytes(0);
                int disablePlayerWeaponWrite = 0;
                MemoryProcessor.Write(_state.Instances[rowId], playerlistStartingLocation + weaponOffset, disablePlayerWeapon, disablePlayerWeapon.Length, ref disablePlayerWeaponWrite);
            }
        }
        public void SetNovaID(ref AppState _state, int rowId)
        {
            if (_state.Instances[rowId].gameRequireNova == true)
            {
                return; // since we are requiring nova login, just return.
            }
            byte[] CurrentAppIDBytes = new byte[4];
            int currentAppIDRead = 0;
            ReadProcessMemory((int)_state.Instances[rowId].instanceProcessHandle, 0x009DDA44, CurrentAppIDBytes, CurrentAppIDBytes.Length, ref currentAppIDRead);
            int CurrentAppID = BitConverter.ToInt32(CurrentAppIDBytes, 0);

            if (CurrentAppID != 0)
            {
                byte[] WriteAppIDBytes = BitConverter.GetBytes((int)0);
                int WriteAppIDWritten = 0;
                MemoryProcessor.Write(_state.Instances[rowId], 0x009DDA44, WriteAppIDBytes, WriteAppIDBytes.Length, ref WriteAppIDWritten);
            }
        }
        public void ChangePlayerTeam(ref AppState _state, int rowId)
        {
            if (_state.Instances[rowId].TeamListChange.Count == 0)
            {
                return;
            }
            else
            {
                int buffer = 0;
                byte[] PointerAddr9 = new byte[4];
                var baseAddr = 0x400000;
                var Pointer = baseAddr + 0x005ED600;

                // read the playerlist memory address from the game...
                ReadProcessMemory((int)_state.Instances[rowId].instanceProcessHandle, (int)Pointer, PointerAddr9, PointerAddr9.Length, ref buffer);
                var playerlistStartingLocationPointer = BitConverter.ToInt32(PointerAddr9, 0) + 0x28;
                byte[] playerListStartingLocationByteArray = new byte[4];
                int playerListStartingLocationBuffer = 0;
                ReadProcessMemory((int)_state.Instances[rowId].instanceProcessHandle, (int)playerlistStartingLocationPointer, playerListStartingLocationByteArray, playerListStartingLocationByteArray.Length, ref playerListStartingLocationBuffer);

                int playerlistStartingLocation = BitConverter.ToInt32(playerListStartingLocationByteArray, 0);

                for (int ii = 0; ii < _state.Instances[rowId].TeamListChange.Count; ii++)
                {
                    int playerLocationOffset = 0;
                    for (int i = 1; i < _state.Instances[rowId].TeamListChange[ii].slotNum; i++)
                    {
                        playerLocationOffset += 0xAF33C;
                    }
                    int playerLocation = playerlistStartingLocation + playerLocationOffset;
                    int playerTeamLocation = playerLocation + 0x90;
                    byte[] teamBytes = BitConverter.GetBytes((int)_state.Instances[rowId].TeamListChange[ii].Team);
                    int bytesWritten = 0;
                    MemoryProcessor.Write(_state.Instances[rowId], playerTeamLocation, teamBytes, teamBytes.Length, ref bytesWritten);
                    _state.Instances[rowId].TeamListChange.RemoveAt(ii);
                }
            }
        }
        public void ResetGodMode(ref AppState _state, int InstanceID)
        {
            if (_state.Instances[InstanceID].PlayerListGodMod.Count == 0)
            {
                return;
            }
            for (int iSlot = 0; iSlot < _state.Instances[InstanceID].PlayerListGodMod.Count; iSlot++)
            {
                int slot = _state.Instances[InstanceID].PlayerListGodMod[iSlot];
                if (!_state.Instances[InstanceID].PlayerList.ContainsKey(slot))
                {
                    _state.Instances[InstanceID].PlayerListGodMod.Remove(slot);
                }
                else
                {
                    int buffer = 0;
                    byte[] PointerAddr9 = new byte[4];
                    var baseAddr = 0x400000;
                    var Pointer = baseAddr + 0x005ED600;

                    // read the playerlist memory address from the game...
                    ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)Pointer, PointerAddr9, PointerAddr9.Length, ref buffer);
                    var playerlistStartingLocationPointer = BitConverter.ToInt32(PointerAddr9, 0) + 0x28;
                    byte[] playerListStartingLocationByteArray = new byte[4];
                    int playerListStartingLocationBuffer = 0;
                    ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)playerlistStartingLocationPointer, playerListStartingLocationByteArray, playerListStartingLocationByteArray.Length, ref playerListStartingLocationBuffer);

                    int playerlistStartingLocation = BitConverter.ToInt32(playerListStartingLocationByteArray, 0);
                    for (int i = 1; i < slot; i++)
                    {
                        playerlistStartingLocation += 0xAF33C;
                    }
                    byte[] playerObjectLocationBytes = new byte[4];
                    int playerObjectLocationRead = 0;
                    ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, playerlistStartingLocation + 0x11C, playerObjectLocationBytes, playerObjectLocationBytes.Length, ref playerObjectLocationRead);
                    int playerObjectLocation = BitConverter.ToInt32(playerObjectLocationBytes, 0);

                    byte[] setPlayerHealth = BitConverter.GetBytes(9999); //set god mode health
                    int setPlayerHealthWrite = 0;

                    byte[] setDamageBy = BitConverter.GetBytes(0);
                    int setDamageByWrite = 0;

                    MemoryProcessor.Write(_state.Instances[InstanceID], playerObjectLocation + 0x138, setDamageBy, setDamageBy.Length, ref setDamageByWrite);
                    MemoryProcessor.Write(_state.Instances[InstanceID], playerObjectLocation + 0xE2, setPlayerHealth, setPlayerHealth.Length, ref setPlayerHealthWrite);
                }
            }
        }
        public string[] GetLastChatMessage(ref AppState _state, int InstanceID)
        {
            var baseAddr = 0x400000;

            var starterPtr = baseAddr + 0x00062D10;
            byte[] ChatLogPtr = new byte[4];
            int ChatLogPtrRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)starterPtr, ChatLogPtr, ChatLogPtr.Length, ref ChatLogPtrRead);

            // get last message sent...
            int ChatLogAddr = BitConverter.ToInt32(ChatLogPtr, 0);

            byte[] Message = new byte[74];
            int MessageRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, ChatLogAddr, Message, Message.Length, ref MessageRead);
            string LastMessage = Encoding.Default.GetString(Message).Replace("\0", "");

            int msgTypeAddr = ChatLogAddr + 0x78;
            byte[] msgType = new byte[4];
            int msgTypeRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, msgTypeAddr, msgType, msgType.Length, ref msgTypeRead);
            string msgTypeBytes = BitConverter.ToString(msgType).Replace("-", "");

            return new string[] { ChatLogAddr.ToString(), LastMessage, msgTypeBytes };
        }
        public void CountDownKiller(ref AppState _state, int InstanceID, int ChatLogAddr)
        {
            byte[] countDownKiller = BitConverter.GetBytes(0);
            int countDownKillerWrite = 0;
            MemoryProcessor.Write(_state.Instances[InstanceID], ChatLogAddr + 0x7C, countDownKiller, countDownKiller.Length, ref countDownKillerWrite);
        }
        public void UpdateGlobalGameType(ref AppState _state, int InstanceID)
        {
            // this function is responsible for adjusting the Pinger Queries to the current game type
            var baseAddr = 0x400000;
            var startingPtr = baseAddr + 0xACE0E8; // pinger query
            byte[] read_pingergametype = new byte[4];
            int read_pingergametypeBytesRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)startingPtr, read_pingergametype, read_pingergametype.Length, ref read_pingergametypeBytesRead);
            int PingerGameType = BitConverter.ToInt32(read_pingergametype, 0);

            // get set gametype
            var CurrentGameTypeAddr = baseAddr + 0x5F21A4;
            byte[] read_currentgametype = new byte[4];
            int read_currentgametypeBytesRead = 0;
            ReadProcessMemory((int)_state.Instances[InstanceID].instanceProcessHandle, (int)CurrentGameTypeAddr, read_currentgametype, read_currentgametype.Length, ref read_currentgametypeBytesRead);
            int CurrentGameType = BitConverter.ToInt32(read_currentgametype, 0);

            // to prevent locking of this address simply look at each address before writing to the address...
            if (PingerGameType != CurrentGameType)
            {
                int UpdatePingerQuery = 0;
                MemoryProcessor.Write(_state.Instances[InstanceID], (int)startingPtr, read_currentgametype, read_currentgametype.Length, ref UpdatePingerQuery);
                return;
            }
            else
            {
                // no update required... Exit the function.
                return;
            }
        }
        public InstanceStatus CheckStatus(ref AppState _state, int instanceid)
        {
            var baseAddr = 0x400000;
            var startingPointer = baseAddr + 0x00098334;
            byte[] startingPointerBuffer = new byte[4];
            int startingPointerReadBytes = 0;
            ReadProcessMemory((int)_state.Instances[instanceid].instanceProcessHandle, (int)startingPointer, startingPointerBuffer, startingPointerBuffer.Length, ref startingPointerReadBytes);


            int statusLocationPointer = BitConverter.ToInt32(startingPointerBuffer, 0);
            byte[] statusLocation = new byte[4];
            int statusLocationReadBytes = 0;
            ReadProcessMemory((int)_state.Instances[instanceid].instanceProcessHandle, (int)statusLocationPointer, statusLocation, statusLocation.Length, ref statusLocationReadBytes);
            int instanceStatus = BitConverter.ToInt32(statusLocation, 0);

            return (InstanceStatus)instanceStatus;
        }
        public bool createAutoRes(Instance startInstance, AppState _state)
        {

            try 
            {
                string autoResPath = Path.Combine(startInstance.profileServerPath, "autores.bin");
                string dfvCFGPath = Path.Combine(startInstance.profileServerPath, "dfv.cfg");

                string text = File.ReadAllText(dfvCFGPath);
                text = text.Replace("// DISPLAY", "[Display]");
                text = text.Replace("// CONTROLS", "[Controls]");
                text = text.Replace("// MULTIPLAYER", "[Multiplayer]");
                text = text.Replace("// MAP", "[infoCurrentMapName]");
                text = text.Replace("// SYSTEM", "[System]");

                var configFileFromString = new ConfigParser(text,
                  new ConfigParserSettings
                  {
                      MultiLineValues = MultiLineValues.Simple | MultiLineValues.AllowValuelessKeys | MultiLineValues.QuoteDelimitedValues
                  });
                // get string vars
                string hw3d_name = configFileFromString.GetValue("Display", "hw3d_name");
                string hw3d_guid = configFileFromString.GetValue("Display", "hw3d_guid");

                // delete existing autores file if it exists
                if (File.Exists(autoResPath))
                {
                    File.Delete(autoResPath);
                }
                MemoryStream ms = new MemoryStream();
                int dedicatedSlots = startInstance.gameMaxSlots + Convert.ToInt32(startInstance.gameDedicated);
                bool loopMaps = true;

                int gamePlayOptionsInt = CalulateGameOptions(startInstance);

                string _miscGraphicSettings = "00 0E 00 00 00 00 00 00 00 01 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 CD CC 4C 3F 06 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 00 10 00 00 00 10 00 00 00 10 00 00 08 00 00 00 01 00 00 00 00 10 00 00 00 00 D0 1E 01 00 00 00 01 00 00 00 01 00 00 00 00 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 00 00 00 00 01 00 00 00 1E 00 00 00 01 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 00 01 00 00 00 02 00 00 00 02 00 00 00 00 00 00 00 03 00 00 00 03 00 00 00 02 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 00 00 00 00 03 00 00 00 02 00 00 00 00 00 00 00 03 00 00 00 03 00 00 00 02 00 00 00 04 00 00 00 01 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 03 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 02 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 FF 00 00 00 C0 00 00 00 C0 00 00 00 C0 00 00 00 C0 00 00 00 02 00 00 00 01 00 00 00";
                string applicationSettings = "01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 00 00 00 00";

                byte[] autoRestart = Encoding.Default.GetBytes("autorestartV0.0");
                byte[] numberOfMapsBytes = BitConverter.GetBytes(128);

                byte[] graphicsSetup_Name = Encoding.Default.GetBytes(hw3d_name);
                byte[] graphicsSetup_GUID = Encoding.Default.GetBytes(hw3d_guid);
                byte[] graphicsSetupMisc_Settings = HexConverter.ToByteArray(_miscGraphicSettings.Replace(" ", ""));
                byte[] applicationSettingBytes = HexConverter.ToByteArray(applicationSettings.Replace(" ", ""));
                byte[] windowedModeBytes = BitConverter.GetBytes(Convert.ToInt32(startInstance.gameWindowedMode));
                byte[] ServerNameBytes = Encoding.Default.GetBytes(startInstance.gameServerName);
                byte[] countryCodeBytes = Encoding.Default.GetBytes(startInstance.gameCountryCode);
                byte[] BindAddress = Encoding.Default.GetBytes(startInstance.profileBindIP);
                byte[] firstMapFile = Encoding.Default.GetBytes(startInstance.MapListCurrent[0].MapFile);
                byte[] maxSlotsBytes = BitConverter.GetBytes(startInstance.gameMaxSlots);
                byte[] dedicatedBytes = BitConverter.GetBytes(Convert.ToInt32(startInstance.gameDedicated));
                byte[] GameScoreBytes = BitConverter.GetBytes(startInstance.gameScoreKills);
                byte[] StartDelayBytes = BitConverter.GetBytes(startInstance.gameStartDelay);
                byte[] serverPasswordBytes = Encoding.Default.GetBytes(startInstance.gamePasswordLobby);
                byte[] redTeamPasswordBytes = Encoding.Default.GetBytes(startInstance.gamePasswordRed);
                byte[] blueTeamPasswordBytes = Encoding.Default.GetBytes(startInstance.gamePasswordBlue);
                byte[] gamePlayOptionsBytes = BitConverter.GetBytes(gamePlayOptionsInt);
                byte[] loopMapsBytes;

                if (loopMaps == true)
                {
                    loopMapsBytes = BitConverter.GetBytes(2);
                }
                else
                {
                    loopMapsBytes = BitConverter.GetBytes(1);
                }

                byte[] gameTypeBytes = BitConverter.GetBytes(_state.autoRes.gameTypes[startInstance.MapListCurrent[0].GameType].DatabaseId);
                byte[] timeLimitBytes = BitConverter.GetBytes(startInstance.gameTimeLimit);
                byte[] respawnTimeBytes = BitConverter.GetBytes(startInstance.gameRespawnTime);
                byte[] allowCustomSkinsBytes = BitConverter.GetBytes(Convert.ToInt32(startInstance.gameCustomSkins));
                byte[] requireNovaLoginBytes = BitConverter.GetBytes(Convert.ToInt32(startInstance.gameRequireNova));
                byte[] MOTDBytes = Encoding.Default.GetBytes(startInstance.gameMOTD);
                byte[] sessionTypeBytes = BitConverter.GetBytes(startInstance.gameSessionType);
                byte[] dedicatedSlotsBytes = BitConverter.GetBytes(dedicatedSlots);
                byte[] graphicsHeaderSettings = BitConverter.GetBytes(-1);
                byte[] graphicsSetting_1 = BitConverter.GetBytes(8);
                byte[] startDelayBytes = BitConverter.GetBytes(startInstance.gameStartDelay);
                byte[] minPingValueBytes = BitConverter.GetBytes(startInstance.gameMinPingValue);
                byte[] enableMinPingBytes = BitConverter.GetBytes(Convert.ToInt32(startInstance.gameMinPing));
                byte[] maxPingValueBytes = BitConverter.GetBytes(startInstance.gameMaxPingValue);
                byte[] enableMaxPingBytes = BitConverter.GetBytes(Convert.ToInt32(startInstance.gameMaxPing));
                byte[] gamePortBytes = BitConverter.GetBytes(startInstance.profileBindPort);
                byte[] flagBallScoreBytes = BitConverter.GetBytes(startInstance.gameScoreFlags);
                byte[] zoneTimerBytes = BitConverter.GetBytes(startInstance.gameScoreZoneTime);
                byte[] customMapFlagBytes = BitConverter.GetBytes(Convert.ToInt32(startInstance.MapListCurrent[0].CustomMap));

                byte[] mapListPrehandle = BitConverter.GetBytes(10621344);
                byte[] finalAppSetup = HexConverter.ToByteArray("00 00 00 00 00 00 00 00 05 00 00 00 00".Replace(" ", ""));
                byte[] resolutionSetup = HexConverter.ToByteArray("02 00 00 00 00 01 00 00 00".Replace(" ", ""));
                byte[] graphicsPrehandle = HexConverter.ToByteArray("02 00 00 00 01 00 00 00".Replace(" ", ""));
                byte[] defaultWeaponSetup = HexConverter.ToByteArray("05 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00 01 00 00 00".Replace(" ", ""));
                byte[] endOfMapCfg = HexConverter.ToByteArray("20 B5 B6 01".Replace(" ", ""));
                byte[] endOfMapCfg2 = HexConverter.ToByteArray("53 01 00 00 00 13 00 00 00 13 00 00 00 04 00 00 00".Replace(" ", ""));


                ms.Seek(0, SeekOrigin.Begin);
                // autorestart header + Number of Total Maps
                ms.Write(autoRestart, 0, autoRestart.Length);
                ms.Write(numberOfMapsBytes, 0, numberOfMapsBytes.Length);

                ms.Seek(0x4D, SeekOrigin.Begin);
                ms.Write(firstMapFile, 0, firstMapFile.Length);

                ms.Seek(0xAF, SeekOrigin.Begin);
                ms.Write(customMapFlagBytes, 0, customMapFlagBytes.Length);

                ms.Seek(0x68F, SeekOrigin.Begin);
                ms.Write(resolutionSetup, 0, resolutionSetup.Length);

                ms.Seek(0x277, SeekOrigin.Begin);
                ms.Write(sessionTypeBytes, 0, sessionTypeBytes.Length);

                ms.Seek(0x1C7, SeekOrigin.Begin);
                ms.Write(applicationSettingBytes, 0, applicationSettingBytes.Length);

                ms.Seek(0x283, SeekOrigin.Begin);
                ms.Write(dedicatedSlotsBytes, 0, dedicatedSlotsBytes.Length);

                ms.Seek(0x28F, SeekOrigin.Begin);
                ms.Write(gameTypeBytes, 0, gameTypeBytes.Length);

                ms.Seek(0x293, SeekOrigin.Begin);
                ms.Write(finalAppSetup, 0, finalAppSetup.Length);

                ms.Seek(0x1347, SeekOrigin.Begin);
                ms.Write(graphicsPrehandle, 0, graphicsPrehandle.Length);

                ms.Seek(0x134F, SeekOrigin.Begin);
                ms.Write(graphicsHeaderSettings, 0, graphicsHeaderSettings.Length);

                ms.Seek(0x1353, SeekOrigin.Begin);
                ms.Write(graphicsSetting_1, 0, graphicsSetting_1.Length);

                ms.Seek(0x1357, SeekOrigin.Begin);
                ms.Write(windowedModeBytes, 0, windowedModeBytes.Length);

                ms.Seek(0x135F, SeekOrigin.Begin);
                ms.Write(graphicsSetup_Name, 0, graphicsSetup_Name.Length);

                ms.Seek(0x137F, SeekOrigin.Begin);
                ms.Write(graphicsSetup_GUID, 0, graphicsSetup_GUID.Length);
                ms.Write(graphicsSetupMisc_Settings, 0, graphicsSetupMisc_Settings.Length);

                ms.Seek(0x152F, SeekOrigin.Begin);
                ms.Write(serverPasswordBytes, 0, serverPasswordBytes.Length);

                ms.Seek(0x1562, SeekOrigin.Begin);
                ms.Write(redTeamPasswordBytes, 0, redTeamPasswordBytes.Length);

                ms.Seek(0x1573, SeekOrigin.Begin);
                ms.Write(blueTeamPasswordBytes, 0, blueTeamPasswordBytes.Length);

                ms.Seek(0x151F, SeekOrigin.Begin);
                ms.Write(gamePlayOptionsBytes, 0, gamePlayOptionsBytes.Length);

                ms.Seek(0x15A6, SeekOrigin.Begin);
                ms.Write(ServerNameBytes, 0, ServerNameBytes.Length);

                ms.Seek(0x15C6, SeekOrigin.Begin);
                ms.Write(countryCodeBytes, 0, countryCodeBytes.Length);

                ms.Seek(0x1613, SeekOrigin.Begin);
                ms.Write(dedicatedBytes, 0, dedicatedBytes.Length);

                ms.Seek(0x15EA, SeekOrigin.Begin);
                ms.Write(BindAddress, 0, BindAddress.Length);

                ms.Seek(0x160B, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(1), 0, BitConverter.GetBytes(1).Length);

                ms.Seek(0x161F, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(100), 0, BitConverter.GetBytes(100).Length);

                ms.Seek(0x162F, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(1), 0, BitConverter.GetBytes(1).Length);

                ms.Seek(0x1633, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(210), 0, BitConverter.GetBytes(210).Length);

                ms.Seek(0x1637, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(1), 0, BitConverter.GetBytes(1).Length);

                ms.Seek(0x163B, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(2), 0, BitConverter.GetBytes(2).Length);

                ms.Seek(0x164B, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(1), 0, BitConverter.GetBytes(1).Length);

                ms.Seek(0x1693, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(1), 0, BitConverter.GetBytes(1).Length);

                ms.Seek(0x1697, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(1), 0, BitConverter.GetBytes(1).Length);

                ms.Seek(0x169B, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(15), 0, BitConverter.GetBytes(15).Length);

                ms.Seek(0x16B7, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(2), 0, BitConverter.GetBytes(2).Length);

                ms.Seek(0x16BB, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(1), 0, BitConverter.GetBytes(1).Length);

                ms.Seek(0x16C7, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(1), 0, BitConverter.GetBytes(1).Length);

                ms.Seek(0x16CB, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(2), 0, BitConverter.GetBytes(2).Length);

                ms.Seek(0x16EF, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(4), 0, BitConverter.GetBytes(4).Length);

                ms.Seek(0x16F4, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(1), 0, BitConverter.GetBytes(1).Length);

                ms.Seek(0x16FC, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(1), 0, BitConverter.GetBytes(1).Length);

                ms.Seek(0x1703, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(1), 0, BitConverter.GetBytes(1).Length);

                ms.Seek(0x1707, SeekOrigin.Begin);
                ms.Write(BitConverter.GetBytes(1), 0, BitConverter.GetBytes(1).Length);

                ms.Seek(0x1627, SeekOrigin.Begin);
                ms.Write(startDelayBytes, 0, startDelayBytes.Length);

                ms.Seek(0x16F3, SeekOrigin.Begin);
                ms.Write(minPingValueBytes, 0, minPingValueBytes.Length);

                ms.Seek(0x16F7, SeekOrigin.Begin);
                ms.Write(enableMinPingBytes, 0, enableMinPingBytes.Length);

                ms.Seek(0x16FB, SeekOrigin.Begin);
                ms.Write(maxPingValueBytes, 0, maxPingValueBytes.Length);

                ms.Seek(0x16FF, SeekOrigin.Begin);
                ms.Write(enableMaxPingBytes, 0, enableMaxPingBytes.Length);

                ms.Seek(0x160F, SeekOrigin.Begin);
                ms.Write(maxSlotsBytes, 0, maxSlotsBytes.Length);

                ms.Seek(0x16CF, SeekOrigin.Begin);
                ms.Write(gamePortBytes, 0, gamePortBytes.Length);

                ms.Seek(0x16DB, SeekOrigin.Begin);
                ms.Write(requireNovaLoginBytes, 0, requireNovaLoginBytes.Length);

                ms.Seek(0x16D7, SeekOrigin.Begin);
                ms.Write(allowCustomSkinsBytes, 0, allowCustomSkinsBytes.Length);

                ms.Seek(0x170B, SeekOrigin.Begin);
                ms.Write(MOTDBytes, 0, MOTDBytes.Length);

                ms.Seek(0x1623, SeekOrigin.Begin);
                ms.Write(flagBallScoreBytes, 0, flagBallScoreBytes.Length);

                ms.Seek(0x1643, SeekOrigin.Begin);
                ms.Write(zoneTimerBytes, 0, zoneTimerBytes.Length);

                ms.Seek(0x1647, SeekOrigin.Begin);
                ms.Write(respawnTimeBytes, 0, respawnTimeBytes.Length);

                ms.Seek(0x163F, SeekOrigin.Begin);
                ms.Write(timeLimitBytes, 0, timeLimitBytes.Length);

                ms.Seek(0x1DA4, SeekOrigin.Begin);
                ms.Write(GameScoreBytes, 0, GameScoreBytes.Length);

                ms.Seek(0x178B, SeekOrigin.Begin);
                ms.Write(defaultWeaponSetup, 0, defaultWeaponSetup.Length);

                ms.Seek(0x187F, SeekOrigin.Begin);
                ms.Write(mapListPrehandle, 0, mapListPrehandle.Length);

                byte[] endOfMap = HexConverter.ToByteArray("20 B5 B6 01 00 00 00 00 53 01 00 00 00 13 00 00 00 13 00 00 00 04 00 00 00".Replace(" ", ""));

                foreach (var map in startInstance.MapListCurrent)
                {
                    byte[] mapFile = Encoding.Default.GetBytes(map.Value.MapFile);
                    ms.Write(mapFile, 0, mapFile.Length);

                    ms.Seek(ms.Position + (0x20F - mapFile.Length), SeekOrigin.Begin);
                    byte[] mapName = Encoding.Default.GetBytes(map.Value.MapName);
                    ms.Write(mapName, 0, mapName.Length);

                    ms.Seek(ms.Position + (0x305 - mapName.Length), SeekOrigin.Begin);
                    ms.Write(endOfMap, 0, endOfMap.Length);

                    ms.Seek(ms.Position + 0x1E3, SeekOrigin.Begin);
                    byte[] customMap = BitConverter.GetBytes(Convert.ToInt32(map.Value.CustomMap));
                    ms.Write(customMap, 0, customMap.Length);

                    // prepare for next entry
                    ms.Seek(ms.Position + 0x1C, SeekOrigin.Begin);
                }

                for (int i = startInstance.MapListCurrent.Count; i < 128; i++)
                {
                    byte[] mapFile = Encoding.Default.GetBytes("NA.bms");
                    ms.Write(mapFile, 0, mapFile.Length);

                    ms.Seek(ms.Position + (0x20F - mapFile.Length), SeekOrigin.Begin);
                    byte[] mapName = Encoding.Default.GetBytes("NA");
                    ms.Write(mapName, 0, mapName.Length);

                    ms.Seek(ms.Position + (0x305 - mapName.Length), SeekOrigin.Begin);
                    ms.Write(endOfMap, 0, endOfMap.Length);

                    ms.Seek(ms.Position + 0x1E3, SeekOrigin.Begin);
                    byte[] customMap = BitConverter.GetBytes(Convert.ToInt32(false));
                    ms.Write(customMap, 0, customMap.Length);

                    // prepare for next entry
                    ms.Seek(ms.Position + 0x1C, SeekOrigin.Begin);
                }

                BinaryWriter writer = new BinaryWriter(File.Open(autoResPath, FileMode.OpenOrCreate, FileAccess.ReadWrite));
                writer.Seek(0, SeekOrigin.Begin);
                writer.Write(ms.ToArray());
                writer.Close();

                Thread.Sleep(1000); // sleep 100ms to allow flushing the file to complete

                return true;

            } 
            catch (Exception e)
            {
                _state.eventLog.WriteEntry("Error creating autores.bin file: " + e.ToString(), EventLogEntryType.Error);
                return false;
            }

        }
    
        public bool startGame(Instance startInstance, AppState _state, int ArrayID, SQLiteConnection conn)
        {
            // Variable to be moved to the function that will need it.
            string file_name = startInstance.profileServerType == 0 ? "dfbhd.exe" : "jops.exe";

            try
            {
                // Start Game
                Process process;
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(startInstance.profileServerPath, file_name),
                    WorkingDirectory = startInstance.profileServerPath,
                    Arguments = "/w /LOADBAR /NOSYSDUMP /serveonly /autorestart",
                    WindowStyle = ProcessWindowStyle.Minimized
                };
                process = Process.Start(startInfo);

                List<int> currentPIDs = new List<int>();
                foreach (var instance in _state.Instances)
                {
                    if (instance.Value.instanceAttachedPID != 0)
                    {
                        currentPIDs.Add(instance.Value.instanceAttachedPID.GetValueOrDefault());
                    }
                }
                Process[] processes = Process.GetProcessesByName("dfbhd");
                foreach (var activeProcess in processes)
                {
                    if (!currentPIDs.Contains(activeProcess.Id) && activeProcess.StartTime > DateTime.Now.AddMinutes(-1))
                    {
                        activeProcess.MaxWorkingSet = new IntPtr(0x7fffffff);
                        startInstance.instanceAttachedPID = activeProcess.Id;
                        _state.ApplicationProcesses[ArrayID] = activeProcess;
                    }
                }

                string pid_update_db = "UPDATE instances_pid SET pid = " + _state.ApplicationProcesses[ArrayID].Id + " WHERE profile_id = " + startInstance.instanceID + ";";
                SQLiteCommand pid_update = new SQLiteCommand(pid_update_db, conn);
                pid_update.ExecuteNonQuery();
                pid_update.Dispose();
            }
            catch (Exception e)
            {
                _state.eventLog.WriteEntry("Error starting game: " + e.ToString(), EventLogEntryType.Error);
                return false;
            }

            // Game Didn't Crash Yay!  Dump Data to the State
            _state.Instances[ArrayID] = startInstance;
            _state.Instances[ArrayID].instanceCrashCounter = 0;

            IntPtr processHandle = OpenProcess(PROCESS_WM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_QUERY_INFORMATION, false, _state.Instances[ArrayID].instanceAttachedPID.GetValueOrDefault());
            _state.Instances[ArrayID].instanceProcessHandle = processHandle;
            var baseAddr = 0x400000;

            Thread.Sleep(1500);

            SetWindowText(_state.ApplicationProcesses[ArrayID].MainWindowHandle, $"{_state.Instances[ArrayID].profileName}");
            if (_state.Instances[ArrayID].gameHostName != "Host")
            {
                int buffer = 0;
                byte[] PointerAddr = new byte[4];
                var Pointer = baseAddr + 0x005ED600;
                ReadProcessMemory((int)processHandle, (int)Pointer, PointerAddr, PointerAddr.Length, ref buffer);
                int buffer2 = 0;
                byte[] Hostname = Encoding.Default.GetBytes(_state.Instances[ArrayID].gameHostName + "\0");
                var Address2HostName = BitConverter.ToInt32(PointerAddr, 0);
                WriteProcessMemory((int)processHandle, (int)Address2HostName + 0x3C, Hostname, Hostname.Length, ref buffer2);
            }

            // map list fix... I hope...
            int MapListMoveGarbageAddress = baseAddr + 0x5EA7B8;
            byte[] CurrentAddressBytes = new byte[4];
            int CurrentAddressRead = 0;
            ReadProcessMemory((int)processHandle, MapListMoveGarbageAddress, CurrentAddressBytes, CurrentAddressBytes.Length, ref CurrentAddressRead);
            int CurrentAddress = BitConverter.ToInt32(CurrentAddressBytes, 0);
            int NewAddress = CurrentAddress + 0x350;

            byte[] NewAddressBytes = BitConverter.GetBytes(NewAddress);
            int NewAddressWritten = 0;
            WriteProcessMemory((int)processHandle, MapListMoveGarbageAddress, NewAddressBytes, NewAddressBytes.Length, ref NewAddressWritten);

            int mapListLocationPtr = baseAddr + 0x005ED5F8;
            byte[] mapListLocationPtrBytes = new byte[4];
            int mapListLocationBytesPtrRead = 0;
            ReadProcessMemory((int)processHandle, mapListLocationPtr, mapListLocationPtrBytes, mapListLocationPtrBytes.Length, ref mapListLocationBytesPtrRead);

            int mapListNumberOfMaps = BitConverter.ToInt32(mapListLocationPtrBytes, 0) + 0x4;
            byte[] numberOfMaps = BitConverter.GetBytes(_state.Instances[ArrayID].MapListCurrent.Count);
            int numberofMapsWritten = 0;
            WriteProcessMemory((int)processHandle, mapListNumberOfMaps, numberOfMaps, numberOfMaps.Length, ref numberofMapsWritten);

            mapListNumberOfMaps += 0x4;
            byte[] TotalnumberOfMaps = BitConverter.GetBytes(_state.Instances[ArrayID].MapListCurrent.Count);
            int TotalnumberofMapsWritten = 0;
            WriteProcessMemory((int)processHandle, mapListNumberOfMaps, TotalnumberOfMaps, TotalnumberOfMaps.Length, ref TotalnumberofMapsWritten);


            UpdateAllowCustomSkins(_state, ArrayID);
            UpdateDestroyBuildings(_state, ArrayID);
            UpdateFatBullets(_state, ArrayID);
            UpdateFlagReturnTime(_state, ArrayID);
            UpdateMaxPing(_state, ArrayID);
            UpdateMaxPingValue(_state, ArrayID);
            UpdateMaxTeamLives(_state, ArrayID);
            UpdateMinPing(_state, ArrayID);
            UpdateMinPingValue(_state, ArrayID);
            UpdateOneShotKills(_state, ArrayID);
            UpdatePSPTakeOverTime(_state, ArrayID);
            UpdateRequireNovaLogin(_state, ArrayID);
            UpdateRespawnTime(_state, ArrayID);
            UpdateWeaponRestrictions(_state, ArrayID);
            _state.Instances[ArrayID].AutoMessages.NextMessage = DateTime.Now.AddMinutes(1.0);

            if (ProgramConfig.EnableWFB)
            {
                // Add Firewall Rules
                _state.Instances[ArrayID].Firewall.AllowTraffic(_state.Instances[ArrayID].profileName, _state.Instances[ArrayID].profileServerPath);
                _state.Instances[ArrayID].Firewall.DenyTraffic(_state.Instances[ArrayID].profileName, _state.Instances[ArrayID].profileServerPath, _state.Instances[ArrayID].PlayerListBans);
            }

            return true;
        }
    }
}
