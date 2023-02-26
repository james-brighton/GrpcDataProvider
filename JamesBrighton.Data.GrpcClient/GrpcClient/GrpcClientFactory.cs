using System.Data;

namespace JamesBrighton.Data.GrpcClient;

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
    public IDbDataParameter CreateParameter() => new GrpcParameter();

    /// <inheritdoc />
    public IAsyncDbTransaction CreateTransaction() => new GrpcTransaction();
}