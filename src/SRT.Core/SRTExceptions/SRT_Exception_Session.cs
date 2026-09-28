
using SRT.Core.SystemEnums;

namespace SRT.Core.SRTExceptions
{
    /// <summary>
    /// Session validation failed — map to HTTP 401 (not 404).
    /// </summary>
    public class SRT_Exception_Session : Exception
    {
        public SessionFailureReason Reason { get; }

        public SRT_Exception_Session(SessionFailureReason reason, string message)
            : base(message)
        {
            Reason = reason;
        }

        public SRT_Exception_Session(SessionFailureReason reason, string message, Exception inner)
            : base(message, inner)
        {
            Reason = reason;
        }
    }
}
