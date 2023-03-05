using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

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
            throw new InvalidOperationException("There's no connection.");
        return TunnelTransaction.BeginTransaction(connection, il);
    }

    /// <inheritdoc />
    public async Task<IAsyncDbTransaction> BeginTransactionAsync() => await BeginTransactionAsync(IsolationLevel.ReadCommitted);

	/// <inheritdoc />
	public async Task<IAsyncDbTransaction> BeginTransactionAsync(IsolationLevel il)
    {
        if (connection == null)
            throw new InvalidOperationException("There's no connection.");
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
            throw new InvalidOperationException("There's no connection.");
        connection.Close();
	}

	/// <inheritdoc />
	public IDbCommand CreateCommand()
    {
        if (connection == null)
            throw new InvalidOperationException("There's no connection.");
        return Tunnel.CreateCommand(connection);
    }

    /// <inheritdoc />
    public async Task<IAsyncDbCommand> CreateCommandAsync()
    {
        if (connection == null)
            throw new InvalidOperationException("There's no connection.");
        return await Tunnel.CreateCommandAsync(connection);
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
        var factory = DbProviderFactories.GetFactory(ServerProviderInvariantName);

        connection = factory.CreateConnection();
        if (connection == null)
            return;
        connection.ConnectionString = ServerConnectionString;
        connection.Open();
	}

	/// <inheritdoc />
	public async Task OpenAsync() => await OpenAsync(CancellationToken.None);

	/// <inheritdoc />
	public async Task OpenAsync(CancellationToken cancellationToken)
	{
        var factory = DbProviderFactories.GetFactory(ServerProviderInvariantName);

        connection = factory.CreateConnection();
        if (connection == null)
            return;
        connection.ConnectionString = ServerConnectionString;
        await connection.OpenAsync(cancellationToken);
	}

	/// <inheritdoc />
	public async Task CloseAsync() => await CloseAsync(CancellationToken.None);

	/// <inheritdoc />
	public async Task CloseAsync(CancellationToken cancellationToken)
	{
        if (connection == null)
            throw new InvalidOperationException("There's no connection.");
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