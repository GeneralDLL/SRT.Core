// Ignore Spelling: Deserialize deserializing بررسی نوع برای اطمینان از سازگاری معمولاً از این نوع‌اند خطاهای

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SRT.Core.GeneralFunctions
{
    public class XMLParser
    {
        public static T? DeserializeXML<T>(string xmlContent)
        {
            if (string.IsNullOrWhiteSpace(xmlContent))
                throw new ArgumentException("XML content cannot be null or empty.", nameof(xmlContent));

            try
            {
                var serializer = new XmlSerializer(typeof(T));

                using var stringReader = new StringReader(xmlContent);
                var result = serializer.Deserialize(stringReader);

                // بررسی نوع برای اطمینان از سازگاری
                if (result is T value)
                    return value;

                return default;
            }
            catch (InvalidOperationException ex)
            {
                // خطاهای XML Serialization معمولاً از این نوع‌اند
                throw new Exception($"❌ Error deserializing XML: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
        }
    }
}
