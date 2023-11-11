using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace JamesBrighton.Data.GrpcClient.Common;

/// <summary>
/// Represents a parameter used in a database command of an <see cref="IDbConnection" />.
/// </summary>
public class Parameter : IDbDataParameter
{
	/// <summary>
	/// An empty object.
	/// </summary>
	readonly object emptyObject = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="Parameter" /> class.
	/// </summary>
	public Parameter()
	{
		Value = emptyObject;
	}

	/// <inheritdoc />
	public byte Precision { get; set; }

	/// <inheritdoc />
	public byte Scale { get; set; }

	/// <inheritdoc />
	public int Size { get; set; }

	/// <inheritdoc />
	public DbType DbType { get; set; }

	/// <inheritdoc />
	public ParameterDirection Direction { get; set; }

	/// <inheritdoc />
	public bool IsNullable => false;

	/// <inheritdoc />
	[AllowNull]
	public string ParameterName { get; set; } = "";

	/// <inheritdoc />
	[AllowNull]
	public string SourceColumn { get; set; } = "";

	/// <inheritdoc />
	public DataRowVersion SourceVersion { get; set; }

	/// <summary>
	/// Value of the parameter.
	/// </summary>
	public object? Value { get; set; }
}