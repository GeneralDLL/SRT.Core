using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRT.Core.GeneralFunctions
{
    public class HTMLFileType
    {
        public static string GetMimeType(string? extension)
        {
            if(extension == null)
                throw new ArgumentNullException(nameof(extension));

            extension = extension.ToLower();

            return extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".html" => "text/html",
                ".htm" => "text/html",
                ".txt" => "text/plain",
                ".json" => "application/json",
                _ => "application/octet-stream"
            };
        }

    }
}
