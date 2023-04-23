using System.Data;
using System.Diagnostics.CodeAnalysis;
using JamesBrighton.Data.GrpcClient.Common;
using JamesBrighton.DataProvider.Grpc;
using IsolationLevel = System.Data.IsolationLevel;

namespace JamesBrighton.Data.GrpcClient.Grpc;

/// <summary>
/// Represents a gRPC implementation of an <see cref="IDbConnection" />.
/// </summary>
public class GrpcConnection : IAsyncRemoteConnection
{
	/// <summary>
	/// The channel connection manager.
	/// </summary>
	ChannelManager? channelManager;

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
	public ConnectionState State => channelManager != null ? ConnectionState.Open : ConnectionState.Closed;

	/// <inheritdoc />
	public IDbTransaction BeginTransaction() => BeginTransaction(IsolationLevel.Unspecified);

	/// <inheritdoc />
	public IDbTransaction BeginTransaction(IsolationLevel il)
    {
		if (channelManager == null)
			throw new RemoteDataException("The channel manager is null.");
        return GrpcTransaction.BeginTransaction(channelManager.Channel, connectionIdentifier, this, il);
    }

    /// <inheritdoc />
    public async Task<IAsyncDbTransaction> BeginTransactionAsync() => await BeginTransactionAsync(IsolationLevel.ReadCommitted);

	/// <inheritdoc />
	public async Task<IAsyncDbTransaction> BeginTransactionAsync(IsolationLevel il)
    {
		if (channelManager == null)
			throw new RemoteDataException("The channel manager is null.");
        return await GrpcTransaction.BeginTransactionAsync(channelManager.Channel, connectionIdentifier, this, il);
    }

    /// <inheritdoc />
    public void ChangeDatabase(string databaseName)
	{
	}

	/// <inheritdoc />
	public void Close()
	{
		if (channelManager == null || string.IsNullOrEmpty(connectionIdentifier)) return;
		var client = new DatabaseService.DatabaseServiceClient(channelManager.Channel);
		client.CloseConnection(new CloseConnectionRequest { ConnectionIdentifier = connectionIdentifier });
		DisposeChannel();
	}

	/// <inheritdoc />
	public IDbCommand CreateCommand()
    {
		if (channelManager == null)
			throw new RemoteDataException("The channel manager is null.");
        return GrpcCommand.CreateCommand(channelManager.Channel, connectionIdentifier, this);
    }

    /// <inheritdoc />
    public async Task<IAsyncDbCommand> CreateCommandAsync()
    {
		if (channelManager == null)
			throw new RemoteDataException("The channel manager is null.");
        return await GrpcCommand.CreateCommandAsync(channelManager.Channel, connectionIdentifier, this);
    }

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
		if (string.IsNullOrEmpty(ConnectionString))
			throw new RemoteDataException("The connection string is empty.");
		if (string.IsNullOrEmpty(ServerConnectionString))
			throw new RemoteDataException("The server connection string is empty.");
		if (string.IsNullOrEmpty(ServerProviderInvariantName))
			throw new RemoteDataException("The server provider invariant name is empty.");
		var connectionStringBuilder = new ConnectionStringBuilder(ConnectionString);
		var address = connectionStringBuilder["GrpcServer"];
		var clientIdentifier = connectionStringBuilder["ClientIdentifier"];
		channelManager = new ChannelManager(address);
		var client = new DatabaseService.DatabaseServiceClient(channelManager.Channel);
		var reply = client.OpenConnection(new OpenConnectionRequest { ClientIdentifier = clientIdentifier, ProviderInvariantName = ServerProviderInvariantName, ConnectionString = ServerConnectionString });
		if (reply.DataException != null)
			RemoteDataException.ThrowDataException(reply.DataException);

		connectionIdentifier = reply.ConnectionIdentifier;
	}

	/// <inheritdoc />
	public async Task OpenAsync() => await OpenAsync(CancellationToken.None);

	/// <inheritdoc />
	public async Task OpenAsync(CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(ConnectionString))
			throw new RemoteDataException("The connection string is empty.");
		if (string.IsNullOrEmpty(ServerConnectionString))
			throw new RemoteDataException("The server connection string is empty.");
		if (string.IsNullOrEmpty(ServerProviderInvariantName))
			throw new RemoteDataException("The server provider invariant name is empty.");

		var connectionStringBuilder = new ConnectionStringBuilder(ConnectionString);
		var address = connectionStringBuilder["GrpcServer"];
		var clientIdentifier = connectionStringBuilder["ClientIdentifier"];
		channelManager = new ChannelManager(address);
		var client = new DatabaseService.DatabaseServiceClient(channelManager.Channel);
		var reply = await client.OpenConnectionAsync(new OpenConnectionRequest { ClientIdentifier = clientIdentifier, ProviderInvariantName = ServerProviderInvariantName, ConnectionString = ServerConnectionString }, cancellationToken: cancellationToken);
		if (reply.DataException != null)
			RemoteDataException.ThrowDataException(reply.DataException);

		connectionIdentifier = reply.ConnectionIdentifier;
	}

	/// <inheritdoc />
	public async Task CloseAsync() => await CloseAsync(CancellationToken.None);

	/// <inheritdoc />
	public async Task CloseAsync(CancellationToken cancellationToken)
	{
		if (channelManager == null || string.IsNullOrEmpty(connectionIdentifier)) return;
		var client = new DatabaseService.DatabaseServiceClient(channelManager.Channel);
		await client.CloseConnectionAsync(new CloseConnectionRequest { ConnectionIdentifier = connectionIdentifier }, cancellationToken: cancellationToken);
		DisposeChannel();
	}

	/// <summary>
	/// Disposes the channel.
	/// </summary>
	void DisposeChannel()
	{
		if (channelManager == null)
			return;

		channelManager.Dispose();
		channelManager = null;
	}
}