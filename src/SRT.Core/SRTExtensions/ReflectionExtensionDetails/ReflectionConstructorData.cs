// Ignore Spelling: DTO SRT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SRT.Core.SRTExtensions.ReflectionExtensionDetails
{
    public class ReflectionConstructorData
    {
        private object mainData;

        #region Constructors
        public ReflectionConstructorData(object data)
        {
            mainData = data;
        }
        #endregion

        public List<ConstructorInfo> All
        {
            get
            {
                var type = mainData.GetType();
                var lst = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);

                return lst.ToList();
            }
        }

        public ConstructorInfo GetConstructorWithSpecialInput(object obj, Type[] typesInput)
        {
            var type = obj.GetType();
            return type.GetConstructor(typesInput)!;
        }
    }
}
