using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace HawkSync_SM.RCClasses
{
    public static class SocketClient
    {
        public static byte[] GetData(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] collectionOfBytes = new byte[65535];
            int count = 0;
            do
            {
                count += stream.Read(collectionOfBytes, count, collectionOfBytes.Length);
            }
            while (stream.DataAvailable);
            return collectionOfBytes;
        }
        public static void SendData(TcpClient client, byte[] buffer, int offset, int size)
        {
            MemoryStream ms = new MemoryStream();
            byte[] numOfBytes = BitConverter.GetBytes(buffer.Length + 5);
            ms.Write(numOfBytes, 0, numOfBytes.Length);
            ms.Write(buffer, 0, buffer.Length);
            byte[] endofFile = Encoding.Default.GetBytes("|");
            ms.Write(endofFile, 0, endofFile.Length);
            int sent = 0;  // how many bytes is already sent
            do
            {
                try
                {
                    //client.GetStream().Write(buffer, offset, size);
                    sent += client.Client.Send(ms.ToArray(), offset + sent, ms.ToArray().Length - sent, SocketFlags.None);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.WouldBlock || ex.SocketErrorCode == SocketError.IOPending || ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                    {
                        // socket buffer is probably full, wait and try again
                        Thread.Sleep(30);
                    }
                    else
                    {
                        throw ex;  // any serious error occurr
                    }
                }
            } while (sent < size);
        }
        public static bool IsConnected(TcpClient _client)
        {
            bool isConnected = true;
            try
            {
                // Detect if client disconnected
                if (_client != null && _client.Client.Poll(0, SelectMode.SelectRead))
                {
                    byte[] buff = new byte[1];
                    if (_client.Client.Receive(buff, SocketFlags.Peek) == 0)
                    {
                        // Client disconnected
                        isConnected = false;
                    }
                }
                //Console.WriteLine($"Function 'IsConnected End -- the check'  {isConnected}");
            }
            //catch (SocketException se)
            catch
            {
                //Console.WriteLine(se.ErrorCode + ": " + se.Message);
            }
            return isConnected;
        }
    }
}
