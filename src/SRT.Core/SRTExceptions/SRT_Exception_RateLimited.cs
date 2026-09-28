// Ignore Spelling: SRT

namespace SRT.Core.SRTExceptions
{
    public class SRT_Exception_RateLimited : Exception
    {
        public int? RetryAfterSeconds { get; }

        public SRT_Exception_RateLimited(string message, int? retryAfterSeconds = null) : base(message)
        {
            RetryAfterSeconds = retryAfterSeconds;
        }
    }
}
