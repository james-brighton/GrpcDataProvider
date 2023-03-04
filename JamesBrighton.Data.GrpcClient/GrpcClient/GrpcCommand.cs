using System.Data;
using System.Diagnostics.CodeAnalysis;
using JamesBrighton.DataProvider.Grpc;
using Grpc.Net.Client;
using JamesBrighton.Data.Common;

namespace JamesBrighton.Data.GrpcClient;

/// <summary>
/// Represents a command to execute against a database.
/// </summary>
public class GrpcCommand : IAsyncDbCommand
{
    /// <summary>
    /// The items.
    /// </summary>
    readonly List<GrpcDataReader> items = new();
    /// <summary>
    /// The server side identifier of the command.
    /// </summary>
    string commandIdentifier = "";

    /// <summary>
    /// Initializes a new instance of the <see cref="GrpcCommand" /> class.
    /// </summary>
    public GrpcCommand() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="GrpcCommand" /> class.
    /// </summary>
    /// <param name="channel">gRPC channel to use.</param>
    /// <param name="connectionIdentifier">The connection identifier.</param>
    public GrpcCommand(GrpcChannel? channel, string connectionIdentifier)
    {
        Channel = channel;
        ConnectionIdentifier = connectionIdentifier;
    }

    /// <inheritdoc />
    [AllowNull]
    public string CommandText { get; set; } = "";

    /// <inheritdoc />
    public int CommandTimeout { get; set; }

    /// <inheritdoc />
    public CommandType CommandType { get; set; }

    /// <inheritdoc />
    public IDbConnection? Connection { get; set; }

    /// <inheritdoc />
    public IDataParameterCollection Parameters { get; } = new GrpcParameterCollection();

    /// <inheritdoc />
    public IDbTransaction? Transaction { get; set; }

    /// <inheritdoc />
    public UpdateRowSource UpdatedRowSource { get; set; }

    /// <summary>
    /// The gRPC channel.
    /// </summary>
    public GrpcChannel? Channel { get; set; }
    /// <summary>
    /// The connection identifier.
    /// </summary>
    public string? ConnectionIdentifier { get; set; }

    /// <inheritdoc />
    public void Cancel()
    {
    }

    /// <inheritdoc />
    public IDbDataParameter CreateParameter() => new GrpcParameter();

    /// <inheritdoc />
    public void Dispose()
    {
        for (var i = items.Count - 1; i >= 0; i--) items[i].Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (Channel == null || string.IsNullOrEmpty(ConnectionIdentifier) || string.IsNullOrEmpty(commandIdentifier))
            throw new InvalidOperationException("There's no gRPC channel.");

        var client = new DatabaseService.DatabaseServiceClient(Channel);

        await client.DestroyCommandAsync(new DestroyCommandRequest
        { ConnectionIdentifier = ConnectionIdentifier, CommandIdentifier = commandIdentifier });
        for (var i = items.Count - 1; i >= 0; i--) await items[i].DisposeAsync();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public int ExecuteNonQuery()
    {
        if (Channel == null || string.IsNullOrEmpty(ConnectionIdentifier) || string.IsNullOrEmpty(commandIdentifier))
            throw new InvalidOperationException("There's no gRPC channel.");

        var client = new DatabaseService.DatabaseServiceClient(Channel);

        if (Parameters is not GrpcParameterCollection parameters)
            throw new InvalidOperationException($"Parameters is not of type {nameof(GrpcParameterCollection)}.");
        if (Transaction is not GrpcTransaction transaction)
            throw new InvalidOperationException($"Transaction is not of type {nameof(GrpcTransaction)}.");
        var query = new ExecuteQueryRequest
        {
            Query = CommandText,
            ConnectionIdentifier = ConnectionIdentifier,
            TransactionIdentifier = transaction.TransactionIdentifier,
            CommandIdentifier = commandIdentifier
        };
        foreach (var p in parameters)
            query.Parameters.Add(new DataParameter { Name = p.ParameterName, Value = p.Value ?? new object() });
        var reply = client.ExecuteNonQuery(query);

        if (reply.DataException != null)
            GrpcDataException.ThrowDataException(reply.DataException);

        return reply.RowsAffected;
    }

    /// <inheritdoc />
    public IDataReader ExecuteReader() => ExecuteReader(CommandBehavior.Default);

    /// <inheritdoc />
    public IDataReader ExecuteReader(CommandBehavior behavior)
    {
        if (Channel == null || string.IsNullOrEmpty(ConnectionIdentifier) || string.IsNullOrEmpty(commandIdentifier))
            throw new InvalidOperationException("There's no gRPC channel.");

        var client = new DatabaseService.DatabaseServiceClient(Channel);

        if (Parameters is not GrpcParameterCollection parameters)
            throw new InvalidOperationException($"Parameters is not of type {nameof(GrpcParameterCollection)}.");
        if (Transaction is not GrpcTransaction transaction)
            throw new InvalidOperationException($"Transaction is not of type {nameof(GrpcTransaction)}.");
        var query = new ExecuteQueryRequest
        {
            Query = CommandText,
            ConnectionIdentifier = ConnectionIdentifier,
            TransactionIdentifier = transaction.TransactionIdentifier,
            CommandIdentifier = commandIdentifier
        };
        foreach (var p in parameters)
            query.Parameters.Add(new DataParameter { Name = p.ParameterName, Value = p.Value ?? new object() });
        var reply = client.ExecuteQuery(query, cancellationToken: CancellationToken.None);

        var result = new GrpcDataReader(reply);
        items.Add(result);
        return result;
    }

    /// <inheritdoc />
    public async Task<IAsyncDataReader> ExecuteReaderAsync(CancellationToken cancellationToken) =>
        await ExecuteReaderAsync(CommandBehavior.Default, CancellationToken.None);

    /// <inheritdoc />
    public async Task<IAsyncDataReader> ExecuteReaderAsync(CommandBehavior behavior,
        CancellationToken cancellationToken)
    {
        await Task.Delay(0, cancellationToken);
        if (ExecuteReader(behavior) is not IAsyncDataReader result)
            throw new InvalidOperationException("Reader is not of type IAsyncDataReader.");
        return result;
    }

    /// <inheritdoc />
    public async Task<IAsyncDataReader> ExecuteReaderAsync(CommandBehavior behavior) =>
        await ExecuteReaderAsync(behavior, CancellationToken.None);

    /// <inheritdoc />
    public async Task<IAsyncDataReader> ExecuteReaderAsync() => await ExecuteReaderAsync(CommandBehavior.Default);

    /// <inheritdoc />
    public async Task<object?> ExecuteScalarAsync()
    {
        return await ExecuteScalarAsync(CancellationToken.None);
    }

    /// <inheritdoc />
    public async Task<object?> ExecuteScalarAsync(CancellationToken cancellationToken)
    {
        var reader = await ExecuteReaderAsync(CommandBehavior.SingleResult, cancellationToken);
        if (!await reader.ReadAsync(cancellationToken) || reader.FieldCount == 0) return null;
        return reader[0];
    }

    /// <inheritdoc />
    public object? ExecuteScalar()
    {
        var reader = ExecuteReader(CommandBehavior.SingleResult);
        if (!reader.Read() || reader.FieldCount == 0) return null;
        return reader[0];
    }

    /// <inheritdoc />
    public void Prepare()
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="GrpcCommand"/> using the specified gRPC channel and database connection.
    /// </summary>
    /// <param name="channel">The gRPC channel to use for communication with the database service.</param>
    /// <param name="connectionIdentifier">The identifier of the database connection.</param>
    /// <param name="connection">The database connection to associate with the command.</param>
    /// <returns>A new instance of <see cref="GrpcCommand"/> with the specified properties.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="channel"/> is null or <paramref name="connectionIdentifier"/> is null or empty.</exception>
    public static GrpcCommand CreateCommand(GrpcChannel? channel, string connectionIdentifier, IDbConnection connection)
    {
        if (channel == null || string.IsNullOrEmpty(connectionIdentifier))
            throw new InvalidOperationException("There's no gRPC channel.");

        var result = new GrpcCommand(channel, connectionIdentifier) { Connection = connection };
        var client = new DatabaseService.DatabaseServiceClient(channel);

        var reply = client.CreateCommand(new CreateCommandRequest { ConnectionIdentifier = connectionIdentifier });
        result.commandIdentifier = reply.CommandIdentifier;
        return result;
    }

    /// <summary>
    /// Asynchronously creates a new instance of <see cref="GrpcCommand"/> using the specified gRPC channel and database connection.
    /// </summary>
    /// <param name="channel">The gRPC channel to use for communication with the database service.</param>
    /// <param name="connectionIdentifier">The identifier of the database connection.</param>
    /// <param name="connection">The database connection to associate with the command.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a new instance of <see cref="GrpcCommand"/> with the specified properties.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="channel"/> is null or <paramref name="connectionIdentifier"/> is null or empty.</exception>
    public static async Task<GrpcCommand> CreateCommandAsync(GrpcChannel? channel, string connectionIdentifier, IDbConnection connection)
    {
        if (channel == null || string.IsNullOrEmpty(connectionIdentifier))
            throw new InvalidOperationException("There's no gRPC channel.");

        var result = new GrpcCommand(channel, connectionIdentifier) { Connection = connection };
        var client = new DatabaseService.DatabaseServiceClient(channel);

        var reply = await client.CreateCommandAsync(new CreateCommandRequest
        { ConnectionIdentifier = connectionIdentifier });
        result.commandIdentifier = reply.CommandIdentifier;
        return result;
    }

    /// <inheritdoc />
    public async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
    {
        if (Channel == null || string.IsNullOrEmpty(ConnectionIdentifier) || string.IsNullOrEmpty(commandIdentifier))
            throw new InvalidOperationException("There's no gRPC channel.");

        var client = new DatabaseService.DatabaseServiceClient(Channel);

        if (Parameters is not GrpcParameterCollection parameters)
            throw new InvalidOperationException($"Parameters is not of type {nameof(GrpcParameterCollection)}.");
        if (Transaction is not GrpcTransaction transaction)
            throw new InvalidOperationException($"Transaction is not of type {nameof(GrpcTransaction)}.");
        var query = new ExecuteQueryRequest
        {
            Query = CommandText,
            ConnectionIdentifier = ConnectionIdentifier,
            TransactionIdentifier = transaction.TransactionIdentifier,
            CommandIdentifier = commandIdentifier
        };
        foreach (var p in parameters)
            query.Parameters.Add(new DataParameter { Name = p.ParameterName, Value = p.Value ?? new object() });
        var reply = await client.ExecuteNonQueryAsync(query, cancellationToken: cancellationToken);

        if (reply.DataException != null)
            GrpcDataException.ThrowDataException(reply.DataException);

        return reply.RowsAffected;
    }

    /// <inheritdoc />
    public async Task<int> ExecuteNonQueryAsync() => await ExecuteNonQueryAsync(CancellationToken.None);
}