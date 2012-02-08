using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inversion.Utils;
using System.Globalization;

namespace Inversion.Utils
{
    internal static class BitUtils
    {
        public static bool IsSet(byte indicator, byte flag)
        {
            return (indicator & flag) == flag;
        }

        public static byte[] FromHexString(string value)
        {
            if (String.IsNullOrEmpty(value)) { throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, CommonResources.Argument_Cannot_Be_Null_Or_Empty, "value"), "value"); }

            if (value.Length % 2 != 0)
            {
                // Prepend a zero
                // PERF: This is so not the fastest way to do this :)
                value = "0" + value;
            }

            byte[] output = new byte[value.Length / 2];
            for (int i = 0; i < value.Length / 2 ; i++)
            {
                byte msb;
                byte lsb;
                if (!TryParseNybble(value[i * 2], out msb) || !TryParseNybble(value[(i * 2) + 1], out lsb))
                {
                    throw new FormatException(String.Format(CoreResources.Input_Not_Valid_Hex_String, value));
                }
                output[i] = (byte)((msb << (byte)4) + lsb);
            }
            return output;
        }

        private static bool TryParseNybble(char c, out byte converted)
        {
            converted = 0;
            if (c >= '0' && c <= '9')
            {
                converted = (byte)(c - '0');
            }
            else if (c >= 'A' && c <= 'F')
            {
                converted = (byte)((c - 'A') + 10);
            }
            else if (c >= 'a' && c <= 'f')
            {
                converted = (byte)((c - 'a') + 10);
            }
            else
            {
                return false;
            }
            return true;
        }
    }
}
