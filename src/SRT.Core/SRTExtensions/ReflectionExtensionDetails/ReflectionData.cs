// Ignore Spelling: DTO SRT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRT.Core.SRTExtensions.ReflectionExtensionDetails
{
    public class ReflectionData
    {
        private object mainData;

        public ReflectionData(object data)
        {
            mainData = data;
        }

        public ReflectionFieldData Fields
        { get { return new ReflectionFieldData(mainData); } }
        public ReflectionPropertyData Properties
        { get { return new ReflectionPropertyData(mainData); } }
        public ReflectionConstructorData Constructor
        { get { return new ReflectionConstructorData(mainData); } }
        public ReflectionMethodData Method
        { get { return new ReflectionMethodData(mainData); } }
    }
}
