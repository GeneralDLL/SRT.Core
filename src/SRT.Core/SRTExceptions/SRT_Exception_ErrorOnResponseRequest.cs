// Ignore Spelling: SRT

using SRT.Core.Domain.RequestsDomqins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRT.Core.SRTExceptions
{
    public class SRT_Exception_ErrorOnResponseRequest : Exception
    {
        public SRT_Exception_ErrorOnResponseRequest(string message, Exception ex) : base(message, ex) { }
        public SRT_Exception_ErrorOnResponseRequest(string message, Exception ex, ResponseData response) : base(message, ex) { Response = response; }
        public SRT_Exception_ErrorOnResponseRequest(string message) : base(message)
        { }
        public SRT_Exception_ErrorOnResponseRequest() : base()
        { }

        public ResponseData? Response { get; set; }
    }
}
