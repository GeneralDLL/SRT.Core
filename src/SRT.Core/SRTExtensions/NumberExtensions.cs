// Ignore Spelling: SRT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRT.Core.SRTExtensions
{
    public static class NumberExtensions
    {       
        #region NumberToShow
        public static string SRT_Number_ToShow(this decimal data, int digit = 2)
        {
            var dataString = data.ToString().Split('.');

            var dig = "";
            if (dataString.Count() > 1)
                dig = "." + (dataString[1].Length < digit + 1 ? dataString[1] : dataString[1].Substring(0, digit));
            return ToStringNumber(dataString[0]) + dig;
        }
        private static string ToStringNumber(string data, string separate = ",")
        {
            try
            {
                var lstData = MySplit(data, 3, true);
                var ret = lstData.First();

                for (int i = 1; i < lstData.Count; i++)
                {
                    ret += separate + lstData[i];
                }

                return ret;
            }
            catch { return "0"; }
        }
        private static List<string> MySplit(string str, int chunkSize, bool fromEnd = false)
        {
            var lstRet = new List<string>();
            if (fromEnd)
            {
                var p0 = str.Length % chunkSize;
                if (p0 != 0)
                {
                    lstRet.Add(str.Substring(0, p0));
                    str = new string(str.Skip(p0).ToArray());
                }
            }
            while (!string.IsNullOrEmpty(str))
            {
                var d = new string(str.Take(chunkSize).ToArray());
                lstRet.Add(d);
                str = new string(str.Skip(chunkSize).ToArray());
            }
            return lstRet;
        }
#endregion

    }
}
