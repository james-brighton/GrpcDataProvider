using System.Data;
using System.Data.Common;

namespace JamesBrighton.Data.GrpcClient.Tunnel;

/// <summary>
/// This class represents a tunneled IDataReader implementation that communicates over gRPC.
/// </summary>
public class TunnelDataReader : IAsyncDataReader
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TunnelDataReader" /> class.
    /// </summary>
    /// <param name="reader">The reader to use.</param>
    public TunnelDataReader(DbDataReader reader)
    {
        this.reader = reader;
    }

    /// <inheritdoc />
    public object this[int i] => reader[i];

    /// <inheritdoc />
    public object this[string name] => reader[name];

    /// <inheritdoc />
    public int Depth => 1;

    /// <inheritdoc />
    public bool IsClosed => reader.IsClosed;

    /// <inheritdoc />
    public int RecordsAffected => 0;

    /// <inheritdoc />
    public int FieldCount => reader.FieldCount;

    /// <inheritdoc />
    public void Close()
    {
        reader.Close();
    }

    /// <inheritdoc />
    public async Task CloseAsync()
    {
        await CloseAsync(CancellationToken.None);
    }

    /// <inheritdoc />
    public async Task CloseAsync(CancellationToken cancellationToken)
    {
        await reader.CloseAsync();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        reader.Dispose();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await reader.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public bool GetBoolean(int i) => reader.GetBoolean(i);

    /// <inheritdoc />
    public byte GetByte(int i) => reader.GetByte(i);

    /// <inheritdoc />
    public long GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferoffset, int length) => reader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);

    /// <inheritdoc />
    public char GetChar(int i) => reader.GetChar(i);

    /// <inheritdoc />
    public long GetChars(int i, long fieldoffset, char[]? buffer, int bufferoffset, int length) => reader.GetChars(i, fieldoffset, buffer, bufferoffset, length);

    /// <inheritdoc />
    public IDataReader GetData(int i) => this;

    /// <inheritdoc />
    public string GetDataTypeName(int i) => reader.GetDataTypeName(i);

    /// <inheritdoc />
    public DateTime GetDateTime(int i) => reader.GetDateTime(i);

    /// <inheritdoc />
    public decimal GetDecimal(int i) => reader.GetDecimal(i);

    /// <inheritdoc />
    public double GetDouble(int i) => reader.GetDouble(i);

    /// <inheritdoc />
    public Type GetFieldType(int i) => reader.GetFieldType(i);

    /// <inheritdoc />
    public float GetFloat(int i) => reader.GetFloat(i);

    /// <inheritdoc />
    public Guid GetGuid(int i) => reader.GetGuid(i);

    /// <inheritdoc />
    public short GetInt16(int i) => reader.GetInt16(i);

    /// <inheritdoc />
    public int GetInt32(int i) => reader.GetInt32(i);

    /// <inheritdoc />
    public long GetInt64(int i) => reader.GetInt64(i);

    /// <inheritdoc />
    public string GetName(int i) => reader.GetName(i);

    /// <inheritdoc />
    public int GetOrdinal(string name) => reader.GetOrdinal(name);

    /// <inheritdoc />
    public DataTable? GetSchemaTable() => null;

    /// <inheritdoc />
    public string GetString(int i) => reader.GetString(i);

    /// <inheritdoc />
    public object GetValue(int i) => reader.GetValue(i);

    /// <inheritdoc />
    public int GetValues(object[] values) => reader.GetValues(values);

    /// <inheritdoc />
    public bool IsDBNull(int i) => reader.IsDBNull(i);

    /// <inheritdoc />
    public bool NextResult() => false;

    /// <inheritdoc />
    public bool Read() => reader.Read();

    /// <inheritdoc />
    public async Task<bool> ReadAsync() => await ReadAsync(CancellationToken.None);

    /// <inheritdoc />
    public async Task<bool> ReadAsync(CancellationToken cancellationToken) => await reader.ReadAsync(cancellationToken);

    /// <summary>
    /// The reader.
    /// </summary>
    readonly DbDataReader reader;
}