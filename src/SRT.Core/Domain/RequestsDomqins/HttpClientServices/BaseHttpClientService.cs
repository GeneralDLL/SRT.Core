// Ignore Spelling: jwt srt

using Newtonsoft.Json;
using SRT.Core.SRTAttributes.HttpServices;
using SRT.Core.SRTExtensions;
using System;

namespace SRT.Core.Domain.RequestsDomqins.HttpClientServices
{
    public abstract class BaseHttpClientService
    {
        private readonly string _jwtToken = string.Empty;
        protected readonly BaseAPIRequest APIRequest;

        #region Constructors
        public BaseHttpClientService(BaseAPIRequest request, string _jwtToken = "")
        {
            APIRequest = request ?? throw new ArgumentNullException(nameof(request));
            this._jwtToken = _jwtToken;
        }
        #endregion

        #region Main Structure For Send Data
        public async Task<HttpResponseMessage> SendRequestAsync()
        {
            try
            {
                var _client = APIRequest.client;
                string requestUri = GetURL();
                object content = APIRequest.FormDataContent ?? APIRequest.Data!;

                #region Check Gateway Structure
                if (!_client.BaseAddress!.LocalPath.SRT_String_IsNullOrEmpty() &&
                     _client.BaseAddress.AbsoluteUri.IndexOf("/api/") > 0)
                {
                    //TODO:Must Get Port Of Gayeway From AppSettings Or Configurations
                    var l = new List<string>()
                    {
                        "backed",
                        "backend",
                        ":8999",
                        ":9999"
                    };

                    var host = _client.BaseAddress.Authority.ToLower();
                    if (l.Any(q => host.Contains(q)))
                    {
                        requestUri = _client.BaseAddress.LocalPath + "/" + requestUri;
                        _client.BaseAddress = new Uri(_client.BaseAddress.AbsoluteUri.Replace(_client.BaseAddress.LocalPath, ""));
                    }
                }
                #endregion

                var request = new HttpRequestMessage(
                    new HttpMethod(APIRequest.requestType.SRT_Enum_GetDescription()),
                    requestUri);

                #region Authentications
                if (!_jwtToken.SRT_String_IsNullOrEmpty())
                {
                    if (_client.BaseAddress.OriginalString.IndexOf("https://metrichand.com") >= 0)
                        request.Headers.Add("X-Authorization", $"{_jwtToken}");
                    else
                        request.Headers.Add("Authorization", $"Bearer {_jwtToken}");
                }
                else if (!APIRequest.token.SRT_String_IsNullOrEmpty())
                {
                    if (_client.BaseAddress.OriginalString.IndexOf("https://metrichand.com") >= 0)
                        request.Headers.Add("X-Authorization", $"{APIRequest.token}");
                    else
                        request.Headers.Add("Authorization", $"Bearer {APIRequest.token}");
                }
                #endregion

                #region Headers
                if (APIRequest.headers != null)
                {
                    foreach (var header in APIRequest.headers)
                    { request.Headers.Add(header.Key, header.Value); }
                }
                #endregion

                #region Content
                if (content != null)
                {
                    if (content.GetType() == typeof(MultipartFormDataContent))
                        request.Content = content as MultipartFormDataContent;
                    else
                        request.Content = new StringContent(JsonConvert.SerializeObject(content), System.Text.Encoding.UTF8, "application/json");
                }
                #endregion

                APIResult_Http = await _client.SendAsync(request);
                return APIResult_Http;
            }
            catch (Exception ex)
            { throw new Exception("SendRequestAsync", ex); }
        }
        #endregion

        #region Get URL
        private string GetURL()
        {
            var lstProperties = this.SRT_GetPropertiesData()
                                    .Where(q => q.GetCustomAttributes(inherit: false).Any(r => r.GetType() == typeof(HttpServices_InQueryAttribute)))
                                    .ToList();

            if (lstProperties.Count == 0) return APIRequest.baseURL;

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

            return APIRequest.baseURL + v;
        }
        #endregion

        public abstract Task SendRequest();

        public HttpResponseMessage? APIResult_Http { get; private set; }
        public ResponseData APIResult_ResponseData =>
            APIResult_Http is not null &&
            APIResult_Http.Content is not null
                ? JsonConvert.DeserializeObject<ResponseData>(APIResult_Http.Content.ReadAsStringAsync().Result)!
                : APIResult_Http is not null
                    ? new ResponseData() { status = APIResult_Http.StatusCode, message = APIResult_Http.RequestMessage?.Content?.ToString() ?? APIResult_Http.Content?.ToString() ?? "Request has Error!!!" }
                    : new ResponseData() { status = System.Net.HttpStatusCode.NotExtended, message = "Request Not Send!!!" };

        public bool IsSuccess => APIResult_Http is not null && APIResult_Http.IsSuccessStatusCode;

        public bool EnsureSuccessStatusCode() =>
            IsSuccess
                ? true
                : throw new Exception($"Request failed with status code: {APIResult_ResponseData.status}, Message: {APIResult_ResponseData.message ?? "No message available"}");

        public override string ToString()
        {
            return $"{IsSuccess} - {APIResult_ResponseData.status}";
        }
    }
}
