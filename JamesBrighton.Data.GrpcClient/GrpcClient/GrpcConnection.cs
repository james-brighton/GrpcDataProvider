using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Brighton.James.Dataprovider.Grpc;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using IsolationLevel = System.Data.IsolationLevel;

namespace JamesBrighton.Data.GrpcClient;

/// <summary>
/// Represents a gRPC implementation of an <see cref="IDbConnection" />.
/// </summary>
public class GrpcConnection : IAsyncGrpcConnection
{
    /// <summary>
    /// Gets the gRPC channel associated with the connection.
    /// </summary>
    GrpcChannel? channel;

    /// <summary>
    /// The server side identifier of the connection.
    /// </summary>
    string connectionIdentifier = "";

    /// <summary>
    /// Gets or sets the provider invariant name at the server side.
    /// </summary>
    public string ServerProviderInvariantName { get; set; } = "";
    /// <summary>
    /// Gets or sets the string used to open a database at the server side.
    /// </summary>
    public string ServerConnectionString { get; set; } = "";

    /// <inheritdoc />
    [AllowNull]
    public string ConnectionString { get; set; } = "";

    /// <inheritdoc />
    public int ConnectionTimeout => 0;

    /// <inheritdoc />
    public string Database => "";

    /// <inheritdoc />
    public ConnectionState State => channel != null ? ConnectionState.Open : ConnectionState.Closed;

    /// <inheritdoc />
    public IDbTransaction BeginTransaction() => BeginTransaction(IsolationLevel.Unspecified);

    /// <inheritdoc />
    public IDbTransaction BeginTransaction(IsolationLevel il) => GrpcTransaction.BeginTransaction(channel, connectionIdentifier, this, il);

    /// <inheritdoc />
    public async Task<IAsyncDbTransaction> BeginTransactionAsync() => await BeginTransactionAsync(IsolationLevel.ReadCommitted);

    /// <inheritdoc />
    public async Task<IAsyncDbTransaction> BeginTransactionAsync(IsolationLevel il) => await GrpcTransaction.BeginTransactionAsync(channel, connectionIdentifier, this, il);

    /// <inheritdoc />
    public void ChangeDatabase(string databaseName)
    {
    }

    /// <inheritdoc />
    public void Close()
    {
        if (channel == null || string.IsNullOrEmpty(connectionIdentifier)) return;
        var client = new DatabaseService.DatabaseServiceClient(channel);
        client.CloseConnection(new CloseConnectionRequest { ConnectionIdentifier = connectionIdentifier });
        channel?.Dispose();
        channel = null;
    }

    /// <inheritdoc />
    public IDbCommand CreateCommand() => GrpcCommand.CreateCommand(channel, connectionIdentifier, this);

    /// <inheritdoc />
    public async Task<IAsyncDbCommand> CreateCommandAsync() => await GrpcCommand.CreateCommandAsync(channel, connectionIdentifier, this);

    /// <inheritdoc />
    public void Dispose()
    {
        // Trouble here?
        CloseAsync().Wait();
        channel?.Dispose();
        channel = null;
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await CloseAsync();
        channel?.Dispose();
        channel = null;
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public void Open()
    {
        var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()));
        channel = GrpcChannel.ForAddress(ConnectionString, new GrpcChannelOptions { HttpClient = httpClient });
        var client = new DatabaseService.DatabaseServiceClient(channel);
        var reply = client.OpenConnection(new OpenConnectionRequest { ProviderInvariantName = ServerProviderInvariantName, ConnectionString = ServerConnectionString });
        if (reply.DataException != null)
            GrpcDataException.ThrowDataException(reply.DataException);

        connectionIdentifier = reply.ConnectionIdentifier;
    }

    /// <inheritdoc />
    public async Task OpenAsync() => await OpenAsync(CancellationToken.None);

    /// <inheritdoc />
    public async Task OpenAsync(CancellationToken cancellationToken)
    {
        var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler())
        {
            HttpVersion = HttpVersion.Version11
        });
        var connectionStringBuilder = new GrpcConnectionStringBuilder(ConnectionString);
        channel = GrpcChannel.ForAddress(connectionStringBuilder["GrpcServer"], new GrpcChannelOptions
        {
            HttpClient = httpClient
        });
        var client = new DatabaseService.DatabaseServiceClient(channel);
        var reply = await client.OpenConnectionAsync(new OpenConnectionRequest { ProviderInvariantName = ServerProviderInvariantName, ConnectionString = ServerConnectionString }, cancellationToken: cancellationToken);
        if (reply.DataException != null)
            GrpcDataException.ThrowDataException(reply.DataException);

        connectionIdentifier = reply.ConnectionIdentifier;
    }

    /// <inheritdoc />
    public async Task CloseAsync() => await CloseAsync(CancellationToken.None);

    /// <inheritdoc />
    public async Task CloseAsync(CancellationToken cancellationToken)
    {
        if (channel == null || string.IsNullOrEmpty(connectionIdentifier)) return;
        var client = new DatabaseService.DatabaseServiceClient(channel);
        await client.CloseConnectionAsync(new CloseConnectionRequest { ConnectionIdentifier = connectionIdentifier }, cancellationToken: cancellationToken);
        channel?.Dispose();
        channel = null;
    }
}