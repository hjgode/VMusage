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

        public procVMinfo(string n, UInt32 pID, uint m, byte bSlot)
        {
            name = n;
            memusage = m;
            slot = bSlot;
            procID = pID;
            Time = DateTime.Now.ToFileTimeUtc();
        }
        public procVMinfo(byte[] buf)
        {
            this.fromBytes(buf);
        }
        public override string ToString()
        {
            return slot.ToString() + ":" + name + ": 0x" + procID.ToString("x") + ": " + memusage.ToString() + " bytes";
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

            return buf.ToArray();
        }
        public procVMinfo fromBytes(byte[] buf)
        {
            int offset = 0;
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
            return this;
        }
    }
}
