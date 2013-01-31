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

            [DllImport("CoreDll.dll")]
            public static extern void GlobalMemoryStatus
            (
                ref MEMORYSTATUS lpBuffer
            );

            public static bool GetMemoryStatus(ref MEMORYSTATUS memStatus)
            {
                bool result = true;
                // Call the native GlobalMemoryStatus method
                // with the defined structure.
                memStatus.dwLength = 32; // bytes
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
        } // public class MemoryInfo
        #endregion
    }
}
