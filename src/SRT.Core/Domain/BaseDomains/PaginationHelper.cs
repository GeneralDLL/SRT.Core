namespace SRT.Core.Domain.BaseDomains
{
    /// <summary>
    /// Shared Skip/Take helper used by repositories and unit-of-work search pipelines.
    /// </summary>
    public static class PaginationHelper
    {
        public const int MaxPageSize = 500;

        public static void Validate(int pageIndex, int pageSize)
        {
            if (pageIndex < 1)
                throw new ArgumentOutOfRangeException(nameof(pageIndex), pageIndex, "pageIndex must be >= 1.");

            if (pageSize < 1)
                throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "pageSize must be >= 1.");

            if (pageSize > MaxPageSize)
                throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, $"pageSize must be <= {MaxPageSize}.");
        }

        public static void Validate(SearchInput search)
        {
            ArgumentNullException.ThrowIfNull(search);
            Validate(search.pageIndex, search.pageSize);
        }

        public static IQueryable<T> Paginate<T>(IQueryable<T> query, int pageIndex, int pageSize)
        {
            ArgumentNullException.ThrowIfNull(query);
            Validate(pageIndex, pageSize);
            return query.Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }

        public static IQueryable<T> Paginate<T>(IQueryable<T> query, SearchInput search)
        {
            ArgumentNullException.ThrowIfNull(search);
            return Paginate(query, search.pageIndex, search.pageSize);
        }
    }
}
