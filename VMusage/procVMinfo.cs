using System;
using System.Collections.Generic;
using System.Text;

namespace VMusage
{
    /// <summary>
    /// holds the VM data of one process
    /// </summary>
    public class procVMinfo
    {
        public string remoteIP;
        public string name;
        public UInt32 memusage;
        public byte slot;
        public UInt32 procID;
        public long Time;

        public procVMinfo()
        {
            remoteIP = "0.0.0.0";
            name = "unknown";
            memusage = 0;
            slot = 0;
            procID = 0;
            Time = DateTime.Now.ToFileTimeUtc();
        }
        public procVMinfo(string n, UInt32 pID, uint m, byte bSlot)
        {
            name = n;
            memusage = m;
            slot = bSlot;
            procID = pID;
            Time = DateTime.Now.ToFileTimeUtc();
        }
        public procVMinfo(string n, UInt32 pID, uint m, byte bSlot, long lTime)
        {
            name = n;
            memusage = m;
            slot = bSlot;
            procID = pID;
            Time = lTime;
        }

        public procVMinfo(byte[] buf)
        {
            this.fromBytes(buf);
        }
        public override string ToString()
        {
            return
                slot.ToString() + "\t"
                + name + "\t 0x" +
                procID.ToString("x") + "\t" +
                memusage.ToString() + " bytes\t" +
                remoteIP;
        }
        public byte[] toByte()
        {
            List<byte> buf = new List<byte>();
            //slot
            buf.AddRange(BitConverter.GetBytes((Int16)slot));
            //memusage
            buf.AddRange(BitConverter.GetBytes((UInt32)memusage));
            //name length
            Int16 len = (Int16)name.Length;
            buf.AddRange(BitConverter.GetBytes(len));
            //name string
            buf.AddRange(Encoding.UTF8.GetBytes(name));
            //procID
            buf.AddRange(BitConverter.GetBytes((UInt32)procID));
            //timestamp
            buf.AddRange(BitConverter.GetBytes((UInt64)Time));

            return buf.ToArray();
        }
        public procVMinfo fromBytes(byte[] buf)
        {
            int offset = 0;

            //is magic packet?
            if (ByteHelper.isLargePacket(buf))
                offset += sizeof(UInt64);   //cut first bytes

            //read slot
            this.slot = (byte)BitConverter.ToInt16(buf, offset);
            offset += sizeof(System.Int16);

            UInt32 _memuse = BitConverter.ToUInt32(buf, offset);
            memusage = _memuse;
            offset += sizeof(System.UInt32);

            Int16 bLen = BitConverter.ToInt16(buf, offset);
            offset += sizeof(System.Int16);
            if (bLen > 0)
            {
                this.name = System.Text.Encoding.UTF8.GetString(buf, offset, bLen);
            }
            offset += bLen;
            this.procID = BitConverter.ToUInt32(buf, offset);
            
            offset += sizeof(System.UInt32);
            this.Time = (long) BitConverter.ToUInt64(buf, offset);

            return this;
        }
        public procVMinfo fromBytes(byte[] buf, ref int bufOffset)
        {
            int offset = bufOffset;

            ////is magic packet?
            //if (ByteHelper.isLargePacket(buf))
            //    offset += sizeof(UInt64);   //cut first bytes

            //read slot
            this.slot = (byte)BitConverter.ToInt16(buf, offset);
            offset += sizeof(System.Int16);

            UInt32 _memuse = BitConverter.ToUInt32(buf, offset);
            memusage = _memuse;
            offset += sizeof(System.UInt32);

            Int16 bLen = BitConverter.ToInt16(buf, offset);
            offset += sizeof(System.Int16);
            if (bLen > 0)
            {
                this.name = System.Text.Encoding.UTF8.GetString(buf, offset, bLen);
            }
            offset += bLen;
            this.procID = BitConverter.ToUInt32(buf, offset);
            offset += sizeof(System.UInt32);

            this.Time = (long)BitConverter.ToUInt64(buf, offset);
            offset += sizeof(System.UInt64);

            bufOffset = offset;
            return this;
        }

        public List<procVMinfo> getprocVmList(byte[] buf, string sIP)
        {
            List<procVMinfo> _mList = new List<procVMinfo>();
            int offset = 0;

            //is magic packet?
            if (ByteHelper.isLargePacket(buf))
                offset += sizeof(UInt64);   //cut first bytes
            try
            {
                while (offset < buf.Length)
                {
                    procVMinfo pi = new procVMinfo();
                    pi.remoteIP = sIP;
                    _mList.Add(pi.fromBytes(buf, ref offset));
                }
            }
            catch (Exception)
            {
                
            }
            return _mList;
        }
    }
}
