// Ignore Spelling: jwt srt

using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using SRT.Core.SRTExtensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SRT.Core.Domain.RequestsDomqins.HttpClientServices
{
    public class BaseHttpClientServiceFullFunc
    {
        private string _jwtToken = string.Empty;
        protected readonly HttpClient _client;

        #region Constructors
        public BaseHttpClientServiceFullFunc(HttpClient client, string _jwtToken = "")
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            this._jwtToken = _jwtToken;
        }
        #endregion

        #region Main Structure For Send Data
        public async Task<HttpResponseMessage> SendRequestAsync(
            HttpMethod method, 
            string requestUri, 
            Dictionary<string, string>? headers = null, 
            object? content = null)
        {
            try
            {
                var request = new HttpRequestMessage(method, requestUri);

                if (_jwtToken != null)
                {
                    if (_client.BaseAddress!.OriginalString.IndexOf("https://metrichand.com") >= 0)
                        request.Headers.Add("X-Authorization", $"{_jwtToken}");
                    else
                        request.Headers.Add("Authorization", $"Bearer {_jwtToken}");
                }

                if (headers != null)
                {
                    foreach (var header in headers)
                    { request.Headers.Add(header.Key, header.Value); }
                }

                if (content != null)
                {
                    if (content.GetType() == typeof(MultipartFormDataContent))
                        request.Content = content as MultipartFormDataContent;
                    else
                        request.Content = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");
                }

                return await _client.SendAsync(request);
            }
            catch (Exception ex)
            { throw new Exception("SendRequestAsync", ex); }
        }

        internal async Task<HttpResponseMessage> SendRequestAsync<T>(SRTRequest<T> request)
            where T : class, new()
        {
            try
            {
                _jwtToken = request.GetToken();
                var method = request.GetHTTPMethod();
                var url = request.GetURL();
                var header = request.GetHeaders();
                var content = request.GetFormData() ?? request.GetContent();

                #region Check Gateway Structure
                if (!_client.BaseAddress!.LocalPath.SRT_String_IsNullOrEmpty() &&
                     _client.BaseAddress.AbsoluteUri.IndexOf("/api/") > 0)
                {
                    var l = new List<string>()
                    {
                        "backed",
                        ":8999"
                    };

                    var host = _client.BaseAddress.Authority.ToLower();
                    if (l.Any(q => host.Contains(q)))
                    {
                        url = _client.BaseAddress.LocalPath + "/" + url;
                        _client.BaseAddress = new Uri(_client.BaseAddress.AbsoluteUri.Replace(_client.BaseAddress.LocalPath, ""));
                    }
                }
                #endregion

                return await SendRequestAsync(method, url, header, content);
            }
            catch (Exception ex)
            { throw new Exception("SendRequestAsync", ex); }
        }
        #endregion

        #region ResponseData Struct
        public async Task<ResponseData> SendRequestGetResponseData(
            HttpMethod method,
            string requestUri,
            Dictionary<string, string>? headers = null,
            object? content = null)
        {
            var response = await SendRequestAsync(method, requestUri, headers, content);

            try
            {
                var r = await response!.ReadContentAs<ResponseData>(checkStatusCodeOK: true);
                if (r is null)
                    throw new Exception();

                return r;
            }
            catch
            {
                try { return JsonConvert.DeserializeObject<ResponseData>(await response.Content.ReadAsStringAsync())!; }
                catch
                {
                    return new ResponseData()
                    {
                        status = response.StatusCode,
                        data = await response.Content.ReadAsStringAsync()
                    };
                }
            }
        }
        internal async Task<ResponseData> SendRequestGetResponseData<T>(SRTRequest<T> request)
            where T : class, new()
        {
            try
            {
                _jwtToken = request.GetToken();
                return await SendRequestGetResponseData(request.GetHTTPMethod(), request.GetURL(), request.GetHeaders(), request.GetContent());
            }
            catch (Exception ex)
            { throw new Exception("SendRequestGetResponseData", ex); }
        }

        public async Task<ResponseData> PostAsync_ResponseData(string requestUri, object content, Dictionary<string, string>? headers = null)
            => await SendRequestGetResponseData(HttpMethod.Post, requestUri, headers, content);
        public async Task<ResponseData> GetAsync_ResponseData(string requestUri, Dictionary<string, string>? headers = null)
            => await SendRequestGetResponseData(HttpMethod.Get, requestUri, headers);
        public async Task<ResponseData> PutAsync_ResponseData(string requestUri, object content, Dictionary<string, string>? headers = null)
            => await SendRequestGetResponseData(HttpMethod.Put, requestUri, headers, content);
        public async Task<ResponseData> PatchAsync_ResponseData(string requestUri, object content, Dictionary<string, string>? headers = null)
             => await SendRequestGetResponseData(new HttpMethod("PATCH"), requestUri, headers, content);
        public async Task<ResponseData> DeleteAsync_ResponseData(string requestUri, object? content = null, Dictionary<string, string>? headers = null)
            => await SendRequestGetResponseData(HttpMethod.Delete, requestUri, headers, content);
        #endregion

        #region T Struct
        public async Task<T?> GetAsync<T>(string requestUri, Dictionary<string, string>? headers = null)
        {
            var response = await GetAsync_ResponseData(requestUri, headers);
            return response.GetData<T>();
        }

        public async Task<T?> PostAsync<T>(string requestUri, object content, Dictionary<string, string>? headers = null)
        {
            var response = await PostAsync_ResponseData(requestUri, content, headers);
            return response.GetData<T>();
        }

        public async Task<T?> PutAsync<T>(string requestUri, object content, Dictionary<string, string>? headers = null)
        {
            var response = await PutAsync_ResponseData(requestUri, content, headers);
            return response.GetData<T>();
        }

        public async Task<T?> PatchAsync<T>(string requestUri, object content, Dictionary<string, string>? headers = null)
        {
            var response = await PatchAsync_ResponseData(requestUri, content, headers);
            return response.GetData<T>();
        }

        public async Task<T?> DeleteAsync<T>(string requestUri, object? content = null, Dictionary<string, string>? headers = null)
        {
            var response = await DeleteAsync_ResponseData(requestUri, content, headers);
            return response.GetData<T>();
        }
        #endregion

        #region String Struct
        public async Task<string?> GetAsync_String(string requestUri, Dictionary<string, string>? headers = null)
        {
            var response = await GetAsync_ResponseData(requestUri, headers);
            return response.GetData();
        }

        public async Task<string?> PostAsync_String(string requestUri, object content, Dictionary<string, string>? headers = null)
        {
            var response = await PostAsync_ResponseData(requestUri, content, headers);
            return response.GetData();
        }

        public async Task<string?> PutAsync_String(string requestUri, object content, Dictionary<string, string>? headers = null)
        {
            var response = await PutAsync_ResponseData(requestUri, content, headers);
            return response.GetData();
        }

        public async Task<string?> PatchAsync_String(string requestUri, object content, Dictionary<string, string>? headers = null)
        {
            var response = await PatchAsync_ResponseData(requestUri, content, headers);
            return response.GetData();
        }

        public async Task<string?> DeleteAsync_String(string requestUri, object? content = null, Dictionary<string, string>? headers = null)
        {
            var response = await DeleteAsync_ResponseData(requestUri, content, headers);
            return response.GetData();
        }
        #endregion

        #region Upload File
        public async Task<ResponseData> Upload_File(string filePath)
        {
            var url = "UploadFile";

            using (var form = new MultipartFormDataContent())
            {
                using (var stream = File.Open(filePath, FileMode.Open))
                {
                    var fileName = Path.GetFileName(filePath);
                    var fileContent = new StreamContent(stream);

                    fileContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(Path.GetExtension(filePath));

                    form.Add(fileContent, "file", fileName);

                    return await PostAsync_ResponseData(url, form);
                }
            }
        }
        #endregion
    }
}
