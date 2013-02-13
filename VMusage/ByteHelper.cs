using System;
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
        const UInt64 magicMemInfoStatus = 0x1122334455667788;
        public static byte[] meminfostatusBytes
        {
            get
            {
                return BitConverter.GetBytes(magicMemInfoStatus);
            }
        }

        public static bool isMemInfoPacket(byte[] buf)
        {
            bool bRet = false;
            if (buf.Length != 36)
                return bRet;

            int bLen = ByteHelper.meminfostatusBytes.Length;
            if (buf.Length < bLen)
                return bRet;

            UInt64 u64 = BitConverter.ToUInt64(buf, 0);
            if (u64 == magicMemInfoStatus)
                bRet = true;

            return bRet;
        }

        public static bool isMemInfoStatus(byte[] buf){
            bool bRet = false;
            if (buf.Length != 8)
                return bRet;
            try
            {
                UInt64 u64 = BitConverter.ToUInt64(buf, 0);
                if (u64 == magicMemInfoStatus)
                    bRet = true;
            }
            catch (Exception)
            {
                bRet = false;
            }
            return bRet;
        }

        const UInt64 magicLargePacket = 0xAAffBBeeCC1100dd;
        public static byte[] LargePacketBytes{
            get{
                return BitConverter.GetBytes(magicLargePacket);
            }
        }
        public static bool isLargePacket(byte[] buf)
        {
            bool bRet = false;
            if (buf.Length < 8)
                return bRet;
            try
            {
                UInt64 u64 = BitConverter.ToUInt64(buf, 0);
                if (u64 == magicLargePacket)
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