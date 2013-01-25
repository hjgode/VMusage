using System;

using System.Collections.Generic;
using System.Text;

using System.Runtime.InteropServices;

//using __int64 = System.UInt64;
using DWORD = System.UInt32;
using FILETIME = System.Int64;

namespace System.Process
{
    public partial class Process
    {
        #region process_stuff
        [DllImport("toolhelp.dll", SetLastError = true)]
        public static extern IntPtr CreateToolhelp32Snapshot(SnapshotFlags dwFlags, uint th32ProcessID);

        [DllImport("toolhelp.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern int Process32First([In]IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [DllImport("toolhelp.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern int Process32Next([In]IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [DllImport("toolhelp.dll")]
        public static extern int CloseToolhelp32Snapshot(IntPtr snapshot);

        [DllImport("toolhelp.dll", SetLastError = true)]
        static extern Int32 Thread32First(IntPtr hSnapshot, ref THREADENTRY32 lpte);
        [DllImport("toolhelp.dll", SetLastError = true)]
        static extern Int32 Thread32Next(IntPtr hSnapshot, out THREADENTRY32 lpte);

        [DllImport("coredll.dll")]
        public static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        // see also http://www.codeproject.com/Tips/123149/How-to-get-individual-process-memory-usage-statist
        [StructLayout(LayoutKind.Sequential)]
        public struct PROCVMINFO
        {
            public int hproc;
            public uint cbRwMemUsed;
        }
        [DllImport("coredll.dll", SetLastError = true)]
        static extern Boolean KernelLibIoControl(IntPtr hLib, 
            int dwIoControlCode, 
            IntPtr lpOutBuf, 
            int nInBufSize, 
            ref PROCVMINFO lpInBuf,
            int nOutBufSize, 
            IntPtr lpBytesReturned);

        public const int KMOD_CORE = 1;
        private const int IOCTL_KLIB_GETPROCMEMINFO = 8;

        [DllImport("coredll.dll", SetLastError = true)]
        static extern bool GetThreadTimes(IntPtr hThread, out FILETIME lpCreationTime, out FILETIME lpExitTime, out FILETIME lpKernelTime, out FILETIME lpUserTime);
        [DllImport("coredll.dll", SetLastError = true)]
        static extern bool GetThreadTimes(uint hThread, out FILETIME lpCreationTime, out FILETIME lpExitTime, out FILETIME lpKernelTime, out FILETIME lpUserTime);

        [DllImport("coredll.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("coredll.dll")]
        public static extern uint GetTickCount();

        [DllImport("coredll.dll")]
        public static extern UInt32 SetProcPermissions(UInt32 uPerm);

        [Flags()]
        public enum ProcessAccessFlags : int
        {
            /// <summary>Specifies all possible access flags for the process object.</summary>
            AllAccess = CreateThread | DuplicateHandle | QueryInformation | SetInformation | Terminate | VMOperation | VMRead | VMWrite | Synchronize,
            /// <summary>Enables usage of the process handle in the CreateRemoteThread function to create a thread in the process.</summary>
            CreateThread = 0x2,
            /// <summary>Enables usage of the process handle as either the source or target process in the DuplicateHandle function to duplicate a handle.</summary>
            DuplicateHandle = 0x40,
            /// <summary>Enables usage of the process handle in the GetExitCodeProcess and GetPriorityClass functions to read information from the process object.</summary>
            QueryInformation = 0x400,
            /// <summary>Enables usage of the process handle in the SetPriorityClass function to set the priority class of the process.</summary>
            SetInformation = 0x200,
            /// <summary>Enables usage of the process handle in the TerminateProcess function to terminate the process.</summary>
            Terminate = 0x1,
            /// <summary>Enables usage of the process handle in the VirtualProtectEx and WriteProcessMemory functions to modify the virtual memory of the process.</summary>
            VMOperation = 0x8,
            /// <summary>Enables usage of the process handle in the ReadProcessMemory function to' read from the virtual memory of the process.</summary>
            VMRead = 0x10,
            /// <summary>Enables usage of the process handle in the WriteProcessMemory function to write to the virtual memory of the process.</summary>
            VMWrite = 0x20,
            /// <summary>Enables usage of the process handle in any of the wait functions to wait for the process to terminate.</summary>
            Synchronize = 0x100000
        }
        //inner enum used only internally
        [Flags]
        public enum SnapshotFlags : uint
        {
            HeapList = 0x00000001,
            Process = 0x00000002,
            Thread = 0x00000004,
            Module = 0x00000008,
            Module32 = 0x00000010,
            Inherit = 0x80000000,
            All = 0x0000001F,
            NoHeaps = 0x40000000
        }
        /*
typedef struct tagPROCESSENTRY32 {
    DWORD   dwSize;
    DWORD   cntUsage;
    DWORD   th32ProcessID;
    DWORD   th32DefaultHeapID;
    DWORD   th32ModuleID;
    DWORD   cntThreads;
    DWORD   th32ParentProcessID;
    LONG    pcPriClassBase;
    DWORD   dwFlags;
    TCHAR   szExeFile[MAX_PATH];
    DWORD	th32MemoryBase;
    DWORD	th32AccessKey;
} PROCESSENTRY32, *PPROCESSENTRY32, *LPPROCESSENTRY32;
        */
        //inner struct used only internally
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct PROCESSENTRY32
        {
            const int MAX_PATH = 260;
            internal UInt32 dwSize;
            internal UInt32 cntUsage;
            internal UInt32 th32ProcessID;
            internal IntPtr th32DefaultHeapID;
            internal UInt32 th32ModuleID;
            internal UInt32 cntThreads;
            internal UInt32 th32ParentProcessID;
            internal Int32 pcPriClassBase;
            internal UInt32 dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            internal string szExeFile;
            public uint th32MemoryBase;
            public uint th32AccessKey;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct THREADENTRY32
        {

            internal UInt32 dwSize;
            internal UInt32 cntUsage;
            internal UInt32 th32ThreadID;
            internal UInt32 th32OwnerProcessID;
            internal UInt32 tpBasePri;
            internal UInt32 tpDeltaPri;
            internal UInt32 dwFlags;
            internal UInt32 th32AccessKey;
            internal UInt32 th32CurrentProcessID;
        }

        #endregion

        #region SYSTEM_TIME
        [DllImport("coredll.dll", EntryPoint = "SystemTimeToFileTime", SetLastError = true)]
        static extern bool SystemTimeToFileTime(ref SYSTEMTIME lpSystemTime, ref FILETIME lpFileTime);

        [DllImport("coredll.dll", SetLastError = true)]
        static extern bool FileTimeToSystemTime([In] ref FILETIME lpFileTime, out SYSTEMTIME lpSystemTime);

        [DllImport("coredll.dll")]
        static extern bool FileTimeToLocalFileTime([In] ref FILETIME lpFileTime, out FILETIME lpLocalFileTime);

        [DllImport("coredll.dll")]
        static extern int OpenProcess(UInt32 dwAccess, bool bInherit, UInt32 dwIDProcess);

        //[StructLayout(LayoutKind.Sequential)]
        //public struct FILETIME
        //{
        //    public UInt32 dwLowDateTime;
        //    public UInt32 dwHighDateTime;
        //}

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEMTIME
        {
            [MarshalAs(UnmanagedType.U2)]
            public short Year;
            [MarshalAs(UnmanagedType.U2)]
            public short Month;
            [MarshalAs(UnmanagedType.U2)]
            public short DayOfWeek;
            [MarshalAs(UnmanagedType.U2)]
            public short Day;
            [MarshalAs(UnmanagedType.U2)]
            public short Hour;
            [MarshalAs(UnmanagedType.U2)]
            public short Minute;
            [MarshalAs(UnmanagedType.U2)]
            public short Second;
            [MarshalAs(UnmanagedType.U2)]
            public short Milliseconds;

            public SYSTEMTIME(DateTime dt)
            {
                dt = dt.ToUniversalTime();  // SetSystemTime expects the SYSTEMTIME in UTC
                Year = (short)dt.Year;
                Month = (short)dt.Month;
                DayOfWeek = (short)dt.DayOfWeek;
                Day = (short)dt.Day;
                Hour = (short)dt.Hour;
                Minute = (short)dt.Minute;
                Second = (short)dt.Second;
                Milliseconds = (short)dt.Millisecond;
            }
        }

        private const int INVALID_HANDLE_VALUE = -1;

        #endregion
        internal class CosmicPowers : IDisposable
        {
            private UInt32 oldPermissions = 0;
            static bool alreadyDone = false;
            public CosmicPowers()
            {
                if (alreadyDone)
                    return;
                //we_are_powerful;
                oldPermissions = SetProcPermissions(0xffffffff);
                alreadyDone = true;
            }
            ~CosmicPowers()
            {
                SetProcPermissions(oldPermissions);
            }
            public void Dispose()
            {
                SetProcPermissions(oldPermissions);
            }
        }

        myFILETIME getFileTimeDiff(myFILETIME ftStart, myFILETIME ftEnd)
        {
            myFILETIME ftDiff = new myFILETIME();
            UInt64 uStart = (((UInt64)ftStart.dwHighDateTime) << 32) + ftStart.dwLowDateTime;
            UInt64 uEnd = (((UInt64)ftEnd.dwHighDateTime) << 32) + ftEnd.dwLowDateTime;
            UInt64 uDiff = uEnd - uStart;

            return ftDiff;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct myFILETIME
        {
            public uint dwLowDateTime;
            public uint dwHighDateTime;
        }
        public static int CeGetProcVMInfo(int idxProc, int cbSize, ref PROCVMINFO pinfo)
        {
            if (KernelLibIoControl((IntPtr)KMOD_CORE, IOCTL_KLIB_GETPROCMEMINFO, IntPtr.Zero, idxProc, ref pinfo, cbSize, IntPtr.Zero))
            {
                System.Diagnostics.Debug.WriteLine( cbSize.ToString() );

            }
            return cbSize;

        }
    }
}
