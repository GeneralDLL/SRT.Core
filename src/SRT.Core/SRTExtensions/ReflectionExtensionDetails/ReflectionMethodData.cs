// Ignore Spelling: DTO SRT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SRT.Core.SRTExtensions.ReflectionExtensionDetails
{
    public class ReflectionMethodData
    {
        private object mainData;

        public ReflectionMethodData(object data)
        {
            mainData = data;
        }

        public List<MethodInfo> All
        {
            get
            {
                var type = mainData.GetType();
                var lst = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);

                return lst.ToList();
            }
        }
        public MethodInfo? GetMethodWithSpecialInput(object obj, string nameMethod, Type[] typesInput)
        {
            var type = obj.GetType();
            var lst = All.Where(q => q.Name == nameMethod).ToList();

            foreach (var item in lst)
            {
                var lst_parameters = item.GetParameters().ToList();
                if (typesInput.Count() != lst_parameters.Count)
                    continue;
                for (int i = 0; i < lst_parameters.Count; i++)
                {
                    if (lst_parameters[i].ParameterType.FullName != typesInput[i].FullName)
                        continue;
                }

                return item;
            }

            return null;
        }
        public MethodInfo GetMethodWithSpecialInput(object obj, string nameMethod)
        {
            var type = obj.GetType();
            var lst = All.Where(q => q.Name == nameMethod).ToList();

            return lst.FirstOrDefault()!;
        }
    }
}
