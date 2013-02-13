using System;
using System.Collections.Generic;
using System.Text;

namespace VMusage
{
    public class MemoryInfoHelper
    {
        public UInt32 memoryLoad = 0;

        public UInt32 totalPhysical = 0;
        public UInt32 availPhysical = 0;

        public UInt32 totalPageFile = 0;
        public UInt32 availPageFile = 0;

        public UInt32 totalVirtual = 0;
        public UInt32 availVirtual = 0;

        public MemoryInfoHelper()
        {
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("MemoryLoad=" + memoryLoad.ToString() + "\r\n");
            sb.Append("Physical Total=" + totalPhysical.ToString() + "\r\n");
            sb.Append("     Available=" + availPhysical.ToString() + "\r\n");
            sb.Append("Pagefile Total=" + totalPageFile.ToString() + "\r\n");
            sb.Append("     Available=" + availPageFile.ToString() + "\r\n");
            sb.Append("Virtual Total =" + totalVirtual.ToString() + "\r\n");
            sb.Append("     Available=" + availVirtual.ToString() + "\r\n");
            return sb.ToString();
            //return base.ToString();
        }
        public MemoryInfoHelper (memorystatus.MemoryInfo.MEMORYSTATUS ms)
        {
            memoryLoad = ms.dwMemoryLoad;

            totalPhysical = ms.dwTotalPhys;
            availPhysical = ms.dwAvailPhys;

            totalPageFile = ms.dwTotalPageFile;
            availPageFile = ms.dwAvailPageFile;

            totalVirtual = ms.dwTotalVirtual;
            availVirtual = ms.dwAvailVirtual;
        }

        public MemoryInfoHelper(byte[] buf)
        {
            this.fromByte(buf);
        }

        public MemoryInfoHelper fromByte(byte[] buf){
            MemoryInfoHelper mi = this;
            int offset = 0;

            //jump behind marker
            offset = ByteHelper.meminfostatusBytes.Length;

            mi.memoryLoad = (UInt32)BitConverter.ToUInt32(buf, offset);
            offset += sizeof(System.UInt32);

            mi.totalPhysical = (UInt32)BitConverter.ToUInt32(buf, offset);
            offset +=sizeof(System.UInt32);
            mi.availPhysical = (UInt32)BitConverter.ToUInt32(buf, offset);
            offset +=sizeof(System.UInt32);

            mi.totalPageFile = (UInt32)BitConverter.ToUInt32(buf, offset);
            offset +=sizeof(System.UInt32);
            mi.availPageFile = (UInt32)BitConverter.ToUInt32(buf, offset);
            offset +=sizeof(System.UInt32);

            mi.totalVirtual = (UInt32)BitConverter.ToUInt32(buf, offset);
            offset +=sizeof(System.UInt32);
            mi.availVirtual = (UInt32)BitConverter.ToUInt32(buf, offset);

            return mi;
        }
        public byte[] toByte()
        {
            List<byte> buf = new List<byte>();

            //add a marker
            buf.AddRange(ByteHelper.meminfostatusBytes);
            // 7 X 4Bytes = 28Bytes
            buf.AddRange(BitConverter.GetBytes((UInt32)memoryLoad));
            buf.AddRange(BitConverter.GetBytes((UInt32)totalPhysical));
            buf.AddRange(BitConverter.GetBytes((UInt32)availPhysical)); 
            buf.AddRange(BitConverter.GetBytes((UInt32)totalPageFile));
            buf.AddRange(BitConverter.GetBytes((UInt32)availPageFile));
            buf.AddRange(BitConverter.GetBytes((UInt32)totalVirtual));
            buf.AddRange(BitConverter.GetBytes((UInt32)availVirtual));
            return buf.ToArray();
        }

    }
}
