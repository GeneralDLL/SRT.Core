// Ignore Spelling: ENV

using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SRT.Core.ENV
{
    public interface ISystemENV
    {
        IWebHostEnvironment _env { get; }

        SystemENV.EnvironmentType environment { get; }

        OSPlatform GetOSPlatform();
        Architecture GetProcessArchitecture();
        Architecture GetOSArchitecture();
    }
}
