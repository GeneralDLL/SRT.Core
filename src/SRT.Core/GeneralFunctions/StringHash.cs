// Ignore Spelling: DTO SRT

using SRT.Core.SRTExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SRT.Core.GeneralFunctions
{
    public class StringHash
    {
        public static string HashString(string inputString)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(inputString));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static List<string> SplitToList(string data, int maxLength = 3990)
        {
            if (data.SRT_String_IsNullOrEmpty())
                return new List<string>();

            List<string> lst = new List<string>();

            while (data.Length > maxLength)
            {
                lst.Add(data.Substring(0, maxLength));

                data.Remove(0, maxLength);
            }

            if (data.Length > 0)
            {
                lst.Add(data);
            }

            return lst;
        }

        public static bool VerifyHashedString(string inputString, string hashedString)
        {
            string newHashedString = HashString(inputString);
            return string.Equals(hashedString, newHashedString);
        }

        public static bool VerifyHashedString_HashFromClient(string inputString, string hashedString)
        {
            return string.Equals(inputString, hashedString);
        }
    }
}
