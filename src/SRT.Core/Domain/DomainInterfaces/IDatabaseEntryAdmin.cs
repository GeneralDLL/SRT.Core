using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRT.Core.Domain.DomainInterfaces
{
    public interface IDatabaseEntryAdmin
    {
        int? id_AdminChange { get; set; }
        string? fullName_AdminChange { get; set; }
    }
}
