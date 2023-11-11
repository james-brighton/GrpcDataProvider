namespace JamesBrighton.Data;

/// <summary>
/// Defines the contract for building a connection string.
/// </summary>
public interface IConnectionStringBuilder : IDictionary<string, string>
{
	/// <summary>
	/// Gets or sets the connection string.
	/// </summary>
	string ConnectionString { get; set; }
}
