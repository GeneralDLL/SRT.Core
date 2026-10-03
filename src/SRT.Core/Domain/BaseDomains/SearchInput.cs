// Ignore Spelling: DTO SRT

using System.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SRT.Core.Domain.BaseDomains
{
    public abstract class SearchInput
    {
        public DateTime? date_Start { get; set; } = null;
        public DateTime? date_End { get; set; } = null;

        public bool? isActive { get; set; } = null;

        public bool OrderByDescending { get; set; } = false;
        public string orderByFieldName { get; set; } = "id";

        [JsonIgnore]
        private int _pageIndex = 1;
        public int pageIndex
        {
            get => _pageIndex;
            set => _pageIndex = value <= 0 ? 1 : value;
        }

        [JsonIgnore]
        private int _pageSize = 20;
        public int pageSize
        {
            get => _pageSize;
            set => _pageSize = value <= 0 ? 1 : value;
        }
    }
}