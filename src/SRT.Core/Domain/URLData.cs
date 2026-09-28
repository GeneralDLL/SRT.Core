// Ignore Spelling: DTO SRT SMS Metri Chand notificationservice

using System.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SRT.Core.SRTExtensions;

namespace SRT.Core.Domain
{
    public class URLData
    {
        private string _BaseURL = string.Empty;
        public string BaseURL
        {
            get => _BaseURL;
            set
            {
                _BaseURL = value;
                if (_BaseURL.SRT_String_IsNullOrEmpty())
                    return;

                if (_BaseURL.IndexOf("/api") < 0)
                {
                    CoreMatch = GenerateURL(CoreMatch, value, ":9001");
                    BaleBot = GenerateURL(BaleBot, value, ":9010");
                    Authentication = GenerateURL(Authentication, value, ":9000");
                    NotificationService = GenerateURL(NotificationService, value, ":9100");
                    FileManagement = GenerateURL(FileManagement, value, ":9900");
                    Chats = GenerateURL(Chats, value, ":9910");
                }
                else
                {
                    CoreMatch = GenerateURL(CoreMatch, value, "/CoreMatch");
                    BaleBot = GenerateURL(BaleBot, value, "/BaleBot");
                    Authentication = GenerateURL(Authentication, value, "/authentication");
                    NotificationService = GenerateURL(NotificationService, value, "/notificationservice");
                    FileManagement = GenerateURL(FileManagement, value, "/filemanagement");
                    Chats = GenerateURL(Chats, value, "/chats");
                }
            }
        }

        private string GenerateURL(string oldData, string value, string apiURL)
        {
            return oldData.SRT_String_IsNullOrEmpty() ? value + apiURL : oldData;
        }

        public string CoreMatch { get; set; } = string.Empty;
        public string AIArticle { get; set; } = string.Empty;
        public string BaleBot { get; set; } = string.Empty;
        public string ElasticManagement { get; set; } = string.Empty;
        public string FileManagement { get; set; } = string.Empty;
        public string Chats { get; set; } = string.Empty;

        public string Authentication { get; set; } = string.Empty;
        public string NotificationService { get; set; } = string.Empty;
    }
}
