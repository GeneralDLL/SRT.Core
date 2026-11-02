using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SRT.Core.SystemEnums
{
    public enum TypeAuthenticationController
    {
        None = 0,
        [Description("ادمین")]
        Admin = 1,
        [Description("ادمین داور")]
        RefereeAdmin = 2,
        [Description("داور")]
        Referee = 3,
        [Description("کاربر")]
        Client = 4,
        Internal = 5,
        [Description("همراه اول")]
        Mci = 6,
        [Description("دواپس")]
        DevOps = 7
    }
}
