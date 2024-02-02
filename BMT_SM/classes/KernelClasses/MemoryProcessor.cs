using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace HawkSync_SM
{
    public static class MemoryProcessor
    {
        const int PROCESS_WM_READ = 0x0010;
        const int PROCESS_VM_WRITE = 0x0020;
        const int PROCESS_VM_OPERATION = 0x0008;
        const int PROCESS_QUERY_INFORMATION = 0x0400;

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        public static bool Write(Instance _instance, int addr, byte[] buffer, int bufferSize, ref int bytesWritten)
        {
            bool status = WriteProcessMemory((int)_instance.Handle, addr, buffer, bufferSize, ref bytesWritten);
            if (status == false)
            {
                _instance.Handle = IntPtr.Zero;
                _instance.Handle = OpenProcess(PROCESS_WM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_QUERY_INFORMATION, false, _instance.PID.GetValueOrDefault());
                WriteProcessMemory((int)_instance.Handle, addr, buffer, bufferSize, ref bytesWritten);
            }
            byte[] errorCheck = new byte[bufferSize];
            int byteRead = 0;
            ReadProcessMemory((int)_instance.Handle, addr, errorCheck, bufferSize, ref byteRead);
            if (errorCheck.SequenceEqual(buffer) == false || errorCheck.Length == 0 || byteRead == 0)
            {
                _instance.Handle = IntPtr.Zero;
                _instance.Handle = OpenProcess(PROCESS_WM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_QUERY_INFORMATION, false, _instance.PID.GetValueOrDefault());
                WriteProcessMemory((int)_instance.Handle, addr, buffer, bufferSize, ref bytesWritten);
            }
            return status;
        }
    }
}
