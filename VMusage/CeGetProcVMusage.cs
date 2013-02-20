using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using System.Runtime.InteropServices;

using System.Process;

namespace VMusage
{
    class CeGetProcVMusage
    {
        public class processInfo
        {
            public string pName = "";
            public UInt32 pID = 0;
            public
            processInfo(string name, UInt32 procID)
            {
                pName = name;
                pID = procID;
            }
        }

        //private List<processInfo> processInfos;
        //public List<processInfo> _processInfos
        //{
        //    get {
        //        getProcVM();
        //        return processInfos; }
        //}

        private processInfo[] processInfoArray;

        //private string[] processNames;
        //public string[] _processNames
        //{
        //    get {
        //        getProcVM();
        //        return processNames; 
        //    }
        //}
        //private UInt32[] processIDs;
        //public UInt32[] _processIDs
        //{
        //    get
        //    {
        //        return processIDs;
        //    }
        //}
        public List<procVMinfo> _procVMinfo{
            get
            {
                procVMinfoList.Clear();
                //procVMinfoList = new List<procVMinfo>();
                getProcVM();
                return procVMinfoList;
            }
        }
        List<procVMinfo> procVMinfoList;

        /// <summary>
        /// contructor
        /// </summary>
        public CeGetProcVMusage(){
            //processNames = new string[32];
            processInfoArray = new processInfo[32];
            procVMinfoList = new List<procVMinfo>();
            //processInfos = new List<processInfo>();
            getProcVM();
        }

        void getProcessNames()
        {            
	        IntPtr hProcessSnap;
	        IntPtr hProcess;
	        Process.PROCESSENTRY32 pe32=new Process.PROCESSENTRY32();
            pe32.dwSize=(uint)(Marshal.SizeOf(typeof(Process.PROCESSENTRY32)));
	        uint slot;
	        uint STARTBAR=1;
	        uint NUMBARS=32;

            //slot0 = active app, do NOT USE
            //slot1 = ROM DLLs
            //slot2 = filesys.exe
            //slot3 = device.exe
	        for(slot=STARTBAR;slot<STARTBAR+NUMBARS;slot++)
	        {
                processInfoArray[slot - STARTBAR].pName = String.Format("Slot {0}: empty", slot);
                //processNames[slot-STARTBAR] = String.Format("Slot {0}: empty", slot);
	        }
            if ((1 - STARTBAR) >= 0)
            {
                //processNames[1 - STARTBAR] = String.Format("ROM DLLs");// "Slot 1: ROM DLLs");
                processInfoArray[1 - STARTBAR].pName = String.Format("ROM DLLs");
            }

	        // Take a snapshot of all processes in the system.
            uint oldPermissions = Process.SetProcPermissions(0xffffffff);
            hProcessSnap = Process.CreateToolhelp32Snapshot(Process.SnapshotFlags.Process | Process.SnapshotFlags.NoHeaps, 0);
            if (hProcessSnap != IntPtr.Zero)
            {
                int iRes = Process.Process32First(hProcessSnap, ref pe32);
                if (iRes != 0)
                {
                    do
                    {
                        hProcess = Process.OpenProcess(Process.ProcessAccessFlags.QueryInformation, false, (int)(pe32.th32ProcessID));
                        if (hProcess != IntPtr.Zero)
                        {
                            slot = pe32.th32MemoryBase / 0x02000000; //32MB slots, 1st = 32mb/32MB=1
                            System.Diagnostics.Debug.WriteLine("th32MemoryBase=" + pe32.th32MemoryBase.ToString() + "\tslot=" + slot.ToString()+
                                "\tname='"+pe32.szExeFile+"'");
                            //if (slot - STARTBAR < NUMBARS)
                            if (slot < NUMBARS)
                            {
                                processInfoArray[slot - STARTBAR].pName = String.Format("{0}", pe32.szExeFile);
                                // processInfoArray[slot].pName = String.Format("{0}", pe32.szExeFile);
                                processInfoArray[slot - STARTBAR].pID = pe32.th32ProcessID;
                                // processInfoArray[slot].pID = pe32.th32ProcessID;
                            }
                            else //NK.exe and ROM DLLs assigned to slot 1
                            {
                                processInfoArray[1].pName = String.Format("{0}", pe32.szExeFile);
                                processInfoArray[1].pID = pe32.th32ProcessID;
                            }

                            Process.CloseHandle(hProcess);
                        }
                    } while (Process.Process32Next(hProcessSnap, ref pe32) != 0);
                }
                else
                    System.Diagnostics.Debug.WriteLine("Process32First failed with " + Marshal.GetLastWin32Error().ToString());

                Process.CloseToolhelp32Snapshot(hProcessSnap);
            }
            Process.SetProcPermissions(oldPermissions);
        }

        StringBuilder getProcVM(){
	        StringBuilder str=new StringBuilder(1024);
	        for (int i=0; i<32; i++){
		        //processNames[i]="";
                processInfoArray[i] = new processInfo("", 0);
	        }
            getProcessNames();  //fills process Names and process IDs into processInfoArray
            //_processInfos.Clear();
            //procVMinfoList.Clear();

	        StringBuilder tempStr=new StringBuilder();
	        uint total = 0;
            int idx = 0;
            int STARTBAR = 1;
            uint NUMBARS = 32;
	        //for( int idx = 1; idx < 33; ++idx )
            for (idx = STARTBAR; idx < STARTBAR + NUMBARS; idx++)
	        {
		        Process.PROCVMINFO vmi=new Process.PROCVMINFO();
                int cbSize=Marshal.SizeOf (typeof(Process.PROCVMINFO) );
                long lTime = DateTime.Now.ToFileTimeUtc();
                int iPID = idx-1;
                if (iPID == 0)
                {
                    iPID = Process.GetProcessIDFromIndex(0);
                }
		        if( Process.CeGetProcVMInfo( iPID, cbSize, ref vmi ) !=0 )
		        {
			        //wsprintf(tempStr, L"%d: %d bytes\r\n", idx, vmi.cbRwMemUsed );
			        //str.Append( String.Format("%d (%s): %d bytes\r\n", idx, processNames[idx-1], vmi.cbRwMemUsed ));
                    str.Append(String.Format("%d (%s): %d bytes\r\n", idx, processInfoArray[idx - 1].pName, vmi.cbRwMemUsed));
			        //System.Diagnostics.Debug.WriteLine( String.Format("\r\n{0} ({1}): {2} bytes", idx, processNames[idx-1], vmi.cbRwMemUsed ));
                    
                    //System.Diagnostics.Debug.WriteLine(String.Format("\r\n{0} ({1}): {2} bytes", idx, processInfoArray[idx - 1].pName, vmi.cbRwMemUsed));
			        
                    total += vmi.cbRwMemUsed;
                    //procVMinfoList.Add(new procVMinfo(processNames[idx - 1], vmi.cbRwMemUsed, (byte)(idx)));
                    procVMinfoList.Add(new procVMinfo(processInfoArray[idx - 1].pName, processInfoArray[idx - 1].pID, vmi.cbRwMemUsed, (byte)idx, lTime));
		        }
	        }
	        str.Append(String.Format("Total: {0} bytes\r\n", total ));
            //System.Diagnostics.Debug.WriteLine( String.Format("Total: {0} bytes\r\n", total ));
	        return str;
        }
    }
}
