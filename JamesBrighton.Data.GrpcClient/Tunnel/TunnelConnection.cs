using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using JamesBrighton.Data.GrpcClient.Common;

namespace JamesBrighton.Data.GrpcClient.Tunnel;

/// <summary>
/// Represents a tunneled gRPC implementation of an <see cref="IDbConnection" />.
/// </summary>
public class TunnelConnection : IAsyncRemoteConnection
{
	/// <summary>
	/// The connection.
	/// </summary>
	DbConnection? connection;

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
	public string ClientIdentifier { get; set; } = "";

	/// <inheritdoc />
	public int ConnectionTimeout => 0;

	/// <inheritdoc />
	public string Database => "";

	/// <inheritdoc />
	public ConnectionState State => connection?.State ?? ConnectionState.Closed;

	/// <inheritdoc />
	public IDbTransaction BeginTransaction() => BeginTransaction(IsolationLevel.Unspecified);

	/// <inheritdoc />
	public IDbTransaction BeginTransaction(IsolationLevel il)
	{
		if (connection == null)
			throw new RemoteDataException("There's no connection.");
		return TunnelTransaction.BeginTransaction(connection, il);
	}

	/// <inheritdoc />
	public async Task<IAsyncDbTransaction> BeginTransactionAsync() => await BeginTransactionAsync(IsolationLevel.ReadCommitted);

	/// <inheritdoc />
	public async Task<IAsyncDbTransaction> BeginTransactionAsync(IsolationLevel il)
	{
		if (connection == null)
			throw new RemoteDataException("There's no connection.");
		return await TunnelTransaction.BeginTransactionAsync(connection, il);
	}

	/// <inheritdoc />
	public void ChangeDatabase(string databaseName)
	{
	}

	/// <inheritdoc />
	public void Close()
	{
		if (connection == null)
			throw new RemoteDataException("There's no connection.");
		connection.Close();
	}

	/// <inheritdoc />
	public IDbCommand CreateCommand()
	{
		if (connection == null)
			throw new RemoteDataException("There's no connection.");
		return TunnelCommand.CreateCommand(connection);
	}

	/// <inheritdoc />
	public async Task<IAsyncDbCommand> CreateCommandAsync()
	{
		if (connection == null)
			throw new RemoteDataException("There's no connection.");
		return await TunnelCommand.CreateCommandAsync(connection);
	}

	/// <inheritdoc />
	public void Dispose()
	{
		Close();
		DisposeConnection();
		GC.SuppressFinalize(this);
	}

	/// <inheritdoc />
	public async ValueTask DisposeAsync()
	{
		await CloseAsync();
		DisposeConnection();
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

		DbProviderFactory factory;
		try
		{
			factory = DbProviderFactories.GetFactory(ServerProviderInvariantName);
		}
		catch (ArgumentException e)
		{
			// Cannot find factory
			RemoteDataException.ThrowDataException(e);
			return;
		}

		connection = factory.CreateConnection();
		if (connection == null)
			return;
		connection.ConnectionString = ServerConnectionString;
		try
		{
			connection.Open();
		}
		catch (Exception e)
		{
			RemoteDataException.ThrowDataException(e);
		}
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

		DbProviderFactory factory;
		try
		{
			factory = DbProviderFactories.GetFactory(ServerProviderInvariantName);
		}
		catch (ArgumentException e)
		{
			// Cannot find factory
			RemoteDataException.ThrowDataException(e);
			return;
		}

		connection = factory.CreateConnection();
		if (connection == null)
			return;
		connection.ConnectionString = ServerConnectionString;
		try
		{
			await connection.OpenAsync(cancellationToken);
		}
		catch (Exception e)
		{
			RemoteDataException.ThrowDataException(e);
		}
	}

	/// <inheritdoc />
	public async Task CloseAsync() => await CloseAsync(CancellationToken.None);

	/// <inheritdoc />
	public async Task CloseAsync(CancellationToken cancellationToken)
	{
		if (connection == null)
			throw new RemoteDataException("There's no connection.");
		await connection.CloseAsync();
	}

	/// <summary>
	/// Disposes the channel.
	/// </summary>
	void DisposeConnection()
	{
		if (connection == null)
			return;

		connection.Dispose();
		connection = null;
	}
}