using SRT.Core.SRTExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRT.Core.SRTExtensions
{
    public class EnumItem<T>
            where T : System.Enum
    {
        public int Cost => Convert.ToInt32(Value);
        public required T Value { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
