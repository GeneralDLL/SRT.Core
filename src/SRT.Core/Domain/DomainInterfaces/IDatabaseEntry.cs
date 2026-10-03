using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRT.Core.Domain.DomainInterfaces
{
    public interface IDatabaseEntry
    {
        DateTime date_Create { get; set; }
        DateTime date_Update { get; set; }
    }
}
