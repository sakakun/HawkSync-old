using HawkSync_SM.classes.StatManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.Runtime.InteropServices;

namespace HawkSync_SM
{
    //
    public class PostGameProcess : ProcessHandler
    {
        // Various Hexadecimal values for Windows/Game Console
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
        List<ob_PlayerChatLog> _chatLogs;
        int ArrayID = 0;

        public PostGameProcess(AppState state, int instanceid, List<ob_PlayerChatLog> chatlogs)
        {
            ArrayID = instanceid;

            _connection = new SQLiteConnection(ProgramConfig.DBConfig);
            _state = state;
            _chatLogs = chatlogs;
            _connection.Open();

        }
        public void Run()
        {
            InsertChatLogs();
            ResetPlayers();
            _connection.Close();
        }

        private void ResetPlayers()
        {
            // Clear the Player Stats
            _state.Instances[ArrayID].playerStats.Clear();
        }

        private void InsertChatLogs()
        {
            foreach (var message in _chatLogs)
            {
                var query = @"INSERT INTO `chatlog` (`id`, `profile_id`, `name`, `msg`, `team`, `datesent`)" +
                            " VALUES (NULL, @ProfileId, @Name, @Msg, @Team, @DateSent);";
                var command = new SQLiteCommand(query, _connection);
                var chatMessage = message;
                command.Parameters.AddWithValue("@ProfileId", _state.Instances[ArrayID].Id);
                command.Parameters.AddWithValue("@Name", chatMessage.PlayerName);
                command.Parameters.AddWithValue("@Msg", chatMessage.msg);
                command.Parameters.AddWithValue("@Team", chatMessage.team);
                command.Parameters.AddWithValue("@DateSent", chatMessage.dateSent);
                command.ExecuteNonQuery();
            }
        }

    }
}
