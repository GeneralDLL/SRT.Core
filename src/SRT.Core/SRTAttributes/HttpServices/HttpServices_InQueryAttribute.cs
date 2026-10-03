// Ignore Spelling: SRT

using System;
using System.Collections.Generic;
using System.Text;

namespace SRT.Core.SRTAttributes.HttpServices
{
    public class HttpServices_InQueryAttribute : Attribute
    {
        public HttpServices_InQueryAttribute(string name, int order)
        {
            this.name = name;
            this.order = order;
        }

        public string name { get; set; }
        public int order { get; set; }
    }
}