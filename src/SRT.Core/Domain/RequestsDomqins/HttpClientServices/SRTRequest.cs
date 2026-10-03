// Ignore Spelling: SRT

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SRT.Core.SRTExtensions;
using SRT.Core.SRTAttributes.HttpServices;

namespace SRT.Core.Domain.RequestsDomqins.HttpClientServices
{
    #region RequestType
    public enum RequestType
    {
        [Description("GET")]
        Get,
        [Description("POST")]
        Post,
        [Description("PATCH")]
        Patch,
        [Description("PUT")]
        Put,
        [Description("DELETE")]
        Delete,
    }
    #endregion

    public abstract class SRTRequest<TInput>
        where TInput : class, new()
    {
        #region Main Data
        protected BaseHttpClientServiceFullFunc client { get; set; }
        protected abstract string baseURL { get; }
        public virtual TInput Data { get; set; }
        protected abstract RequestType HTTPRequestType { get; }

        protected abstract string token { get; }
        protected virtual MultipartFormDataContent? FormDataContent { get; set; } = null;
        #endregion

        #region MainData Result
        public HttpResponseMessage? BaseResult { get; private set; } = null;
        #endregion

        #region Constructors
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public SRTRequest(HttpClient client)
        {
            this.client = new BaseHttpClientServiceFullFunc(client);
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        #endregion

        #region Internal
        internal HttpMethod GetHTTPMethod() => new HttpMethod(HTTPRequestType.SRT_Enum_GetDescription());

        internal string GetURL()
        {
            var lstProperties = this.SRT_GetPropertiesData()
                                    .Where(q => q.GetCustomAttributes(inherit: false).Any(r => r.GetType() == typeof(HttpServices_InQueryAttribute)))
                                    .ToList();

            if (lstProperties.Count == 0) return baseURL;

            var v = "";
            foreach (var item in lstProperties)
            {
                if (v.Length > 0)
                    v += "&";
                else
                    v = "?";

                var dm = (HttpServices_InQueryAttribute)item.GetCustomAttributes(inherit: false).First(r => r.GetType() == typeof(HttpServices_InQueryAttribute));
                v += $"{dm.name}={item.GetValue(this)}";
            }

            return baseURL + v;
        }

        internal Dictionary<string, string>? GetHeaders()
        {
            var lstProperties = this.SRT_GetPropertiesData()
                                    .Where(q => q.GetCustomAttributes(inherit: false).Any(r => r.GetType() == typeof(HttpServices_InHeaderAttribute)))
                                    .ToList();

            if (lstProperties.Count == 0)
                return null;

            var v = new Dictionary<string, string>();
            foreach (var item in lstProperties)
            {
                var dm = (HttpServices_InHeaderAttribute)item.GetCustomAttributes(inherit: false).First(r => r.GetType() == typeof(HttpServices_InHeaderAttribute));
                v.Add(dm.name, item.GetValue(this)?.ToString() ?? "");
            }

            return v;
        }

        internal object? GetContent()
        {
            if (Data is null) return null;

            return Data;
        }
        internal MultipartFormDataContent? GetFormData()
        {
            if (FormDataContent is null) return null;

            return FormDataContent;
        }
        internal string GetToken() => token;
        #endregion

        #region Base Send Request
        public async Task<HttpResponseMessage> SendRequest()
        {
            if (BaseResult is null)
                BaseResult = await client.SendRequestAsync(this);

            return BaseResult;
        }
        public void ResetRequest()
        {
            BaseResult = null;
        }
        #endregion

        #region Send Request
        public async Task<ResponseData> GetResponseData()
        {
            await SendRequest();

            try
            {
                var r = await BaseResult!.ReadContentAs<ResponseData>(checkStatusCodeOK: true);
                if (r is null)
                    throw new Exception();

                return r;
            }
            catch
            {
                try { return JsonConvert.DeserializeObject<ResponseData>(await BaseResult!.Content.ReadAsStringAsync())!; }
                catch
                {
                    return new ResponseData()
                    {
                        status = BaseResult!.StatusCode,
                        data = await BaseResult.Content.ReadAsStringAsync()
                    };
                }
            }
        }

        public async Task<TResult> GetResponseObject<TResult>()
        {
            var response = await GetResponseData();
            return response.GetData<TResult>()!;
        }
        public async Task<List<TResult>> GetResponseObjectList<TResult>()
        {
            var response = await GetResponseData();
            return response.GetDataList<TResult>()!;
        }
        public async Task<string> GetResponseString()
        {
            var response = await GetResponseData();
            return response.GetData()!;
        }
        #endregion

        public System.Net.HttpStatusCode GetStatusCode() => GetResponseData().Result.status;

        public async Task<bool> SendRequestCorrectly()
        {
            var response = await GetResponseData();
            if (BaseResult!.StatusCode == System.Net.HttpStatusCode.OK && response.status == System.Net.HttpStatusCode.OK)
                return true;

            throw response.GenerateError();
        }

        protected string base_ConvertDateToString(DateTime date)
        {
            return date.ToString("yyyy-MM-ddTHH-mm");
        }
    }
    public abstract class SRTRequest<TInput, TOut> : SRTRequest<TInput>
        where TInput : class, new()
        where TOut : class, new()
    {
        #region Get Result
        public TOut Result { get => GetResponseObject().Result; }
        #endregion

        #region Constructors
        public SRTRequest(HttpClient client) : base(client) { }
        #endregion

        #region Send Request
        public async Task<TOut> GetResponseObject() => await base.GetResponseObject<TOut>();
        public async Task<List<TOut>> GetResponseObjectList() => await base.GetResponseObjectList<TOut>();
        #endregion

        public async Task<TOut> GetResult()
        {
            var response = await GetResponseData();
            if (BaseResult!.StatusCode == System.Net.HttpStatusCode.OK && response.status == System.Net.HttpStatusCode.OK)
                return Result;

            throw response.GenerateError();
        }
    }

    #region Example
    public class SRTRequest : SRTRequest<object>
    {
        public SRTRequest(HttpClient client, string baseURL, RequestType httpRequestType = RequestType.Get) : base(client)
        { }

        protected override string baseURL { get => throw new NotImplementedException(); }
        protected override RequestType HTTPRequestType { get => throw new NotImplementedException(); }

        protected override string token => throw new NotImplementedException();
    }
    #endregion
}