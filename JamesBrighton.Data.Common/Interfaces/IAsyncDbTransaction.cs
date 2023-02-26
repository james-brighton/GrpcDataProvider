using System.Data;

namespace JamesBrighton.Data;

/// <summary>
/// Represents an asynchronous database transaction.
/// </summary>
/// <remarks>
/// This interface extends the <see cref="IDbTransaction" /> interface and adds support for asynchronous operations.
/// </remarks>
public interface IAsyncDbTransaction : IDbTransaction, IAsyncDisposable
{
    /// <summary>
    /// Commits the database transaction asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CommitAsync();

    /// <summary>
    /// Commits the database transaction asynchronously, using the specified <paramref name="cancellationToken" />.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to use.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CommitAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Rolls back the database transaction asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RollbackAsync();

    /// <summary>
    /// Rolls back the database transaction asynchronously, using the specified <paramref name="cancellationToken" />.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to use.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RollbackAsync(CancellationToken cancellationToken);
}