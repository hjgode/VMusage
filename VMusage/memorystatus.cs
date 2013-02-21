using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace VMusage
{
    public class memorystatus
    {
        #region MEMORY_INFO
        public class MemoryInfo
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct MEMORYSTATUS
            {
                public UInt32 dwLength;
                public UInt32 dwMemoryLoad;
                public UInt32 dwTotalPhys;
                public UInt32 dwAvailPhys;
                public UInt32 dwTotalPageFile;
                public UInt32 dwAvailPageFile;
                public UInt32 dwTotalVirtual;
                public UInt32 dwAvailVirtual;
            }
            /*
typedef struct _MEMORYSTATUS {
    DWORD dwLength;
    DWORD dwMemoryLoad;
    DWORD dwTotalPhys;
    DWORD dwAvailPhys;
    DWORD dwTotalPageFile;
    DWORD dwAvailPageFile;
    DWORD dwTotalVirtual;
    DWORD dwAvailVirtual;
} MEMORYSTATUS, *LPMEMORYSTATUS;
            */
            [DllImport("CoreDll.dll", SetLastError=true)]
            public static extern void GlobalMemoryStatus(ref MEMORYSTATUS lpBuffer);

            public static bool GetMemoryStatus(ref MEMORYSTATUS memStatus)
            {
                bool result = true;
                // Call the native GlobalMemoryStatus method
                // with the defined structure.
                memStatus.dwLength = (uint) Marshal.SizeOf(memStatus);// 32; // bytes
                try
                {
                    GlobalMemoryStatus(ref memStatus);
                }
                catch
                {
                    result = false;
                }
                return result;
            }
            public static string dumpMSI(MEMORYSTATUS _ms)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("MEMORYSTATUS info" + "\r\n");
                sb.Append("dwTotalPhys: " + _ms.dwTotalPhys.ToString()+"\r\n");
                sb.Append("dwAvailPhys: " + _ms.dwAvailPhys.ToString() + "\r\n");

                sb.Append("dwTotalVirtual: " + _ms.dwTotalVirtual.ToString() + "\r\n");
                sb.Append("dwAvailVirtual: " + _ms.dwAvailVirtual.ToString() + "\r\n");

                sb.Append("dwTotalPageFile: " + _ms.dwTotalPageFile.ToString() + "\r\n");
                sb.Append("dwAvailPageFile: " + _ms.dwAvailPageFile.ToString() + "\r\n");

                sb.Append("dwMemoryLoad: " + _ms.dwMemoryLoad.ToString() + "\r\n");

                return sb.ToString();
            }
            /// <summary>
            /// get total avail phys bytes
            /// </summary>
            /// <returns></returns>
            public static UInt32 getTotalPhys()
            {
                UInt32 tvm = 0;
                MEMORYSTATUS ms=new MEMORYSTATUS();
                ms.dwLength=(uint)Marshal.SizeOf(typeof(MEMORYSTATUS));
                if (GetMemoryStatus(ref ms))
                    tvm = ms.dwTotalPhys;
                return tvm;
            }
            public static UInt32 getAvailPhys()
            {
                UInt32 tvm = 0;
                MEMORYSTATUS ms = new MEMORYSTATUS();
                ms.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUS));
                if (GetMemoryStatus(ref ms))
                    tvm = ms.dwAvailPhys;
                return tvm;
            }
            public static UInt32 getTotalVirtual()
            {
                UInt32 tvm = 0;
                MEMORYSTATUS ms=new MEMORYSTATUS();
                ms.dwLength=(uint)Marshal.SizeOf(typeof(MEMORYSTATUS));
                if (GetMemoryStatus(ref ms))
                    tvm = ms.dwTotalVirtual;
                return tvm;
            }
            public static UInt32 getAvailVirtual()
            {
                UInt32 tvm = 0;
                MEMORYSTATUS ms=new MEMORYSTATUS();
                ms.dwLength=(uint)Marshal.SizeOf(typeof(MEMORYSTATUS));
                if (GetMemoryStatus(ref ms))
                    tvm = ms.dwAvailVirtual;
                return tvm;
            }
        } // public class MemoryInfo
        #endregion
    }
}
