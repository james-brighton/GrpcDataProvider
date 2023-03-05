using System.Data;
using JamesBrighton.Data.GrpcClient.Common;

namespace JamesBrighton.Data.GrpcClient.Grpc;

/// <summary>
/// Represents a gRPC client factory class.
/// </summary>
public sealed class GrpcClientFactory : IAsyncProviderFactory
{
	/// <summary>
	/// The instance of this class.
	/// </summary>
	public static readonly GrpcClientFactory Instance = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GrpcClientFactory" /> class.
    /// </summary>
	GrpcClientFactory() { }

    /// <inheritdoc />
    public IAsyncDbCommand CreateCommand() => new GrpcCommand();

    /// <inheritdoc />
    public IAsyncConnection CreateConnection() => new GrpcConnection();

    /// <inheritdoc />
	public IConnectionStringBuilder CreateConnectionStringBuilder() =>  new ConnectionStringBuilder();

    /// <inheritdoc />
    public IDbDataParameter CreateParameter() => new Parameter();

    /// <inheritdoc />
    public IAsyncDbTransaction CreateTransaction() => new GrpcTransaction();
}