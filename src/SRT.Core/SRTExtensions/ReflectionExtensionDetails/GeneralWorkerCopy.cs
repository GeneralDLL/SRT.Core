// Ignore Spelling: SRT

using SRT.Core.SRTExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SRT.Core.SRTExtensions.ReflectionExtensionDetails
{
    class GeneralWorkerCopy
    {
        public static bool CopyClass(object Source, object Destination, ReflectionTypeProperty copyType = ReflectionTypeProperty.NonStatic, bool sameType = true)
        {
            try
            {
                if (sameType && Source.GetType() != Destination.GetType())
                    throw new Exception("The Type Is Not The Same");

                switch (copyType)
                {
                    case ReflectionTypeProperty.FullCopy:
                        AllCopy(Source, Destination);
                        break;
                    case ReflectionTypeProperty.Public:
                        PublicCopy(Source, Destination);
                        break;
                    case ReflectionTypeProperty.PublicStatic:
                        PublicStaticCopy(Source, Destination);
                        break;
                    case ReflectionTypeProperty.PublicAll:
                        PublicAllCopy(Source, Destination);
                        break;
                    case ReflectionTypeProperty.Private:
                        PrivateCopy(Source, Destination);
                        break;
                    case ReflectionTypeProperty.PrivateStatic:
                        PrivateStaticCopy(Source, Destination);
                        break;
                    case ReflectionTypeProperty.PrivateAll:
                        PrivateAllCopy(Source, Destination);
                        break;
                    case ReflectionTypeProperty.Static:
                        StaticCopy(Source, Destination);
                        break;
                    case ReflectionTypeProperty.NonStatic:
                        NonStaticCopy(Source, Destination);
                        break;
                    default:
                        throw new Exception("Out Of Range");
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Can Not Copy", ex);
            }
        }

        private static void Copy(object source, List<PropertyInfo> pSource, object destination, List<PropertyInfo> pDestination)
        {
            foreach (var item in pSource)
            {
                if (!item.CanRead || !item.CanWrite)
                    continue;
                var value = item.GetValue(source);
                var dm = pDestination.FirstOrDefault(q => q.Name == item.Name);
                if (dm != null)
                    dm.SetValue(destination, value);
            }
        }

        private static void PublicCopy(object source, object destination)
        {
            var pSource = source.SRT_GetReflectionData().Properties.Public;
            var pDestination = destination.SRT_GetReflectionData().Properties.Public;
            Copy(source, pSource, destination, pDestination);
        }
        private static void PublicAllCopy(object source, object destination)
        {
            var pSource = source.SRT_GetReflectionData().Properties.PublicAll;
            var pDestination = destination.SRT_GetReflectionData().Properties.PublicAll;
            Copy(source, pSource, destination, pDestination);
        }
        private static void PublicStaticCopy(object source, object destination)
        {
            var pSource = source.SRT_GetReflectionData().Properties.PublicStatic;
            var pDestination = destination.SRT_GetReflectionData().Properties.PublicStatic;
            Copy(source, pSource, destination, pDestination);
        }

        private static void PrivateCopy(object source, object destination)
        {
            var pSource = source.SRT_GetReflectionData().Properties.Private;
            var pDestination = destination.SRT_GetReflectionData().Properties.Private;
            Copy(source, pSource, destination, pDestination);
        }
        private static void PrivateAllCopy(object source, object destination)
        {
            var pSource = source.SRT_GetReflectionData().Properties.PrivateAll;
            var pDestination = destination.SRT_GetReflectionData().Properties.PrivateAll;
            Copy(source, pSource, destination, pDestination);
        }

        private static void PrivateStaticCopy(object source, object destination)
        {
            var pSource = source.SRT_GetReflectionData().Properties.PrivateStatic;
            var pDestination = destination.SRT_GetReflectionData().Properties.PrivateStatic;
            Copy(source, pSource, destination, pDestination);
        }

        private static void StaticCopy(object source, object destination)
        {
            var pSource = source.SRT_GetReflectionData().Properties.Static;
            var pDestination = destination.SRT_GetReflectionData().Properties.Static;
            Copy(source, pSource, destination, pDestination);
        }
        private static void NonStaticCopy(object source, object destination)
        {
            var pSource = source.SRT_GetReflectionData().Properties.NonStatic;
            var pDestination = destination.SRT_GetReflectionData().Properties.NonStatic;
            Copy(source, pSource, destination, pDestination);
        }

        private static void AllCopy(object source, object destination)
        {
            var pSource = source.SRT_GetReflectionData().Properties.All;
            var pDestination = destination.SRT_GetReflectionData().Properties.All;
            Copy(source, pSource, destination, pDestination);
        }
    }
}
