// Ignore Spelling: SRT Enums

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRT.Core.SRTExtensions.ReflectionExtensionDetails
{
    public enum ReflectionTypeProperty
    {
        [Description("Public & Private static & NonStatic")]
        FullCopy = 0,
        [Description("Public NonStatic")]
        Public = 1,
        [Description("Public static")]
        PublicStatic = 2,
        [Description("Public static & NonStatic")]
        PublicAll = 3,
        [Description("Private NonStatic")]
        Private = 4,
        [Description("Private static")]
        PrivateStatic = 5,
        [Description("Private static & NonStatic")]
        PrivateAll = 6,
        [Description("Public & Private static")]
        Static = 7,
        [Description("Public & Private NonStatic")]
        NonStatic = 8
    }
}
