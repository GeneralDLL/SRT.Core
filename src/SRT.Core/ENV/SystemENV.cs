// Ignore Spelling: ENV

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using SRT.Core.SRTExtensions;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SRT.Core.ENV
{
    public class SystemENV : ISystemENV
    {
        public IWebHostEnvironment _env { get; }

        public SystemENV(IWebHostEnvironment env)
        {
            _env = env;
        }

        public EnvironmentType environment { get => GetEnvironmentType(_env); }

        public enum EnvironmentType
        {
            LocalSRT,
            LocalRahimzadeh,
            LocalAkbarzadeh,
            LocalSajadi,
            LocalGholizadeh,
            LocalNajafi,
            Test,
            Development,
            Preview,
            Production,
            ProductionIIS
        }

        public OSPlatform GetOSPlatform()
        {
            OSPlatform osPlatform = (OSPlatform)Enum.Parse(typeof(OSPlatform), RuntimeInformation.OSDescription);
            return osPlatform;
        }
        public Architecture GetProcessArchitecture()
        { return RuntimeInformation.ProcessArchitecture; }
        public Architecture GetOSArchitecture()
        { return RuntimeInformation.OSArchitecture; }

        #region Get Environment Type - use In Constructors
        public static EnvironmentType GetEnvironmentType(IWebHostEnvironment _env)
        {
            var r = FixEnvironmentName(_env.EnvironmentName) switch
            {
                "ProductionIIS" => EnvironmentType.ProductionIIS,
                "Production" => EnvironmentType.Production,
                "Development" => EnvironmentType.Development,
                "Preview" => EnvironmentType.Preview,
                "LocalSRT" => EnvironmentType.LocalSRT,
                "LocalRahimzadeh" => EnvironmentType.LocalRahimzadeh,
                "LocalAkbarzadeh" => EnvironmentType.LocalAkbarzadeh,
                "LocalGholizadeh" => EnvironmentType.LocalGholizadeh,
                "LocalNajafi" => EnvironmentType.LocalNajafi,
                _ => EnvironmentType.Test,
            };

            if (r != EnvironmentType.Test)
                return r;

            if (_env.IsProduction())
                return EnvironmentType.Production;

            if (_env.IsDevelopment())
                return EnvironmentType.Development;

            if (_env.IsStaging())
                return EnvironmentType.Preview;

            return EnvironmentType.Test;
        }

        #region Normalize Environment
        public static string FixEnvironmentName(string environmentName)
        {
            string env = "Test";

            if (environmentName.ToLower() == "production" ||
                environmentName.ToLower() == "product")
                env = "Production";

            else if (environmentName.ToLower() == "productioniis" ||
                     environmentName.ToLower() == "productiis")
                env = "ProductionIIS";

            else if (environmentName.ToLower() == "development" ||
                     environmentName.ToLower() == "develop" ||
                     environmentName.ToLower() == "dev")
                env = "Development";

            else if (environmentName.ToLower() == "preview" ||
                     environmentName.ToLower() == "pre" ||
                     environmentName.ToLower() == "stage" ||
                     environmentName.ToLower() == "staging")
                env = "Preview";

            else if (environmentName.ToLower().Contains("srt"))
                env = "LocalSRT";
            else if (environmentName.ToLower().Contains("rahimzadeh"))
                env = "LocalRahimzadeh";
            else if (environmentName.ToLower().Contains("akbarzadeh"))
                env = "LocalAkbarzadeh";
            else if (environmentName.ToLower().Contains("gholizadeh"))
                env = "LocalGholizadeh";
            else if (environmentName.ToLower().Contains("najafi"))
                env = "LocalNajafi";

            return env;
        }
        #endregion

        #endregion

        public override string ToString()
        {
            return environment.SRT_Enum_GetDescription();
        }
    }
}