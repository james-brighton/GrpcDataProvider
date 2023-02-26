using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace JamesBrighton.Data.GrpcClient;

/// <summary>
/// Represents a parameter used in a database command in a gRPC implementation of an <see cref="IDbConnection" />.
/// </summary>
public class GrpcParameter : IDbDataParameter
{
    /// <summary>
    /// An empty object.
    /// </summary>
    readonly object emptyObject = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GrpcParameter" /> class.
    /// </summary>
    public GrpcParameter()
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