// Ignore Spelling: DTO SRT

using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRT.Core.Domain.BaseDomains
{
    public abstract class SearchResult<T>
    {
        protected SearchResult(SearchInput input)
        {
            pageIndex = input.pageIndex;
            pageSize = input.pageSize;
            orderByFieldName = input.orderByFieldName;
            OrderByDescending = input.OrderByDescending;
        }

        public bool OrderByDescending { get; set; } = false;
        public string orderByFieldName { get; set; } = "id";

        public int pageIndex { get; set; }
        public int pageSize { get; set; }

        public int totalPage
        {
            get
            {
                var t = count / pageSize;
                if (t * pageSize < count)
                    t++;

                return t;
            }
            set { }
        }

        public int count { get; set; }
        public List<T> lstData { get; set; } = new List<T>();

        public virtual List<TOut> GetShowData<TOut>(Func<T, TOut> func)
        {
            return lstData.Select(x => func(x)).ToList();
        }

        public override string ToString()
        {
            return $"Count: {count} - Type: {typeof(T)}";
        }
    }
}