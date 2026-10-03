// Ignore Spelling: SRT

namespace SRT.Core.Domain.RequestsDomqins.HttpClientServices
{
    public abstract class BaseAPIRequest
    {
        #region Main Data
        public HttpClient client { get; set; }
        public abstract string baseURL { get; }
        public abstract RequestType requestType { get; }

        public Dictionary<string, string> headers { get; set; } = new();

        public abstract string token { get; }
        #endregion

        #region Data

        public virtual object? Data { get; set; }
        public virtual MultipartFormDataContent? FormDataContent { get; set; } = null;

        #endregion

        #region Constructors
        public BaseAPIRequest(HttpClient client)
        {
            this.client = client;
        }
        #endregion

        public override string ToString()
        {
            return $"{baseURL} - {requestType} - {token}";
        }
    }

    public abstract class BaseAPIRequest<TInput> : BaseAPIRequest
        where TInput : class, new()
    {
        #region Constructors
        protected BaseAPIRequest(HttpClient client) : base(client)
        { }
        #endregion

        public new virtual TInput? Data
        {
            get => base.Data as TInput;
            set => base.Data = value;
        }
    }
}