using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HorioFingerprintCapturer
{
    public class TemplateConverter
    {
        public static byte[] ConvertToSynelTemplateFormat(byte[] bytes)
        {
            byte[] synelBytes = new byte[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                byte leftNibble = bytes[i];
                leftNibble &= 0xF0;
                leftNibble >>= 4;
                leftNibble |= 0x60;
                synelBytes[i * 2] = leftNibble;

                byte rightNibble = bytes[i];
                rightNibble &= 0x0F;
                rightNibble |= 0x30;
                synelBytes[i * 2 + 1] = rightNibble;
            }
            return synelBytes;
        }

        public static byte[] ConvertFromSynelFormat(byte[] bytes)
        {
            byte[] originalBytes = new byte[bytes.Length / 2];

            for (int i = 0; i < bytes.Length; i += 2)
            {
                byte tmp = bytes[i];
                byte threeStartingByte = bytes[i + 1];

                tmp &= 0x0F;
                tmp <<= 4;

                threeStartingByte &= 0x0F;
                originalBytes[i / 2] = threeStartingByte |= tmp;
            }
            return originalBytes;
        }
    }
}
