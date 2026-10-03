// Ignore Spelling: SRT

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SRT.Core.SRTExtensions.ReflectionExtensionDetails;

namespace SRT.Core.SRTExtensions
{
    public static class ReflectionExtension
    {
        #region Get Reflection Data
        public static ReflectionData SRT_GetReflectionData(this object data)
        { return new ReflectionData(data); }
        #endregion

        #region Property Extension
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static List<PropertyInfo> SRT_GetPropertiesData(this object data, ReflectionTypeProperty propertyType = ReflectionTypeProperty.NonStatic)
        {
            switch (propertyType)
            {
                case ReflectionTypeProperty.FullCopy:
                    return data.SRT_GetReflectionData().Properties.All;
                case ReflectionTypeProperty.Public:
                    return data.SRT_GetReflectionData().Properties.Public;
                case ReflectionTypeProperty.PublicStatic:
                    return data.SRT_GetReflectionData().Properties.PublicStatic;
                case ReflectionTypeProperty.PublicAll:
                    return data.SRT_GetReflectionData().Properties.PublicAll;
                case ReflectionTypeProperty.Private:
                    return data.SRT_GetReflectionData().Properties.Private;
                case ReflectionTypeProperty.PrivateStatic:
                    return data.SRT_GetReflectionData().Properties.PrivateStatic;
                case ReflectionTypeProperty.PrivateAll:
                    return data.SRT_GetReflectionData().Properties.PrivateAll;
                case ReflectionTypeProperty.Static:
                    return data.SRT_GetReflectionData().Properties.Static;
                case ReflectionTypeProperty.NonStatic:
                    return data.SRT_GetReflectionData().Properties.NonStatic;
                default:
                    throw new Exception("Out Of Range");
            }
        }
        public static object? SRT_GetPropertyValue(this object data, string Name)
        {
            var lst = data.SRT_GetPropertiesData(ReflectionTypeProperty.FullCopy);
            var dm = lst.FirstOrDefault(q => q.Name == Name || q.Name.IndexOf($"<{Name}>") >= 0);
            if (dm == null)
                throw new Exception("Can Not Find Property => " + Name);

            return dm.GetValue(data);
        }
        public static PropertyInfo SRT_GetPropertyData(this object data, string Name)
        {
            var dm = data.SRT_GetPropertiesData(ReflectionTypeProperty.FullCopy).FirstOrDefault(q => q.Name == Name || q.Name.IndexOf($"<{Name}>") >= 0);
            if (dm == null)
                throw new Exception("Can Not Find Property => " + Name);

            return dm;
        }
        #endregion

        #region Field Extension
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static List<FieldInfo> SRT_GetFieldsData(this object data, ReflectionTypeProperty FieldType = ReflectionTypeProperty.NonStatic)
        {
            switch (FieldType)
            {
                case ReflectionTypeProperty.FullCopy:
                    return data.SRT_GetReflectionData().Fields.All;
                case ReflectionTypeProperty.Public:
                    return data.SRT_GetReflectionData().Fields.Public;
                case ReflectionTypeProperty.PublicStatic:
                    return data.SRT_GetReflectionData().Fields.PublicStatic;
                case ReflectionTypeProperty.PublicAll:
                    return data.SRT_GetReflectionData().Fields.PublicAll;
                case ReflectionTypeProperty.Private:
                    return data.SRT_GetReflectionData().Fields.Private;
                case ReflectionTypeProperty.PrivateStatic:
                    return data.SRT_GetReflectionData().Fields.PrivateStatic;
                case ReflectionTypeProperty.PrivateAll:
                    return data.SRT_GetReflectionData().Fields.PrivateAll;
                case ReflectionTypeProperty.Static:
                    return data.SRT_GetReflectionData().Fields.Static;
                case ReflectionTypeProperty.NonStatic:
                    return data.SRT_GetReflectionData().Fields.NonStatic;
                default:
                    throw new Exception("Out Of Range");
            }
        }
        public static object? SRT_GetFieldValue(this object data, string Name)
        {
            var dm = data.SRT_GetFieldsData(ReflectionTypeProperty.FullCopy).FirstOrDefault(q => q.Name == Name || q.Name.IndexOf($"<{Name}>") >= 0);
            if (dm == null)
                throw new Exception("Can Not Find Field => " + Name);

            return dm.GetValue(data);
        }
        public static FieldInfo SRT_GetFieldData(this object data, string Name)
        {
            var dm = data.SRT_GetFieldsData(ReflectionTypeProperty.FullCopy).FirstOrDefault(q => q.Name == Name || q.Name.IndexOf($"<{Name}>") >= 0);
            if (dm == null)
                throw new Exception("Can Not Find Field => " + Name);

            return dm;
        }
        #endregion

        #region Copy
        public static void SRT_CopyTo(this object data, object Destination, ReflectionTypeProperty copyType = ReflectionTypeProperty.NonStatic, bool sameType = true)
        { GeneralWorkerCopy.CopyClass(data, Destination, copyType, sameType); }
        public static void SRT_CopyFrom(this object data, object Source, ReflectionTypeProperty copyType = ReflectionTypeProperty.NonStatic, bool sameType = true)
        { GeneralWorkerCopy.CopyClass(Source, data, copyType, sameType); }
        #endregion

        #region New Instance
        public static T SRT_GetNewInstance<T>(this T data, object[] lst_input)
            where T : class
        {
            try
            {
                var cons = data.SRT_GetReflectionData()
                               .Constructor
                               .GetConstructorWithSpecialInput(data, lst_input.Select(q => q.GetType()).ToArray());

                object instance = cons.Invoke(lst_input);
                return (T)instance;
            }
            catch (Exception ex)
            { throw new Exception("Create New Instance => Type" + data.GetType(), ex); }
        }
        public static T SRT_GetNewInstance<T>(this T data)
            where T : class
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    // MaxDepth = maxDepth // Set the maximum depth for serialization
                };
                return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(data, settings))!;
            }
            catch (Exception exp)
            { throw new Exception("Create New Instance => Type" + data.GetType(), exp); }
        }
        public static TOut SRT_GetNewInstance<TOut>(this object data)
            where TOut : class
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    // MaxDepth = maxDepth // Set the maximum depth for serialization
                };
                return JsonConvert.DeserializeObject<TOut>(JsonConvert.SerializeObject(data, settings))!;
            }
            catch (Exception exp)
            { throw new Exception("Create New Instance => Type" + data.GetType(), exp); }
        }
        #endregion

        #region Do Method
        public static T SRT_DoMethod<T>(this object data, string nameMethod, object[]? lst_input = null)
        {
            lst_input = lst_input ?? new object[] { };
            var method = data.SRT_GetReflectionData()
                                      .Method
                                      .GetMethodWithSpecialInput(data, nameMethod, lst_input.Select(q => q.GetType()).ToArray())!;

            object instance = method.Invoke(data, lst_input)!;
            return (T)instance;
        }
        public static void SRT_DoMethod(this object data, string nameMethod, object[]? lst_input = null)
        {
            //var m = data.SRT_GetReflectionData()
            //                          .Method.All;
            lst_input = lst_input ?? new object[] { };
            var method = data.SRT_GetReflectionData()
                                      .Method
                                      .GetMethodWithSpecialInput(data, nameMethod, lst_input.Select(q => q.GetType()).ToArray())!;

            object instance = method.Invoke(data, lst_input)!;
        }
        #endregion

        #region Check Derived
        public static bool SRT_CheckDerived<T>(this object data, out T? outPut)
           where T : class
        {
            try
            {
                var assembly = Assembly.GetAssembly(data.GetType());
                var derivedType = typeof(T);
                var lst = assembly!
                               .GetTypes()
                               .Where(t => t != derivedType &&
                                           derivedType.IsAssignableFrom(t))
                               .ToList();

                if (lst.Count == 0)
                    throw new Exception();

                outPut = (T)data;
                return true;
            }
            catch
            {
                outPut = null;
                return false;
            }
        }
        #endregion
    }
}
