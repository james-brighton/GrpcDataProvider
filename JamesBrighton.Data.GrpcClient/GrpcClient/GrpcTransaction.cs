using System.Data;
using JamesBrighton.DataProvider.Grpc;
using Grpc.Net.Client;
using JamesBrighton.Data.Common.Helpers;
using IsolationLevel = System.Data.IsolationLevel;

namespace JamesBrighton.Data.GrpcClient;

/// <summary>
/// Represents a gRPC implementation of an <see cref="IAsyncDbTransaction" />.
/// </summary>
public class GrpcTransaction : IAsyncDbTransaction
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GrpcTransaction" /> class.
    /// </summary>
    public GrpcTransaction() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="GrpcTransaction" /> class with the specified connection and isolation
    /// level.
    /// </summary>
    /// <param name="channel">gRPC channel to use.</param>
    /// <param name="connectionIdentifier">The connection identifier.</param>
    /// <param name="connection">The connection associated with the transaction.</param>
    /// <param name="isolationLevel">The isolation level for the transaction.</param>
    public GrpcTransaction(GrpcChannel? channel, string connectionIdentifier, IDbConnection connection, IsolationLevel isolationLevel)
    {
        Channel = channel;
        ConnectionIdentifier = connectionIdentifier;
        Connection = connection;
        IsolationLevel = isolationLevel;
    }

    /// <summary>
    /// The server side identifier of the transaction.
    /// </summary>
    public string TransactionIdentifier { get; private set; } = "";

    /// <inheritdoc />
    public IDbConnection? Connection { get; }

    /// <inheritdoc />
    public IsolationLevel IsolationLevel { get; }
    /// <summary>
    /// The gRPC channel.
    /// </summary>
    public GrpcChannel? Channel { get; set; }
    /// <summary>
    /// The connection identifier.
    /// </summary>
    public string? ConnectionIdentifier { get; set; }

    /// <inheritdoc />
    public void Commit()
    {
        if (Channel == null || string.IsNullOrEmpty(ConnectionIdentifier))
            throw new InvalidOperationException("There's no gRPC channel.");

        var client = new DatabaseService.DatabaseServiceClient(Channel);

        var reply = client.CommitTransaction(new CommitTransactionRequest
        { ConnectionIdentifier = ConnectionIdentifier, TransactionIdentifier = TransactionIdentifier });
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
        if (Channel == null || string.IsNullOrEmpty(ConnectionIdentifier))
            throw new InvalidOperationException("There's no gRPC channel.");

        var client = new DatabaseService.DatabaseServiceClient(Channel);

        var reply = client.RollbackTransaction(new RollbackTransactionRequest
        { ConnectionIdentifier = ConnectionIdentifier, TransactionIdentifier = TransactionIdentifier });
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
        if (Channel == null || string.IsNullOrEmpty(ConnectionIdentifier))
            throw new InvalidOperationException("There's no gRPC channel.");

        var client = new DatabaseService.DatabaseServiceClient(Channel);

        var reply = await client.CommitTransactionAsync(
            new CommitTransactionRequest
            { ConnectionIdentifier = ConnectionIdentifier, TransactionIdentifier = TransactionIdentifier },
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
        if (Channel == null || string.IsNullOrEmpty(ConnectionIdentifier))
            throw new InvalidOperationException("There's no gRPC channel.");

        var client = new DatabaseService.DatabaseServiceClient(Channel);

        var reply = await client.RollbackTransactionAsync(
            new RollbackTransactionRequest
            { ConnectionIdentifier = ConnectionIdentifier, TransactionIdentifier = TransactionIdentifier },
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
        { ConnectionIdentifier = connectionIdentifier, IsolationLevel = IsolationLevelConverter.Convert(isolationLevel) });
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
        { ConnectionIdentifier = connectionIdentifier, IsolationLevel = IsolationLevelConverter.Convert(isolationLevel) });
        result.TransactionIdentifier = reply.TransactionIdentifier;
        return result;
    }
}