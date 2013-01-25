using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace VMusage
{
    /// <summary>
    /// holds the VM data of one process
    /// </summary>
    public class procVMinfo
    {
        public string name;
        public UInt32 memusage;
        public byte slot;
        public procVMinfo(string n, uint m, byte s)
        {
            name = n;
            memusage = m;
            slot = s;
        }
        public override string ToString()
        {
            return slot.ToString() + ":" + name + ": " + memusage.ToString() + " bytes";
        }
        public byte[] toByte()
        {
            List<byte> buf = new List<byte>();
            buf.AddRange(BitConverter.GetBytes((Int16)slot));

            buf.AddRange(BitConverter.GetBytes((UInt32)memusage));
            
            Int16 len = (Int16)name.Length;
            buf.AddRange(BitConverter.GetBytes(len));
            buf.AddRange(Encoding.UTF8.GetBytes(name));
            
            return buf.ToArray();
        }
        public procVMinfo fromBytes(byte[] buf)
        {
            int offset = 0;
            //read slot
            this.slot = (byte)BitConverter.ToInt16(buf,0);
            offset += offset += sizeof(System.Int16);

            UInt32 _memuse = BitConverter.ToUInt32(buf, offset);
            offset += sizeof(System.UInt32);

            int bLen = BitConverter.ToInt16(buf, offset);
            if (bLen > 0)
            {
                this.name = System.Text.Encoding.UTF8.GetString(buf, offset, bLen);
            }
            return this;
        }
    }
}
