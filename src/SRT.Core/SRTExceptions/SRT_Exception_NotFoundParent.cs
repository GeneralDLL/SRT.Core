// Ignore Spelling: SRT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRT.Core.SRTExceptions
{
    public class SRT_Exception_NotFoundParent : Exception
    {
        public SRT_Exception_NotFoundParent(string message) : base(message) { }
        public SRT_Exception_NotFoundParent() { }
    }
}
