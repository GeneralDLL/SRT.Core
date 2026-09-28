// Ignore Spelling: SRT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRT.Core.SRTExceptions
{
    public class SRT_Exception_NotFoundData : Exception
    {
        public SRT_Exception_NotFoundData(string message, Exception ex) : base(message, ex) { }

        public SRT_Exception_NotFoundData(string message) : base(message) { }
        public SRT_Exception_NotFoundData() { }
    }
}
