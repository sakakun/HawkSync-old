using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Web.Http;

namespace HawkSync_SM.Api.v1
{
    public class PlayerManagerController : ApiController
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("user32.dll")]
        static extern bool SetWindowText(IntPtr hWnd, string text);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);

        const int PROCESS_ALL_ACCESS = 0x1F0FFF;
        const int PROCESS_WM_READ = 0x0010;
        const int PROCESS_VM_WRITE = 0x0020;
        const int PROCESS_VM_OPERATION = 0x0008;
        const int PROCESS_QUERY_INFORMATION = 0x0400;

        [HttpGet]
        public HttpResponseMessage CurrentPlayers(int instanceid)
        {
            var Response = Request.CreateResponse(HttpStatusCode.OK);
            Response.Content = new StringContent(JsonConvert.SerializeObject(GlobalAppState.AppState.Instances[instanceid].PlayerList), Encoding.Default, "application/json");
            return Response;
        }
        [HttpGet]
        public HttpResponseMessage CurrentBans(int instanceid)
        {
            var Response = Request.CreateResponse(HttpStatusCode.OK);
            Response.Content = new StringContent(JsonConvert.SerializeObject(GlobalAppState.AppState.Instances[instanceid].BanList), Encoding.Default, "application/json");
            return Response;
        }
        [HttpPost]
        public HttpResponseMessage PermBanPlayer(int instanceid, int slotNum, string banReason = "")
        {
            var Response = Request.CreateResponse(HttpStatusCode.OK);

            GlobalAppState.AppState.Instances[instanceid].BanList.Add(new playerbans
            {
                id = GlobalAppState.AppState.Instances[instanceid].BanList.Count,
                ipaddress = GlobalAppState.AppState.Instances[instanceid].PlayerList[slotNum].address,
                onlykick = false,
                newBan = true,
                player = GlobalAppState.AppState.Instances[instanceid].PlayerList[slotNum].name,
                reason = banReason,
                retry = DateTime.Now,
                lastseen = DateTime.Now,
                expires = "-1"
            });
            Dictionary<string, dynamic> reply = new Dictionary<string, dynamic>
            {
                { "success", true },
                { "Msg", "User has been successfully banned." }
            };

            Response.Content = new StringContent(JsonConvert.SerializeObject(reply), Encoding.Default, "application/json");

            return Response;
        }
        [HttpPost]
        public HttpResponseMessage TempBanPlayer(int instanceid, int slotNum, string expiryDate, string banReason = "")
        {
            var Response = Request.CreateResponse(HttpStatusCode.OK);
            GlobalAppState.AppState.Instances[instanceid].BanList.Add(new playerbans
            {
                id = GlobalAppState.AppState.Instances[instanceid].BanList.Count,
                ipaddress = GlobalAppState.AppState.Instances[instanceid].PlayerList[slotNum].address,
                onlykick = false,
                newBan = true,
                player = GlobalAppState.AppState.Instances[instanceid].PlayerList[slotNum].name,
                reason = banReason,
                retry = DateTime.Now,
                lastseen = DateTime.Now,
                expires = expiryDate
            });
            return Response;
        }
        [HttpPost]
        public HttpResponseMessage KickPlayer(int instanceid, int slotNum, string kickReason = "")
        {
            var Response = Request.CreateResponse(HttpStatusCode.OK);
            GlobalAppState.AppState.Instances[instanceid].BanList.Add(new playerbans
            {
                id = GlobalAppState.AppState.Instances[instanceid].BanList.Count,
                ipaddress = GlobalAppState.AppState.Instances[instanceid].PlayerList[slotNum].address,
                onlykick = true,
                newBan = false,
                player = GlobalAppState.AppState.Instances[instanceid].PlayerList[slotNum].name,
                reason = kickReason,
                retry = DateTime.Now,
                lastseen = DateTime.Now,
                expires = "-1"
            });
            return Response;
        }
        [HttpPost]
        public HttpResponseMessage KillPlayer(int instanceid, int slotNum)
        {
            var Response = Request.CreateResponse(HttpStatusCode.OK);
            Process process = Process.GetProcessById((int)GlobalAppState.AppState.Instances[instanceid].PID.GetValueOrDefault());
            IntPtr processHandle = OpenProcess(PROCESS_WM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_QUERY_INFORMATION, false, process.Id);

            int buffer = 0;
            byte[] PointerAddr9 = new byte[4];
            var baseAddr = 0x400000;
            var Pointer = baseAddr + 0x005ED600;

            // read the playerlist memory address from the game...
            ReadProcessMemory((int)processHandle, (int)Pointer, PointerAddr9, PointerAddr9.Length, ref buffer);
            var playerlistStartingLocationPointer = BitConverter.ToInt32(PointerAddr9, 0) + 0x28;
            byte[] playerListStartingLocationByteArray = new byte[4];
            int playerListStartingLocationBuffer = 0;
            ReadProcessMemory((int)processHandle, (int)playerlistStartingLocationPointer, playerListStartingLocationByteArray, playerListStartingLocationByteArray.Length, ref playerListStartingLocationBuffer);

            int playerlistStartingLocation = BitConverter.ToInt32(playerListStartingLocationByteArray, 0);
            for (int i = 1; i < slotNum; i++)
            {
                playerlistStartingLocation += 0xAF33C;
            }
            byte[] playerObjectLocationBytes = new byte[4];
            int playerObjectLocationRead = 0;
            ReadProcessMemory((int)processHandle, playerlistStartingLocation + 0x11C, playerObjectLocationBytes, playerObjectLocationBytes.Length, ref playerObjectLocationRead);
            int playerObjectLocation = BitConverter.ToInt32(playerObjectLocationBytes, 0);

            byte[] setPlayerHealth = BitConverter.GetBytes(0);
            int setPlayerHealthWrite = 0;

            WriteProcessMemory((int)processHandle, playerObjectLocation + 0x138, setPlayerHealth, setPlayerHealth.Length, ref setPlayerHealthWrite);
            WriteProcessMemory((int)processHandle, playerObjectLocation + 0xE2, setPlayerHealth, setPlayerHealth.Length, ref setPlayerHealthWrite);

            Dictionary<string, dynamic> reply = new Dictionary<string, dynamic>
            {
                { "success", true },
                { "Msg", "Player has been killed successfully." }
            };

            Response.Content = new StringContent(JsonConvert.SerializeObject(reply), Encoding.Default, "application/json");
            process.Dispose();
            return Response;
        }
        [HttpPost]
        public HttpResponseMessage ArmPlayer(int instanceid, int slotNum)
        {
            var Response = Request.CreateResponse(HttpStatusCode.OK);
            Dictionary<string, dynamic> reply = new Dictionary<string, dynamic>();
            Process process = Process.GetProcessById((int)GlobalAppState.AppState.Instances[instanceid].PID.GetValueOrDefault());
            IntPtr processHandle = OpenProcess(PROCESS_WM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_QUERY_INFORMATION, false, process.Id);

            int buffer = 0;
            byte[] PointerAddr9 = new byte[4];
            var baseAddr = 0x400000;
            var Pointer = baseAddr + 0x005ED600;
            ReadProcessMemory((int)processHandle, (int)Pointer, PointerAddr9, PointerAddr9.Length, ref buffer);
            var playerlistStartingLocationPointer = BitConverter.ToInt32(PointerAddr9, 0) + 0x28;

            byte[] playerListStartingLocationByteArray = new byte[4];
            int playerListStartingLocationBuffer = 0;
            ReadProcessMemory((int)processHandle, (int)playerlistStartingLocationPointer, playerListStartingLocationByteArray, playerListStartingLocationByteArray.Length, ref playerListStartingLocationBuffer);

            int playerlistStartingLocation = BitConverter.ToInt32(playerListStartingLocationByteArray, 0);
            for (int slot = 1; slot < slotNum; slot++)
            {
                playerlistStartingLocation += 0xAF33C;
            }
            byte[] disablePlayerWeapon = BitConverter.GetBytes(1);
            int disablePlayerWeaponWrite = 0;
            WriteProcessMemory((int)processHandle, playerlistStartingLocation + 0xADE08, disablePlayerWeapon, disablePlayerWeapon.Length, ref disablePlayerWeaponWrite);
            process.Dispose();

            reply.Add("success", true);
            reply.Add("Msg", "Player has been armed.");

            Response.Content = new StringContent(JsonConvert.SerializeObject(reply), Encoding.Default, "application/json");
            return Response;
        }
        [HttpPost]
        public HttpResponseMessage DisarmPlayer(int instanceid, int slotNum)
        {
            var Response = Request.CreateResponse(HttpStatusCode.OK);
            Dictionary<string, dynamic> reply = new Dictionary<string, dynamic>();
            Process process = Process.GetProcessById((int)GlobalAppState.AppState.Instances[instanceid].PID.GetValueOrDefault());
            IntPtr processHandle = OpenProcess(PROCESS_WM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_QUERY_INFORMATION, false, process.Id);

            int buffer = 0;
            byte[] PointerAddr9 = new byte[4];
            var baseAddr = 0x400000;
            var Pointer = baseAddr + 0x005ED600;
            ReadProcessMemory((int)processHandle, (int)Pointer, PointerAddr9, PointerAddr9.Length, ref buffer);
            var playerlistStartingLocationPointer = BitConverter.ToInt32(PointerAddr9, 0) + 0x28;

            byte[] playerListStartingLocationByteArray = new byte[4];
            int playerListStartingLocationBuffer = 0;
            ReadProcessMemory((int)processHandle, (int)playerlistStartingLocationPointer, playerListStartingLocationByteArray, playerListStartingLocationByteArray.Length, ref playerListStartingLocationBuffer);

            int playerlistStartingLocation = BitConverter.ToInt32(playerListStartingLocationByteArray, 0);
            for (int slot = 1; slot < slotNum; slot++)
            {
                playerlistStartingLocation += 0xAF33C;
            }
            byte[] disablePlayerWeapon = BitConverter.GetBytes(0);
            int disablePlayerWeaponWrite = 0;
            WriteProcessMemory((int)processHandle, playerlistStartingLocation + 0xADE08, disablePlayerWeapon, disablePlayerWeapon.Length, ref disablePlayerWeaponWrite);
            process.Dispose();

            reply.Add("success", true);
            reply.Add("Msg", "Player has been disarmed.");

            Response.Content = new StringContent(JsonConvert.SerializeObject(reply), Encoding.Default, "application/json");
            return Response;
        }
        [HttpPost]
        public HttpResponseMessage WarnPlayer(int instanceid, int slotNum, string WarnMsg = "")
        {
            var Response = Request.CreateResponse(HttpStatusCode.OK);
            GlobalAppState.AppState.Instances[instanceid].WarningQueue.Add(new WarnPlayerClass
            {
                slot = slotNum,
                warningMsg = WarnMsg
            });
            return Response;
        }
        [HttpPost]
        public HttpResponseMessage EnableGodMode(int instanceid, int slotNum)
        {
            Dictionary<string, dynamic> reply = new Dictionary<string, dynamic>();
            var Response = Request.CreateResponse(HttpStatusCode.OK);
            // check if user is already in GodMode...
            if (!GlobalAppState.AppState.Instances[instanceid].GodModeList.Contains(slotNum))
            {
                GlobalAppState.AppState.Instances[instanceid].GodModeList.Add(slotNum);
                reply.Add("success", true);
                reply.Add("Msg", "User has been placed in God Mode.");
            }
            else
            {
                reply.Add("success", false);
                reply.Add("Msg", "The selected Player is already in God Mode.");
            }

            Response.Content = new StringContent(JsonConvert.SerializeObject(reply), Encoding.Default, "application/json");

            return Response;
        }
        [HttpPost]
        public HttpResponseMessage DisableGodMode(int instanceid, int slotNum)
        {
            Dictionary<string, dynamic> reply = new Dictionary<string, dynamic>();
            var Response = Request.CreateResponse(HttpStatusCode.OK);
            if (GlobalAppState.AppState.Instances[instanceid].GodModeList.Contains(slotNum))
            {
                GlobalAppState.AppState.Instances[instanceid].GodModeList.Remove(slotNum);
                Process process = Process.GetProcessById((int)GlobalAppState.AppState.Instances[instanceid].PID.GetValueOrDefault());
                IntPtr processHandle = OpenProcess(PROCESS_WM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_QUERY_INFORMATION, false, process.Id);

                int buffer = 0;
                byte[] PointerAddr9 = new byte[4];
                var baseAddr = 0x400000;
                var Pointer = baseAddr + 0x005ED600;

                // read the playerlist memory address from the game...
                ReadProcessMemory((int)processHandle, (int)Pointer, PointerAddr9, PointerAddr9.Length, ref buffer);
                var playerlistStartingLocationPointer = BitConverter.ToInt32(PointerAddr9, 0) + 0x28;
                byte[] playerListStartingLocationByteArray = new byte[4];
                int playerListStartingLocationBuffer = 0;
                ReadProcessMemory((int)processHandle, (int)playerlistStartingLocationPointer, playerListStartingLocationByteArray, playerListStartingLocationByteArray.Length, ref playerListStartingLocationBuffer);

                int playerlistStartingLocation = BitConverter.ToInt32(playerListStartingLocationByteArray, 0);
                for (int i = 1; i < slotNum; i++)
                {
                    playerlistStartingLocation += 0xAF33C;
                }
                byte[] playerObjectLocationBytes = new byte[4];
                int playerObjectLocationRead = 0;
                ReadProcessMemory((int)processHandle, playerlistStartingLocation + 0x11C, playerObjectLocationBytes, playerObjectLocationBytes.Length, ref playerObjectLocationRead);
                int playerObjectLocation = BitConverter.ToInt32(playerObjectLocationBytes, 0);

                byte[] setPlayerHealth = BitConverter.GetBytes(100); //set god mode health
                int setPlayerHealthWrite = 0;

                byte[] setDamageBy = BitConverter.GetBytes(0);
                int setDamageByWrite = 0;

                WriteProcessMemory((int)processHandle, playerObjectLocation + 0x138, setDamageBy, setDamageBy.Length, ref setDamageByWrite);
                WriteProcessMemory((int)processHandle, playerObjectLocation + 0xE2, setPlayerHealth, setPlayerHealth.Length, ref setPlayerHealthWrite);
                process.Dispose();

                reply.Add("success", true);
                reply.Add("Msg", "Player has been removed from God Mode.");

            }
            else
            {
                reply.Add("success", false);
                reply.Add("Msg", "The selected Player is not in God Mode.");
            }

            Response.Content = new StringContent(JsonConvert.SerializeObject(reply), Encoding.Default, "application/json");

            return Response;
        }
        [HttpPost]
        public HttpResponseMessage ChangeTeam(int instanceid, int slotNum)
        {
            var Response = Request.CreateResponse(HttpStatusCode.OK);
            //playerlist.Teams currentTeam = (playerlist.Teams)Enum.Parse(typeof(playerlist.Teams), GlobalAppState.AppState.Instances[instanceid].PlayerList[slotNum].team);
            //playerlist.Teams switchTeam = playerlist.Teams.TEAM_NONE;

            /*if (currentTeam == playerlist.Teams.TEAM_BLUE)
            {
                switchTeam = playerlist.Teams.TEAM_RED;
            }
            else if (currentTeam == playerlist.Teams.TEAM_RED)
            {
                switchTeam = playerlist.Teams.TEAM_BLUE;
            }

            GlobalAppState.AppState.Instances[instanceid].ChangeTeamList.Add(new ChangeTeamClass
            {
                slotNum = slotNum,
                Team = switchTeam
            });*/
            return Response;
        }
    }
}
