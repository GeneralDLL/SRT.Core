// Ignore Spelling: SRT

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRT.Core.SRTExtensions
{
    public static class EnumExtensions
    {
        public static List<EnumItem<T>> SRT_Enum_GetItemList<T>(this T data, List<T>? lstRemove = null)
                where T : Enum
        {
            var enumItems = CreateEnumItems<T>();

            if (lstRemove != null)
            {
                var removedDescriptions = CreateEnumItems(lstRemove).Select(item => item.Description).ToHashSet();
                enumItems.RemoveAll(item => removedDescriptions.Contains(item.Description));
            }

            return enumItems;
        }
        public static EnumItem<T> SRT_Enum_GetItem<T>(this T data, List<T>? lstRemove = null)
              where T : Enum
        {
            var enumItems = data.SRT_Enum_GetItemList(lstRemove);

            // Find the first matching enum item based on the value
            var matchedItem = enumItems.FirstOrDefault(item => item.Value.Equals(data));

            // Handle case where no match is found
            return matchedItem ??
                   throw new ArgumentException($"No matching enum item found for {data}.", nameof(data));
        }

        public static string SRT_Enum_GetName<T>(this T data)
            where T : System.Enum
        {
            var dm = data.SRT_Enum_GetItem();

            return dm.Name;
        }
        public static string SRT_Enum_GetDescription<T>(this T data)
            where T : System.Enum
        {
            var dm = data.SRT_Enum_GetItem();

            return dm.Description;
        }

        #region Private
        private static List<EnumItem<T>> CreateEnumItems<T>(IEnumerable<T> enumValues)
           where T : Enum
        {
            return enumValues.Select(value => new EnumItem<T>
            {
                Value = value,
                Description = GetEnumDescription(value),
                Name = value.ToString()
            }).ToList();
        }
        private static List<EnumItem<T>> CreateEnumItems<T>()
            where T : Enum
        {
            var enumValues = Enum.GetValues(typeof(T)).Cast<T>();
            return CreateEnumItems(enumValues);
        }

        private static string GetEnumDescription<T>(T value) where T : Enum
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            var attribute = Attribute.GetCustomAttribute(fieldInfo!, typeof(DescriptionAttribute)) as DescriptionAttribute;
            return attribute?.Description ?? value.ToString();
        }
        #endregion
    }
}
