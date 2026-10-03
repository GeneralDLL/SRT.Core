namespace SRT.Core.Interfaces.Databases
{
    /// <summary>
    /// Provider-agnostic unit of work. Repositories that belong to one UoW must share
    /// the same underlying context instance and must not dispose it.
    /// </summary>
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Runs <paramref name="action"/>, then <see cref="SaveChangesAsync"/>, then commits.
        /// On error or cancellation: rolls back and rethrows.
        /// </summary>
        Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default);

        /// <summary>
        /// Runs <paramref name="func"/>, then <see cref="SaveChangesAsync"/>, then commits.
        /// On error or cancellation: rolls back and rethrows.
        /// </summary>
        Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> func, CancellationToken cancellationToken = default);
    }
}
