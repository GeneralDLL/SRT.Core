// Ignore Spelling: DTO SRT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SRT.Core.SRTExtensions.ReflectionExtensionDetails
{
    public class ReflectionFieldData
    {
        private object mainData;

        private bool IsStatic(FieldInfo q)
        {
            try
            {
                bool? v = q.IsStatic;
                if (v == null)
                    v = false;

                return v.Value;
            }
            catch { return false; }
        }

        public ReflectionFieldData(object data)
        { mainData = data; }

        ///With Test Complete
        #region Public
        /// <summary>
        /// Public & No Static
        /// </summary>
        public List<FieldInfo> Public
        {
            get
            { return PublicAll.Where(q => !IsStatic(q)).ToList(); }
        }
        /// <summary>
        /// Just Public Static
        /// </summary>
        public List<FieldInfo> PublicStatic
        {
            get
            { return PublicAll.Where(q => IsStatic(q)).ToList(); }
        }
        /// <summary>
        /// Public & Public Static
        /// </summary>
        public List<FieldInfo> PublicAll
        {
            get
            {
                var type = mainData.GetType();
                var lst = type.GetFields();

                return lst.ToList();
            }
        }
        #endregion

        ///Testing
        #region Private
        /// <summary>
        /// Private & No Static
        /// </summary>
        public List<FieldInfo> Private
        {
            get
            { return PrivateAll.Where(q => !IsStatic(q)).ToList(); }
        }
        /// <summary>
        /// Just Private Static
        /// </summary>
        public List<FieldInfo> PrivateStatic
        {
            get
            { return PrivateAll.Where(q => IsStatic(q)).ToList(); }
        }
        /// <summary>
        /// Private & Private Static
        /// </summary>
        public List<FieldInfo> PrivateAll
        {
            get
            {
                var type = mainData.GetType();
                var lst = type.GetFields(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

                return lst.ToList();
            }
        }
        #endregion

        /// <summary>
        /// Private & Public Static
        /// </summary>
        public List<FieldInfo> Static
        {
            get
            { return All.Where(q => IsStatic(q)).ToList(); }
        }
        /// <summary>
        /// Private & Public Non Static
        /// </summary>
        public List<FieldInfo> NonStatic
        {
            get
            { return All.Where(q => !IsStatic(q)).ToList(); }
        }

        public List<FieldInfo> All
        {
            get
            {
                var type = mainData.GetType();
                var lst = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);

                return lst.ToList();
            }
        }
    }
}
