using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using JamesBrighton.DataProvider.Grpc;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using IsolationLevel = System.Data.IsolationLevel;

namespace JamesBrighton.Data.GrpcClient;

internal static class Channel
{
    public static GrpcChannel GetChannel(string address)
    {
        lock (lockObject)
        {
            if (dictionary.TryGetValue(address, out var v))
            {
                dictionary[address] = (v.Item1, v.Item2 + 1);
                return v.Item1;
            }
            else
            {
                var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler())
                {
                    HttpVersion = HttpVersion.Version11
                });
                var channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions
                {
                    HttpClient = httpClient
                });
                dictionary[address] = (channel, 1);
                return channel;
            }
        }
    }

    public static void Dispose(GrpcChannel channel)
    {
        lock (lockObject)
        {
            foreach (var item in dictionary)
            {
                if (item.Value.Item1 != channel)
                    continue;
                var count = item.Value.Item2 - 1;
                if (count > 0)
                {
                    dictionary[item.Key] = (channel, count);
                }
                else
                {
                    channel.Dispose();
                    dictionary.Remove(item.Key);
                }
                return;
            }
        }
    }

    static readonly Dictionary<string, (GrpcChannel, int)> dictionary = new();

    static readonly object lockObject = new();
}

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
		DisposeChannel();
	}

	/// <inheritdoc />
	public IDbCommand CreateCommand() => GrpcCommand.CreateCommand(channel, connectionIdentifier, this);

    /// <inheritdoc />
    public async Task<IAsyncDbCommand> CreateCommandAsync() => await GrpcCommand.CreateCommandAsync(channel, connectionIdentifier, this);

    /// <inheritdoc />
    public void Dispose()
    {
        Close();
        DisposeChannel();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await CloseAsync();
        DisposeChannel();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public void Open()
    {
        var connectionStringBuilder = new GrpcConnectionStringBuilder(ConnectionString);
        var address = connectionStringBuilder["GrpcServer"];
        channel = Channel.GetChannel(address);
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
        DisposeChannel();
    }

    /// <summary>
    /// Disposes the channel.
    /// </summary>
	void DisposeChannel()
	{
		if (channel == null)
			return;

		Channel.Dispose(channel);
		channel = null;
	}
}