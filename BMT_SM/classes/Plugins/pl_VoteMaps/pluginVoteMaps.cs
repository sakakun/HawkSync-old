using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HawkSync_SM.classes.Plugins.pl_VoteMaps
{
    internal class pluginVoteMaps
    {
        /*
           private void pl_VoteSkipMap(int profileid, string PlayerMessage, string msgTypeString, string PlayerName)
           {
               var baseAddr = 0x400000;
               var starterPtr = baseAddr + 0x00062D10;
               byte[] ChatLogPtr = new byte[4];
               int ChatLogPtrRead = 0;
               ReadProcessMemory((int)_state.Instances[profileid].ProcessHandle, (int)starterPtr, ChatLogPtr, ChatLogPtr.Length, ref ChatLogPtrRead);

               // get last message sent...
               int ChatLogAddr = BitConverter.ToInt32(ChatLogPtr, 0);
               if (_state.Instances[profileid].Plugins.VoteMaps == true)
               {
                   if (_state.Instances[profileid].PlayerList.Count < _state.Instances[profileid].Plugins.VoteMapSettings.MinPlayers)
                   {
                       //if (_state.Instances[profileid].VoteMapStandBy == true)
                       //{
                       //    return;
                       //}

                       if (PlayerMessage == "!skip")
                       {
                           int colorbuffer_written = 0;
                           byte[] colorcode = HexConverter.ToByteArray("6A 03".Replace(" ", ""));
                           MemoryProcessor.Write(_state.Instances[profileid], 0x00462ABA, colorcode, colorcode.Length, ref colorbuffer_written);
                           Thread.Sleep(100);
                           // open cmdConsole
                           PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, GlobalChat, 0);
                           PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, GlobalChat, 0);
                           Thread.Sleep(100);
                           int bytesWritten = 0;
                           byte[] buffer = Encoding.Default.GetBytes($"Not enough players to cast a vote! Min Required: {_state.Instances[profileid].Plugins.VoteMapSettings.MinPlayers}\0"); // '\0' marks the end of string
                           MemoryProcessor.Write(_state.Instances[profileid], 0x00879A14, buffer, buffer.Length, ref bytesWritten);
                           Thread.Sleep(100);
                           PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, VK_ENTER, 0);
                           PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, VK_ENTER, 0);

                           Thread.Sleep(100);
                           int revert_colorbuffer = 0;
                           byte[] revert_colorcode = HexConverter.ToByteArray("6A 01".Replace(" ", ""));
                           MemoryProcessor.Write(_state.Instances[profileid], 0x00462ABA, revert_colorcode, revert_colorcode.Length, ref revert_colorbuffer);
                           _state.Instances[profileid].VoteMapStandBy = true;
                       }
                       return;
                   }
                   else if (_state.Instances[profileid].Status == InstanceStatus.SCORING)
                   {
                       if (_state.Instances[profileid].VoteMapStandBy == true)
                       {
                           return;
                       }
                       int colorbuffer_written = 0;
                       byte[] colorcode = HexConverter.ToByteArray("6A 03".Replace(" ", ""));
                       MemoryProcessor.Write(_state.Instances[profileid], 0x00462ABA, colorcode, colorcode.Length, ref colorbuffer_written);
                       Thread.Sleep(100);
                       // open cmdConsole
                       PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, GlobalChat, 0);
                       PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, GlobalChat, 0);
                       Thread.Sleep(100);
                       int bytesWritten = 0;
                       byte[] buffer = Encoding.Default.GetBytes($"Cannot cast vote at this time!\0"); // '\0' marks the end of string
                       MemoryProcessor.Write(_state.Instances[profileid], 0x00879A14, buffer, buffer.Length, ref bytesWritten);
                       Thread.Sleep(100);
                       PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, VK_ENTER, 0);
                       PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, VK_ENTER, 0);

                       Thread.Sleep(100);
                       int revert_colorbuffer = 0;
                       byte[] revert_colorcode = HexConverter.ToByteArray("6A 01".Replace(" ", ""));
                       MemoryProcessor.Write(_state.Instances[profileid], 0x00462ABA, revert_colorcode, revert_colorcode.Length, ref revert_colorbuffer);
                       _state.Instances[profileid].VoteMapStandBy = true;
                       return;
                   }
                   else
                   {
                       _state.Instances[profileid].VoteMapStandBy = false;
                   }
                   // HOOK: VoteMap
                   if (PlayerMessage == "!skip" && msgTypeString != "Server" && PlayerName != _state.Instances[profileid].HostName)
                   {
                       bool found = false;
                       foreach (var item in _state.Instances[profileid].VoteMapsTally)
                       {
                           if (item.PlayerName == PlayerName)
                           {
                               found = true;
                               break;
                           }
                       }
                       if (found == false && _state.Instances[profileid].VoteMapsTally.Count == 0)
                       {

                           int colorbuffer_written = 0;
                           byte[] colorcode = HexConverter.ToByteArray("6A 03".Replace(" ", ""));
                           MemoryProcessor.Write(_state.Instances[profileid], 0x00462ABA, colorcode, colorcode.Length, ref colorbuffer_written);
                           Thread.Sleep(100);

                           // open cmdConsole
                           PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, GlobalChat, 0);
                           PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, GlobalChat, 0);
                           Thread.Sleep(100);
                           int bytesWritten = 0;
                           byte[] buffer = Encoding.Default.GetBytes($"{PlayerName} has initiated a SKIP MAP vote!\0"); // '\0' marks the end of string
                           MemoryProcessor.Write(_state.Instances[profileid], 0x00879A14, buffer, buffer.Length, ref bytesWritten);
                           Thread.Sleep(100);
                           PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, VK_ENTER, 0);
                           PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, VK_ENTER, 0);
                           Thread.Sleep(3000);
                           PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, GlobalChat, 0);
                           PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, GlobalChat, 0);
                           Thread.Sleep(100);
                           int bytesWritten1 = 0;
                           byte[] buffer1 = Encoding.Default.GetBytes($"Cast your vote by typing !skip\0"); // '\0' marks the end of string
                           MemoryProcessor.Write(_state.Instances[profileid], 0x00879A14, buffer1, buffer1.Length, ref bytesWritten1);
                           Thread.Sleep(100);
                           PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, VK_ENTER, 0);
                           PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, VK_ENTER, 0);

                           var starterPtr1 = baseAddr + 0x00062D10;
                           byte[] ChatLogPtr1 = new byte[4];
                           int ChatLogPtrRead1 = 0;
                           ReadProcessMemory((int)_state.Instances[profileid].ProcessHandle, (int)starterPtr, ChatLogPtr, ChatLogPtr.Length, ref ChatLogPtrRead1);

                           int ChatLogAddr1 = BitConverter.ToInt32(ChatLogPtr, 0);
                           byte[] countDownKiller = BitConverter.GetBytes(0);
                           int countDownKillerWrite = 0;
                           MemoryProcessor.Write(_state.Instances[profileid], ChatLogAddr1 + 0x7C, countDownKiller, countDownKiller.Length, ref countDownKillerWrite);

                           _state.Instances[profileid].VoteMapsTally.Add(new VoteMapsTally
                           {
                               Slot = 0,
                               PlayerName = PlayerName,
                               Vote = VoteMapsTally.VoteStatus.VOTE_YES
                           });
                           _state.Instances[profileid].VoteMapTimer = new Timer
                           {
                               Enabled = true,
                               Interval = 1
                           };
                           int timer = 3000; // 2 minutes
                           _state.Instances[profileid].VoteMapTimer.Tick += (s, e) =>
                           {
                               if (timer != 0)
                               {
                                   timer--;
                                   return;
                               }
                               if (_state.Instances[profileid].VoteMapsTally.Count > (_state.Instances[profileid].PlayerList.Count / 2))
                               {
                                   int colorbuffer_written2 = 0;
                                   byte[] colorcode2 = HexConverter.ToByteArray("6A 03".Replace(" ", ""));
                                   MemoryProcessor.Write(_state.Instances[profileid], 0x00462ABA, colorcode2, colorcode2.Length, ref colorbuffer_written2);
                                   Thread.Sleep(100);
                                   // open cmdConsole
                                   PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, GlobalChat, 0);
                                   PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, GlobalChat, 0);
                                   Thread.Sleep(100);
                                   int bytesWritten2 = 0;
                                   byte[] buffer2 = Encoding.Default.GetBytes($"Vote Success! - Skipping Map...\0"); // '\0' marks the end of string
                                   MemoryProcessor.Write(_state.Instances[profileid], 0x00879A14, buffer2, buffer2.Length, ref bytesWritten2);
                                   Thread.Sleep(100);
                                   PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, VK_ENTER, 0);
                                   PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, VK_ENTER, 0);

                                   Thread.Sleep(100);
                                   int revert_colorbuffer = 0;
                                   byte[] revert_colorcode = HexConverter.ToByteArray("6A 01".Replace(" ", ""));
                                   MemoryProcessor.Write(_state.Instances[profileid], 0x00462ABA, revert_colorcode, revert_colorcode.Length, ref revert_colorbuffer);
                                   Thread.Sleep(3000);


                                   // open cmdConsole
                                   PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, console, 0);
                                   PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, console, 0);
                                   Thread.Sleep(50);
                                   int bytesWritten3 = 0;
                                   byte[] buffer3 = Encoding.Default.GetBytes("resetgames\0"); // '\0' marks the end of string
                                   MemoryProcessor.Write(_state.Instances[profileid], 0x00879A14, buffer3, buffer3.Length, ref bytesWritten3);
                                   Thread.Sleep(50);
                                   PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, VK_ENTER, 0);
                                   PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, VK_ENTER, 0);
                                   _state.Instances[profileid].VoteMapsTally.Clear();
                                   _state.Instances[profileid].VoteMapStandBy = true;
                                   _state.Instances[profileid].VoteMapTimer.Stop();
                               }
                               else
                               {
                                   // change color
                                   int colorbuffer_written2 = 0;
                                   byte[] colorcode2 = HexConverter.ToByteArray("6A 03".Replace(" ", ""));
                                   MemoryProcessor.Write(_state.Instances[profileid], 0x00462ABA, colorcode2, colorcode2.Length, ref colorbuffer_written2);
                                   Thread.Sleep(100);


                                   // do not skip map
                                   PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, GlobalChat, 0);
                                   PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, GlobalChat, 0);
                                   Thread.Sleep(100);
                                   int bytesWritten_nope = 0;
                                   byte[] buffer_nope = Encoding.Default.GetBytes($"Not enough votes to skip map.\0"); // '\0' marks the end of string
                                   MemoryProcessor.Write(_state.Instances[profileid], 0x00879A14, buffer_nope, buffer_nope.Length, ref bytesWritten_nope);
                                   Thread.Sleep(100);
                                   PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, VK_ENTER, 0);
                                   PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, VK_ENTER, 0);
                                   Thread.Sleep(100);

                                   int revert_colorbuffer = 0;
                                   byte[] revert_colorcode = HexConverter.ToByteArray("6A 01".Replace(" ", ""));
                                   MemoryProcessor.Write(_state.Instances[profileid], 0x00462ABA, revert_colorcode, revert_colorcode.Length, ref revert_colorbuffer);
                                   _state.Instances[profileid].VoteMapStandBy = true;
                                   _state.Instances[profileid].VoteMapsTally.Clear();
                                   _state.Instances[profileid].VoteMapTimer.Stop();
                               }
                           };
                           _state.Instances[profileid].VoteMapTimer.Start();
                       }
                       else if (found == false && _state.Instances[profileid].VoteMapsTally.Count > 0)
                       {
                           int colorbuffer_written = 0;
                           byte[] colorcode = HexConverter.ToByteArray("6A 03".Replace(" ", ""));
                           MemoryProcessor.Write(_state.Instances[profileid], 0x00462ABA, colorcode, colorcode.Length, ref colorbuffer_written);
                           Thread.Sleep(100);
                           // open cmdConsole
                           PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, GlobalChat, 0);
                           PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, GlobalChat, 0);
                           Thread.Sleep(100);
                           int bytesWritten = 0;
                           byte[] buffer = Encoding.Default.GetBytes($"{PlayerName} - vote casted!\0"); // '\0' marks the end of string
                           MemoryProcessor.Write(_state.Instances[profileid], 0x00879A14, buffer, buffer.Length, ref bytesWritten);
                           Thread.Sleep(100);
                           PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYDOWN, VK_ENTER, 0);
                           PostMessage(_state.ApplicationProcesses[profileid].MainWindowHandle, WM_KEYUP, VK_ENTER, 0);
                           Thread.Sleep(100);
                           int revert_colorbuffer = 0;
                           byte[] revert_colorcode = HexConverter.ToByteArray("6A 01".Replace(" ", ""));
                           MemoryProcessor.Write(_state.Instances[profileid], 0x00462ABA, revert_colorcode, revert_colorcode.Length, ref revert_colorbuffer);
                           _state.Instances[profileid].VoteMapsTally.Add(new VoteMapsTally
                           {
                               Slot = 0,
                               PlayerName = PlayerName,
                               Vote = VoteMapsTally.VoteStatus.VOTE_YES
                           });
                       }
                   }
               }
           }
        */

    }

}
