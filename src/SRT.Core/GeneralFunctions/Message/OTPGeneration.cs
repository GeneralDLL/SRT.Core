// Ignore Spelling: DTO SRT

using SRT.Core.SRTExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SRT.Core.GeneralFunctions.Message
{
    public static class OTPGeneration
    {
        public static string GenerateOTP() => GenerateOTP(4);
        public static string GenerateOTP(int OTPLength, bool checksum = true)
        {
            if (checksum)
            {
                var otp = GenerateOTP("JBSWY3DPEHPK3PXP", OTPLength - 1);
                var sum = otp.Select(q => int.Parse(q.ToString())).Sum().ToString();

                return otp + sum.Last();
            }

            return GenerateOTP("JBSWY3DPEHPK3PXP", OTPLength);
        }
        public static string GenerateOTP(string secret)
        {
            var key = Base32Encoding.ToBytes(secret);
            var epochSeconds = (long)Math.Floor((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
            var timeStepSeconds = 30;
            var counter = BitConverter.GetBytes(epochSeconds / timeStepSeconds);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(counter);
            }
            var hmac = new HMACSHA1(key);
            var hash = hmac.ComputeHash(counter);
            var offset = hash[hash.Length - 1] & 0xf;
            var truncatedHash = BitConverter.ToInt32(hash, offset) & 0x7fffffff;
            var otp = truncatedHash % 1000000;
            return otp.ToString("D6");
        }

        public static string GenerateOTP(string secret, int OTPLength)
        {
            var key = Base32Encoding.ToBytes(secret);
            var epochSeconds = (long)Math.Floor((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
            var timeStepSeconds = 30;
            var counter = BitConverter.GetBytes(epochSeconds / timeStepSeconds);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(counter);
            }
            var hmac = new HMACSHA1(key);
            var hash = hmac.ComputeHash(counter);
            var offset = hash[hash.Length - 1] & 0xf;
            var truncatedHash = BitConverter.ToInt32(hash, offset) & 0x7fffffff;
            var otp = truncatedHash % (int)Math.Pow(10, OTPLength);
            return otp.ToString($"D{OTPLength}");
        }

        public static string GenerateOtpWithCheckSum()
        {
            var length = 5;
            using (var rng = RandomNumberGenerator.Create())
            {
                char[] digits = new char[length];
                byte[] data = new byte[length];

                rng.GetBytes(data);

                for (int i = 0; i < length; i++)
                {
                    digits[i] = (char)('0' + (data[i] % 10));
                }

                var otp = new string(digits);
                var sum = otp.Select(q => int.Parse(q.ToString())).Sum().ToString();

                return otp + sum.Last();
            }
        }
    }

    static class Base32Encoding
    {
        private const string base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        public static byte[] ToBytes(string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentNullException(nameof(input));

            input = input.TrimEnd('='); //remove padding characters
            int byteCount = input.Length * 5 / 8; //this must be an integer
            byte[] result = new byte[byteCount];

            byte curByte = 0, bitsRemaining = 8;
            int mask = 0;
            int arrayIndex = 0;
            foreach (char c in input)
            {
                int cValue = base32Chars.IndexOf(c);
                if (cValue < 0)
                    throw new ArgumentOutOfRangeException(nameof(input));

                if (bitsRemaining > 5)
                {
                    mask = cValue << bitsRemaining - 5;
                    curByte |= (byte)mask;
                    bitsRemaining -= 5;
                }
                else
                {
                    mask = cValue >> 5 - bitsRemaining;
                    curByte |= (byte)mask;
                    result[arrayIndex++] = curByte;
                    curByte = (byte)(cValue << 3 + bitsRemaining);
                    bitsRemaining += 3;
                }
            }
            //if we didn't end with a full byte
            if (bitsRemaining != 8)
            {
                result[arrayIndex++] = curByte;
            }

            return result;
        }
    }
}
