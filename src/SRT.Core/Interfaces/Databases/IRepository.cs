using System.Linq.Expressions;
using SRT.Core.Domain.DomainInterfaces;

namespace SRT.Core.Interfaces.Databases
{
    /// <summary>
    /// Provider-agnostic repository for a single entity type.
    /// <para>
    /// <see cref="AddOrUpdateAsync"/> with <c>autoSave: true</c> persists all pending changes
    /// on the shared context (not only this entity).
    /// </para>
    /// <para>
    /// <see cref="DeleteByFilterAsync"/> executes immediately against the store; it is not
    /// deferred by <c>autoSave</c>. Combine bulk and tracked work only inside a UoW transaction.
    /// </para>
    /// </summary>
    public interface IRepository<TEntity, TKey>
        where TEntity : class, IDatabaseEntry, IDatabaseEntryID<TKey>
    {
        Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);

        Task<TEntity> AddOrUpdateAsync(
            TEntity entity,
            IUserInfo? adminInfoChange = null,
            bool autoSave = true,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Stages all entities with internal <c>autoSave: false</c>, then a single Save at the end.
        /// Multiple new entities with default keys (e.g. int id == 0) are allowed.
        /// Duplicate non-default keys in the same range throw <see cref="ArgumentException"/>.
        /// </summary>
        Task AddOrUpdateRangeAsync(
            IEnumerable<TEntity> entities,
            IUserInfo? adminInfoChange = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Immediately deletes matching rows in the store (not deferred by autoSave).
        /// </summary>
        Task<int> DeleteByFilterAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default);
    }
}
