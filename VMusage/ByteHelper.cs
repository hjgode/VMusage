using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace VMusage
{
    class ByteHelper
    {
        const UInt64 magicBytes = 0x524015240152401;
        public static byte[] endOfTransferBytes
        {
            get
            {
                return BitConverter.GetBytes(magicBytes);
            }
        }//= new byte[] { 0xFF, 0xFa, 0xaf, 0xAA };

        public static bool isEndOfTransfer(byte[] buf)
        {
            bool bRet = false;
            if (buf.Length != 8)
                return bRet;
            try
            {
                UInt64 u64 = BitConverter.ToUInt64(buf, 0);
                if (u64 == magicBytes)
                    bRet = true;
            }
            catch (Exception)
            {
                bRet = false;
            }
            return bRet;
        }
    }
}