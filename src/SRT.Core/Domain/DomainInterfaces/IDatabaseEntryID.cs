namespace SRT.Core.Domain.DomainInterfaces
{
    public interface IDatabaseEntryID<TKey>
    {
        TKey id { get; set; }
    }

    public interface IDatabaseEntryID : IDatabaseEntryID<int>
    {
    }
}
