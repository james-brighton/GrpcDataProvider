using System.Data;
using System.Data.Common;

namespace JamesBrighton.Data.GrpcClient.Tunnel;

/// <summary>
/// Represents a tunneled gRPC implementation of an <see cref="IAsyncDbTransaction" />.
/// </summary>
public class TunnelTransaction : IAsyncDbTransaction
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TunnelTransaction" /> class.
    /// </summary>
    public TunnelTransaction() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="TunnelTransaction" /> class with the specified connection and isolation
    /// level.
    /// </summary>
    /// <param name="connection">The connection associated with the transaction.</param>
    /// <param name="isolationLevel">The isolation level for the transaction.</param>
    public TunnelTransaction(IDbConnection connection, IsolationLevel isolationLevel)
    {
        Connection = connection;
        IsolationLevel = isolationLevel;
    }

    /// <inheritdoc />
    public IDbConnection? Connection { get; }
    /// <summary>
    /// The transaction.
    /// </summary>
    public DbTransaction? Transaction { get; set; }

    /// <inheritdoc />
    public IsolationLevel IsolationLevel { get; }

    /// <inheritdoc />
    public void Commit()
    {
        if (Connection == null || Transaction == null)
            throw new InvalidOperationException("There's no connection.");

        Transaction.Commit();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Transaction?.Dispose();
        Transaction = null;
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await Transaction?.DisposeAsync().AsTask()!;
        Transaction = null;
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public void Rollback()
    {
        if (Connection == null || Transaction == null)
            throw new InvalidOperationException("There's no connection.");

        Transaction.Rollback();
    }

    /// <inheritdoc />
    public async Task CommitAsync()
    {
        await CommitAsync(CancellationToken.None);
    }

    /// <inheritdoc />
    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        if (Connection == null || Transaction == null)
            throw new InvalidOperationException("There's no connection.");

        await Transaction.CommitAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task RollbackAsync()
    {
        await RollbackAsync(CancellationToken.None);
    }

    /// <inheritdoc />
    public async Task RollbackAsync(CancellationToken cancellationToken)
    {
        if (Connection == null || Transaction == null)
            throw new InvalidOperationException("There's no connection.");

        await Transaction.RollbackAsync(cancellationToken);
    }

    /// <summary>
    /// Begins a new database transaction over gRPC.
    /// </summary>
    /// <param name="connection">The database connection to use.</param>
    /// <param name="isolationLevel">The isolation level to use for the transaction.</param>
    /// <returns>A new instance of GrpcTransaction.</returns>
    public static TunnelTransaction BeginTransaction(DbConnection connection, IsolationLevel isolationLevel)
    {
        return new TunnelTransaction(connection, isolationLevel)
        {
            Transaction = connection.BeginTransaction()
        };
    }

    /// <summary>
    /// Asynchronously begins a new database transaction over gRPC.
    /// </summary>
    /// <param name="connection">The database connection to use.</param>
    /// <param name="isolationLevel">The isolation level to use for the transaction.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a new instance of GrpcTransaction.</returns>
    public static async Task<TunnelTransaction> BeginTransactionAsync(DbConnection connection, IsolationLevel isolationLevel)
    {
        return new TunnelTransaction(connection, isolationLevel)
        {
            Transaction = await connection.BeginTransactionAsync()
        };
    }
}