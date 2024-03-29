namespace JamesBrighton.Data;

/// <summary>
/// Represents an asynchronous gRPC database connection.
/// </summary>
public interface IAsyncRemoteConnection : IAsyncConnection
{
	/// <summary>
	/// Gets or sets the provider invariant name at the server side.
	/// </summary>
	string ServerProviderInvariantName { get; set; }
	/// <summary>
	/// Gets or sets the string used to open a database at the server side.
	/// </summary>
	string ServerConnectionString { get; set; }
	/// <summary>
	/// Gets the client identifier.
	/// </summary>
	string ClientIdentifier { get; set; }
}
