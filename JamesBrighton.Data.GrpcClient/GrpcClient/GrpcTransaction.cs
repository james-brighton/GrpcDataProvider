using System.Data;
using Database;
using Grpc.Net.Client;
using IsolationLevel = System.Data.IsolationLevel;

namespace JamesBrighton.Data.GrpcClient;

/// <summary>
/// Represents a gRPC implementation of an <see cref="IAsyncDbTransaction" />.
/// </summary>
public class GrpcTransaction : IAsyncDbTransaction
{
    /// <summary>
    /// The gRPC channel.
    /// </summary>
    readonly GrpcChannel? channel;

    /// <summary>
    /// The server side identifier of the connection.
    /// </summary>
    readonly string connectionIdentifier = "";

    /// <summary>
    /// Initializes a new instance of the <see cref="GrpcTransaction" /> class with the specified connection and isolation
    /// level.
    /// </summary>
    /// <param name="channel">gRPC channel to use.</param>
    /// <param name="connectionIdentifier">The connection identifier.</param>
    /// <param name="connection">The connection associated with the transaction.</param>
    /// <param name="isolationLevel">The isolation level for the transaction.</param>
    GrpcTransaction(GrpcChannel? channel, string connectionIdentifier, IDbConnection connection,
        IsolationLevel isolationLevel)
    {
        this.channel = channel;
        this.connectionIdentifier = connectionIdentifier;
        Connection = connection;
        IsolationLevel = isolationLevel;
    }

    /// <summary>
    /// The server side identifier of the transaction.
    /// </summary>
    public string TransactionIdentifier { get; private set; } = "";

    /// <inheritdoc />
    public IDbConnection Connection { get; }

    /// <inheritdoc />
    public IsolationLevel IsolationLevel { get; }

    /// <inheritdoc />
    public void Commit()
    {
        if (channel == null || string.IsNullOrEmpty(connectionIdentifier))
            throw new InvalidOperationException("There's no gRPC channel.");

        var client = new DatabaseService.DatabaseServiceClient(channel);

        var reply = client.CommitTransaction(new CommitTransactionRequest
        { ConnectionIdentifier = connectionIdentifier, TransactionIdentifier = TransactionIdentifier });
        if (reply.DataException != null)
            GrpcDataException.ThrowDataException(reply.DataException);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await Task.Delay(0);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public void Rollback()
    {
        if (channel == null || string.IsNullOrEmpty(connectionIdentifier))
            throw new InvalidOperationException("There's no gRPC channel.");

        var client = new DatabaseService.DatabaseServiceClient(channel);

        var reply = client.RollbackTransaction(new RollbackTransactionRequest
        { ConnectionIdentifier = connectionIdentifier, TransactionIdentifier = TransactionIdentifier });
        if (reply.DataException != null)
            GrpcDataException.ThrowDataException(reply.DataException);
    }

    /// <inheritdoc />
    public async Task CommitAsync()
    {
        await CommitAsync(CancellationToken.None);
    }

    /// <inheritdoc />
    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        if (channel == null || string.IsNullOrEmpty(connectionIdentifier))
            throw new InvalidOperationException("There's no gRPC channel.");

        var client = new DatabaseService.DatabaseServiceClient(channel);

        var reply = await client.CommitTransactionAsync(
            new CommitTransactionRequest
            { ConnectionIdentifier = connectionIdentifier, TransactionIdentifier = TransactionIdentifier },
            cancellationToken: cancellationToken);
        if (reply.DataException != null)
            GrpcDataException.ThrowDataException(reply.DataException);
    }

    /// <inheritdoc />
    public async Task RollbackAsync()
    {
        await RollbackAsync(CancellationToken.None);
    }

    /// <inheritdoc />
    public async Task RollbackAsync(CancellationToken cancellationToken)
    {
        if (channel == null || string.IsNullOrEmpty(connectionIdentifier))
            throw new InvalidOperationException("There's no gRPC channel.");

        var client = new DatabaseService.DatabaseServiceClient(channel);

        var reply = await client.RollbackTransactionAsync(
            new RollbackTransactionRequest
            { ConnectionIdentifier = connectionIdentifier, TransactionIdentifier = TransactionIdentifier },
            cancellationToken: cancellationToken);
        if (reply.DataException != null)
            GrpcDataException.ThrowDataException(reply.DataException);
    }

    /// <summary>
    /// Begins a new database transaction over gRPC.
    /// </summary>
    /// <param name="channel">The gRPC channel to use.</param>
    /// <param name="connectionIdentifier">The identifier of the connection to use.</param>
    /// <param name="connection">The database connection to use.</param>
    /// <param name="isolationLevel">The isolation level to use for the transaction.</param>
    /// <returns>A new instance of GrpcTransaction.</returns>
    public static GrpcTransaction BeginTransaction(GrpcChannel? channel, string connectionIdentifier, IDbConnection connection, IsolationLevel isolationLevel)
    {
        if (channel == null || string.IsNullOrEmpty(connectionIdentifier))
            throw new InvalidOperationException("There's no gRPC channel.");

        var result = new GrpcTransaction(channel, connectionIdentifier, connection, isolationLevel);
        var client = new DatabaseService.DatabaseServiceClient(channel);

        var reply = client.BeginTransaction(new BeginTransactionRequest
        { ConnectionIdentifier = connectionIdentifier, IsolationLevel = ToIsolationLevel(isolationLevel) });
        result.TransactionIdentifier = reply.TransactionIdentifier;
        return result;
    }

    /// <summary>
    /// Asynchronously begins a new database transaction over gRPC.
    /// </summary>
    /// <param name="channel">The gRPC channel to use.</param>
    /// <param name="connectionIdentifier">The identifier of the connection to use.</param>
    /// <param name="connection">The database connection to use.</param>
    /// <param name="isolationLevel">The isolation level to use for the transaction.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a new instance of GrpcTransaction.</returns>
    public static async Task<GrpcTransaction> BeginTransactionAsync(GrpcChannel? channel, string connectionIdentifier, IDbConnection connection, IsolationLevel isolationLevel)
    {
        if (channel == null || string.IsNullOrEmpty(connectionIdentifier))
            throw new InvalidOperationException("There's no gRPC channel.");

        var result = new GrpcTransaction(channel, connectionIdentifier, connection, isolationLevel);
        var client = new DatabaseService.DatabaseServiceClient(channel);

        var reply = await client.BeginTransactionAsync(new BeginTransactionRequest
        { ConnectionIdentifier = connectionIdentifier, IsolationLevel = ToIsolationLevel(isolationLevel) });
        result.TransactionIdentifier = reply.TransactionIdentifier;
        return result;
    }

    /// <summary>
    /// Converts the given isolation level to a gRPC friendly version.
    /// </summary>
    /// <param name="isolationLevel">Given isolation level.</param>
    /// <returns>The gRPC version.</returns>
    static Database.IsolationLevel ToIsolationLevel(IsolationLevel isolationLevel)
    {
        return isolationLevel switch
        {
            IsolationLevel.Unspecified => Database.IsolationLevel.Unspecified,
            IsolationLevel.Chaos => Database.IsolationLevel.Chaos,
            IsolationLevel.ReadUncommitted => Database.IsolationLevel.ReadUncommitted,
            IsolationLevel.ReadCommitted => Database.IsolationLevel.ReadCommitted,
            IsolationLevel.RepeatableRead => Database.IsolationLevel.RepeatableRead,
            IsolationLevel.Serializable => Database.IsolationLevel.Serializable,
            _ => Database.IsolationLevel.Snapshot
        };
    }
}