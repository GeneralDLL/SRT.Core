// Ignore Spelling: DTO SRT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SRT.Core.SRTExtensions.ReflectionExtensionDetails
{
    public class ReflectionPropertyData
    {
        private object mainData;

        private bool IsStatic(PropertyInfo q)
        {
            try
            {
                bool? v = q.SetMethod?.IsStatic;
                if (v == null)
                    v = q.GetAccessors()[0] != null ? q.GetAccessors()[0].IsStatic : false;
                if (v == null)
                    v = false;

                return v.Value;
            }
            catch { return false; }
        }

        public ReflectionPropertyData(object data)
        {
            mainData = data;
        }

        ///With Test Complete
        #region Public
        /// <summary>
        /// Public & No Static
        /// </summary>
        public List<PropertyInfo> Public
        {
            get
            { return PublicAll.Where(q => !IsStatic(q)).ToList(); }
        }
        /// <summary>
        /// Just Public Static
        /// </summary>
        public List<PropertyInfo> PublicStatic
        {
            get
            { return PublicAll.Where(q => IsStatic(q)).ToList(); }
        }
        /// <summary>
        /// Public & Public Static
        /// </summary>
        public List<PropertyInfo> PublicAll
        {
            get
            {
                var type = mainData.GetType();
                var lstProperties = type.GetProperties();

                return lstProperties.ToList();
            }
        }
        #endregion

        ///Testing
        #region Private
        /// <summary>
        /// Private & No Static
        /// </summary>
        public List<PropertyInfo> Private
        {
            get
            { return PrivateAll.Where(q => !IsStatic(q)).ToList(); }
        }
        /// <summary>
        /// Just Private Static
        /// </summary>
        public List<PropertyInfo> PrivateStatic
        {
            get
            { return PrivateAll.Where(q => IsStatic(q)).ToList(); }
        }
        /// <summary>
        /// Private & Private Static
        /// </summary>
        public List<PropertyInfo> PrivateAll
        {
            get
            {
                var type = mainData.GetType();
                var lstProperties = type.GetProperties(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

                return lstProperties.ToList();
            }
        }
        #endregion

        /// <summary>
        /// Private & Public Static
        /// </summary>
        public List<PropertyInfo> Static
        {
            get
            { return All.Where(q => IsStatic(q)).ToList(); }
        }
        /// <summary>
        /// Private & Public Non Static
        /// </summary>
        public List<PropertyInfo> NonStatic
        {
            get
            { return All.Where(q => !IsStatic(q)).ToList(); }
        }

        public List<PropertyInfo> All
        {
            get
            {
                var type = mainData.GetType();
                var lstProperties = type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);

                return lstProperties.ToList();
            }
        }
    }
}
