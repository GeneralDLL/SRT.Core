using System;
using System.Collections.Generic;
using System.Text;

using SRT.Core.SystemEnums;

namespace SRT.Core.Domain.DomainInterfaces
{
    public interface IUserInfo : IDatabaseEntryID
    {
        public string GetFullName();
        UserRoleType userRole { get; }
    }
}
