namespace SRT.Core.Interfaces.Cache
{
    public interface ICacheRepository
    {
        Task AddNewData(string category, string key, object value, DateTime expire);
        Task AddPersistentData(string category, string key, object value);
        Task<object?> GetData(string category, string key);
        Task<T?> GetData<T>(string category, string key) where T : class, new();
        Task<bool> RemoveData(string category, string key);
        Task<long> ClearCategory(string category);
        Task<long> GetCounterAsync(string category, string key);
        Task<long> IncrementWithExpiryAsync(string category, string key, TimeSpan ttl);
        Task<(bool Allowed, long Count)> TryIncrementIfBelowLimitAsync(string category, string key, long maxCount, TimeSpan ttl);
        Task<long> GetAccountBackoffRemainingSecondsAsync(string category, string key, long nowUnixSeconds);
        Task ApplyAccountBackoffAsync(string category, string key, long nowUnixSeconds, TimeSpan stateTtl, int backoffBaseSeconds, int backoffMaxSeconds);
    }

    public static class CacheServiceKeys
    {
        public const string Sql = "sql";
    }

    public interface ICacheSqlPersistence
    {
        bool IsAvailable { get; }
    }
}
