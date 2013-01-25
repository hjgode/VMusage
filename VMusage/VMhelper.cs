using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using System.Runtime.InteropServices;

using DWORD = System.Int32;
using BYTE = System.Byte;

namespace VMusage
{
    class VMhelper
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEM_INFO
        {
            internal PROCESSOR_INFO_UNION p;
            public uint dwPageSize;
            public IntPtr lpMinimumApplicationAddress;
            public IntPtr lpMaximumApplicationAddress;
            public UIntPtr dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public ushort wProcessorLevel;
            public ushort wProcessorRevision;
        };

        [StructLayout(LayoutKind.Explicit)]
        public struct PROCESSOR_INFO_UNION
        {
            [FieldOffset(0)]
            internal uint dwOemId;
            [FieldOffset(0)]
            internal ushort wProcessorArchitecture;
            [FieldOffset(2)]
            internal ushort wReserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION
        {
            public UIntPtr BaseAddress;
            public UIntPtr AllocationBase;
            public uint AllocationProtect;
            public IntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }
        SYSTEM_INFO system_info;
        MEMORY_BASIC_INFORMATION mbi;

        [DllImport("coredll.dll")]
        private static extern int VirtualQuery(
            ref uint lpAddress,
            ref MEMORY_BASIC_INFORMATION lpBuffer,
            int dwLength
        );

        [DllImport("coredll.dll")]
        private static extern void GetSystemInfo(
            [MarshalAs(UnmanagedType.Struct)] ref SYSTEM_INFO lpSystemInfo
        );
        [DllImport("coredll.dll")]
        private static extern int VirtualQuery(
            ref UIntPtr lpAddress,
            ref MEMORY_BASIC_INFORMATION lpBuffer,
            IntPtr dwLength
        );

        const int TH32CS_SNAPNOHEAPS = 0x40000000;
        const int MEM_COMMIT = 0x1000;
        const int MEM_RESERVE = 0x2000;
        const int VMEMCOMMIT = (MEM_COMMIT>>12);
        const int VMEMRESERVE = (MEM_RESERVE>>12);
        const int NUMPAGES = 8192;
        const int NUMBARS = 32;

        struct VIRTUALDATA
        {
	        public int barWidth;
	        public int focusSlot;
	        public string[] szExeName;//= new string[NUMBARS];
	        public byte[,] pageAllocated;// = new bool[NUMBARS, NUMPAGES];
        };

        public void ShowMemory()
        {
            int iSize;

            GetSystemInfo(ref system_info);
            Console.WriteLine("dwProcessorType: {0}", system_info.dwProcessorType.ToString());
            Console.WriteLine("dwPageSize: {0}", system_info.dwPageSize.ToString());

            if (VirtualQuery(ref system_info.dwPageSize,
                ref mbi,
                (int)System.Runtime.InteropServices.Marshal.SizeOf(mbi)) != 0
            )
            {
                Console.WriteLine("AllocationBase: {0}", mbi.AllocationBase);
                Console.WriteLine("BaseAddress: {0}", mbi.BaseAddress);
                Console.WriteLine("RegionSize: {0}", mbi.RegionSize);
            }
            else
            {
                Console.WriteLine("ERROR: VirtualQuery() failed.");
            }
        }

        public void test()
        {
            VIRTUALDATA vd = new VIRTUALDATA();
            vd.pageAllocated = new byte[NUMBARS, NUMPAGES];
            vd.szExeName = new string[NUMBARS];
            GetVirtualMemoryStatus(ref vd);
        }
        void GetVirtualMemoryStatus(ref VIRTUALDATA pvd)
        {
	        MEMORY_BASIC_INFORMATION mbi;
	        int idx;
	        DWORD addr;
	        BYTE state;
            int STARTBAR=0;

	        //memset(pvd->pageAllocated,0x00,sizeof(pvd->pageAllocated));

	        for(idx=STARTBAR;idx<STARTBAR+NUMBARS;idx++)
	        {
		        DWORD offset;

		        addr = idx * 0x02000000;

		        for( offset = 0; offset < 0x02000000; offset += mbi.RegionSize.ToInt32() )
		        {
			        Int32 i;

			        //memset(&mbi,0x00,sizeof(MEMORY_BASIC_INFORMATION));
                    uint newAddr = (uint)(addr + offset);
                    mbi = new MEMORY_BASIC_INFORMATION();
			        if(VirtualQuery( ref newAddr, ref mbi, Marshal.SizeOf(typeof( MEMORY_BASIC_INFORMATION )) )==0)
				        break;

			        state=(BYTE)((mbi.State>>12)&(VMEMCOMMIT|VMEMRESERVE));

			        if(state>0)
			        {
				        for(i=(offset)/4096;i< (offset+mbi.RegionSize.ToInt32())/4096;i++)
					        pvd.pageAllocated[idx-STARTBAR,i]=state;
			        }
		        }
	        }
        }
    }
}
