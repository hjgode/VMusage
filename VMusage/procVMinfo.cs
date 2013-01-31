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
        public long Time;

        public procVMinfo(string n, uint m, byte s)
        {
            name = n;
            memusage = m;
            slot = s;
            Time = DateTime.Now.ToFileTimeUtc();
        }
        public procVMinfo(byte[] buf)
        {
            this.fromBytes(buf);
        }
        public override string ToString()
        {
            return slot.ToString() + ":" + name + ": " + memusage.ToString() + " bytes";
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
            return this;
        }
    }
}
