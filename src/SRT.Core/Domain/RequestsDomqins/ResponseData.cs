// Ignore Spelling: DTO SRT

using SRT.Core.SRTExceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace SRT.Core.Domain.RequestsDomqins
{
    public class ResponseData
    {
        public HttpStatusCode status { get; set; } = HttpStatusCode.OK;
        public object? data { get; set; } = null;
        public string? redirectURL { get; set; } = null;
        public string? message { get; set; } = null;
        public string? errorCode { get; set; } = null;

        #region Get Data
        public string? GetData()
        { return data?.ToString(); }
        public T? GetData<T>()
        {
            try
            {
                if (data == null)
                    return default(T);

                return JsonConvert.DeserializeObject<T>(GetData() ?? "");
            }
            catch
            {
                if (data == null)
                    return default(T);

                return (T)data;
            }
        }

        public List<T>? GetDataList<T>()
        {
            if (data == null)
                return null;

            return JsonConvert.DeserializeObject<List<T>>(GetData() ?? "");
        }
        #endregion

        #region Generate Error
        internal Exception GenerateError()
        {
            return new SRT_Exception_ErrorOnResponseRequest("مشکل در ارسال و دریافت درخواست!",
                       new Exception(status.ToString(),
                       new Exception(errorCode,
                       new Exception(message))),
                       this);
        }
        #endregion
    }
}