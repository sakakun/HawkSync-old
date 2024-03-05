using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using HawkSync_SM.classes.StatManagement;

namespace HawkSync_SM
{
    class ScoringGameProcess : ProcessHandler
    {
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

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("user32.dll")]
        static extern int SetWindowText(IntPtr hWnd, string text);

        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);

        [DllImport("kernel32", SetLastError = true)]
        static extern bool CloseHandle(IntPtr handle);

        SQLiteConnection _connection;

        AppState _state;
        int ArrayID = -1;

        public ScoringGameProcess(AppState state, int InstanceID)
        {
            _connection = new SQLiteConnection(ProgramConfig.DBConfig);
            _state = state;
            ArrayID = InstanceID;
            _connection.Open();
        }

        public void Run()
        {
            SendStats();
            IncrementMapCounter();
            ChangeGameType();
            OverrideScoreBoard();
            _connection.Close();
        }

        private void SendStats()
        {
            statsBabstats sendBabstats = new statsBabstats();
            sendBabstats.sendBabstatsImportData(_state, ArrayID);
        }

        private void UpdateGameScores()
        {
            var baseAddr = 0x400000;
            if (_state.Instances[ArrayID].nextMapGameType == 0 || _state.Instances[ArrayID].nextMapGameType == 1 || _state.Instances[ArrayID].nextMapGameType == 8)
            {
                var gameScore1 = baseAddr + 0x5F21AC;
                var gameScore2 = baseAddr + 0x6034B8;
                int gameScore = 0;
                switch (_state.Instances[ArrayID].nextMapGameType)
                {
                    case 0:
                    case 1:
                        gameScore = _state.Instances[ArrayID].GameScore; // DM + TDM
                        break;
                    case 8:
                        gameScore = _state.Instances[ArrayID].FBScore; // Flagball
                        break;
                    default:
                        gameScore = 19; // set default just in case if NOTHING gets set from the instance.
                        break;
                }
                byte[] gameScoreBytes = BitConverter.GetBytes(gameScore);
                int gameScore1Written = 0;
                int gameScore2Written = 0;
                WriteProcessMemory((int)_state.Instances[ArrayID].ProcessHandle, gameScore1, gameScoreBytes, gameScoreBytes.Length, ref gameScore1Written);
                WriteProcessMemory((int)_state.Instances[ArrayID].ProcessHandle, gameScore2, gameScoreBytes, gameScoreBytes.Length, ref gameScore2Written);
            }
            else if (_state.Instances[ArrayID].nextMapGameType == 3 || _state.Instances[ArrayID].nextMapGameType == 4)
            {
                int kothScore = _state.Instances[ArrayID].KOTHScore; // KOTH + TKOTH
                byte[] gameScoreBytes = BitConverter.GetBytes(kothScore);
                var kothScore1 = baseAddr + 0x5F21B8;
                var kothScore2 = baseAddr + 0x6344B4;
                int gameScore1Written = 0;
                int gameScore2Written = 0;
                WriteProcessMemory((int)_state.Instances[ArrayID].ProcessHandle, kothScore1, gameScoreBytes, gameScoreBytes.Length, ref gameScore1Written);
                WriteProcessMemory((int)_state.Instances[ArrayID].ProcessHandle, kothScore2, gameScoreBytes, gameScoreBytes.Length, ref gameScore2Written);
            }
        }

        private void OverrideScoreBoard()
        {
            _state.Instances[ArrayID].ScoreboardTimer = new Timer();
            _state.Instances[ArrayID].ScoreboardTimer.Tick += ScoreboardTimer_Tick;
            _state.Instances[ArrayID].ScoreboardTimer.Interval = _state.Instances[ArrayID].ScoreBoardDelay * 1000;
            _state.Instances[ArrayID].ScoreboardTimer.Enabled = true;
            _state.Instances[ArrayID].ScoreboardTimer.Start();
        }

        private void ScoreboardTimer_Tick(object sender, EventArgs e)
        {
            var baseAddr = 0x400000;
            var instanceTimer = baseAddr + 0x5DAE00;
            byte[] endTimerBytes = BitConverter.GetBytes(1);
            int bytesWritten = 0;
            WriteProcessMemory((int)_state.Instances[ArrayID].ProcessHandle, instanceTimer, endTimerBytes, endTimerBytes.Length, ref bytesWritten);
            _state.Instances[ArrayID].ScoreboardTimer.Stop();
        }

        private void IncrementMapCounter()
        {
            _state.Instances[ArrayID].mapCounter++;
        }

        private void ChangeGameType()
        {
            IntPtr processHandle = OpenProcess(PROCESS_WM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_QUERY_INFORMATION, false, _state.Instances[ArrayID].PID.GetValueOrDefault());
            var baseAddr = 0x400000;
            var startingPtr = baseAddr + 0x005ED5F8;

            byte[] read_Ptr2Bytes = new byte[4];
            int read_Ptr2BytesRead = 0;
            ReadProcessMemory((int)processHandle, (int)startingPtr, read_Ptr2Bytes, read_Ptr2Bytes.Length, ref read_Ptr2BytesRead);
            int Ptr2 = BitConverter.ToInt32(read_Ptr2Bytes, 0) + 0xC;

            byte[] CurrentMapIndexBytes = new byte[4];
            int CurrentMapIndexBytesRead = 0;
            ReadProcessMemory((int)processHandle, Ptr2, CurrentMapIndexBytes, CurrentMapIndexBytes.Length, ref CurrentMapIndexBytesRead);
            int currentMapIndex = BitConverter.ToInt32(CurrentMapIndexBytes, 0);
            if (currentMapIndex + 1 >= _state.Instances[ArrayID].MapList.Count || !_state.Instances[ArrayID].MapList.ContainsKey(currentMapIndex + 1))
            {
                currentMapIndex = 0;
            }
            else
            {
                currentMapIndex++;
            }
            try
            {
                int nextMapType = _state.autoRes.gameTypes[_state.Instances[ArrayID].MapList[currentMapIndex].GameType].DatabaseId;

                bool isNextMapTeamMap = _state.autoRes.IsMapTeamBased(nextMapType);
                bool isCurrentMapTeamMap = _state.autoRes.IsMapTeamBased(_state.Instances[ArrayID].gameMapType);

                // TDM -> DM
                if (isNextMapTeamMap == false && isCurrentMapTeamMap == true)
                {
                    foreach (var playerObj in _state.Instances[ArrayID].PlayerList)
                    {
                        _state.Instances[ArrayID].previousTeams.Add(new ob_playerPreviousTeam
                        {
                            slotNum = playerObj.Value.slot,
                            Team = (ob_playerList.Teams)playerObj.Value.team
                        });

                        _state.Instances[ArrayID].ChangeTeamList.Add(new ob_playerChangeTeamList
                        {
                            slotNum = playerObj.Value.slot,
                            Team = playerObj.Value.slot + 5
                        });
                    }
                }
                // DM -> TDM
                else if (isNextMapTeamMap == true && isCurrentMapTeamMap == false)
                {
                    foreach (var playerObj in _state.Instances[ArrayID].previousTeams)
                    {
                        if (_state.Instances[ArrayID].PlayerList.ContainsKey(playerObj.slotNum))
                        {
                            _state.Instances[ArrayID].ChangeTeamList.Add(new ob_playerChangeTeamList
                            {
                                slotNum = playerObj.slotNum,
                                Team = (int)playerObj.Team
                            });
                        }
                    }
                    foreach (var player in _state.Instances[ArrayID].PlayerList)
                    {
                        bool found = false;
                        foreach (var previousPlayer in _state.Instances[ArrayID].previousTeams)
                        {
                            if (player.Value.slot == previousPlayer.slotNum)
                            {
                                found = true;
                                break;
                            }
                        }
                        if (found == false)
                        {
                            int blueteam = 0;
                            int redteam = 0;
                            foreach (var playerTeam in _state.Instances[ArrayID].previousTeams)
                            {
                                if (playerTeam.Team == ob_playerList.Teams.TEAM_BLUE)
                                {
                                    blueteam++;
                                }
                                else if (playerTeam.Team == ob_playerList.Teams.TEAM_RED)
                                {
                                    redteam++;
                                }
                            }
                            if (blueteam > redteam)
                            {
                                _state.Instances[ArrayID].ChangeTeamList.Add(new ob_playerChangeTeamList
                                {
                                    slotNum = player.Value.slot,
                                    Team = (int)ob_playerList.Teams.TEAM_RED
                                });
                                _state.Instances[ArrayID].previousTeams.Add(new ob_playerPreviousTeam
                                {
                                    slotNum = player.Value.slot,
                                    Team = ob_playerList.Teams.TEAM_RED
                                });
                            }
                            else if (blueteam < redteam)
                            {
                                _state.Instances[ArrayID].ChangeTeamList.Add(new ob_playerChangeTeamList
                                {
                                    slotNum = player.Value.slot,
                                    Team = (int)ob_playerList.Teams.TEAM_BLUE
                                });
                                _state.Instances[ArrayID].previousTeams.Add(new ob_playerPreviousTeam
                                {
                                    slotNum = player.Value.slot,
                                    Team = ob_playerList.Teams.TEAM_BLUE
                                });
                            }
                            else if (blueteam == redteam)
                            {
                                Random rand = new Random();
                                int rnd = rand.Next(1, 2);
                                if (rnd == 1)
                                {
                                    _state.Instances[ArrayID].ChangeTeamList.Add(new ob_playerChangeTeamList
                                    {
                                        slotNum = player.Value.slot,
                                        Team = (int)ob_playerList.Teams.TEAM_BLUE
                                    });
                                    _state.Instances[ArrayID].previousTeams.Add(new ob_playerPreviousTeam
                                    {
                                        slotNum = player.Value.slot,
                                        Team = ob_playerList.Teams.TEAM_BLUE
                                    });
                                }
                                else if (rnd == 2)
                                {
                                    _state.Instances[ArrayID].ChangeTeamList.Add(new ob_playerChangeTeamList
                                    {
                                        slotNum = player.Value.slot,
                                        Team = (int)ob_playerList.Teams.TEAM_RED
                                    });
                                    _state.Instances[ArrayID].previousTeams.Add(new ob_playerPreviousTeam
                                    {
                                        slotNum = player.Value.slot,
                                        Team = ob_playerList.Teams.TEAM_RED
                                    });
                                }
                            }
                        }
                    }
                    _state.Instances[ArrayID].previousTeams.Clear();
                }
                var CurrentGameTypeAddr = baseAddr + 0x5F21A4;
                byte[] nextMaptypeBytes = BitConverter.GetBytes(nextMapType);
                int nextMaptypeBytesWrite = 0;
                WriteProcessMemory((int)processHandle, CurrentGameTypeAddr, nextMaptypeBytes, nextMaptypeBytes.Length, ref nextMaptypeBytesWrite);
                CloseHandle(processHandle);
                _state.Instances[ArrayID].nextMapGameType = nextMapType;
            }
            catch (Exception ex)
            {
                _state.eventLog.WriteEntry("Something went wrong with ScoringProcessHandler: " + ex + "\nInstance: " + ArrayID + "\n\nCurrent MapIndex: " + currentMapIndex + "\nCurrent GameType: " + _state.Instances[ArrayID].MapList[currentMapIndex].GameType + "\n\nSend this to Staff Members: " + Crypt.Base64Encode(JsonConvert.SerializeObject(_state.Instances[ArrayID].MapList)), System.Diagnostics.EventLogEntryType.Error);
                throw new Exception("Something went wrong with ScoringProcessHandler: " + ex + " Instance: " + ArrayID);
            }
        }
    }
}
