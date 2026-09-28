// Ignore Spelling: SRT

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRT.Core.SRTExtensions
{
    public static class ObjectExtensions
    {
        public static string SRT_ToString(this object s)
        { return JsonConvert.SerializeObject(s); }

        #region Read Content
        public static async Task<T?> ReadContentAs<T>(this HttpResponseMessage response, bool checkStatusCodeOK = false)
        {
            if (checkStatusCodeOK && !response.IsSuccessStatusCode)
                throw new ApplicationException($"Something went wrong calling the API: {response.ReasonPhrase}");

            var dataAsString = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(dataAsString);
        }
        #endregion
    }
}
