using System.Data;
using JamesBrighton.Data.GrpcClient.Common;

namespace JamesBrighton.Data.GrpcClient.Tunnel;

/// <summary>
/// Represents a tunneled gRPC client factory class.
/// </summary>
public sealed class TunnelClientFactory : IAsyncProviderFactory
{
	/// <summary>
	/// The instance of this class.
	/// </summary>
	public static readonly TunnelClientFactory Instance = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TunnelClientFactory" /> class.
    /// </summary>
	TunnelClientFactory() { }

    /// <inheritdoc />
    public IAsyncDbCommand CreateCommand() => new TunnelCommand();

    /// <inheritdoc />
    public IAsyncConnection CreateConnection() => new TunnelConnection();

    /// <inheritdoc />
	public IConnectionStringBuilder CreateConnectionStringBuilder() =>  new ConnectionStringBuilder();

    /// <inheritdoc />
    public IDbDataParameter CreateParameter() => new GrpcParameter();

    /// <inheritdoc />
    public IAsyncDbTransaction CreateTransaction() => new TunnelTransaction();
}