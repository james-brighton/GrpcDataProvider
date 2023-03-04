using System.Data;
using JamesBrighton.DataProvider.Grpc;
using Grpc.Core;
using JamesBrighton.Data.Common;

namespace JamesBrighton.Data.GrpcClient;

/// <summary>
/// This class represents a IDataReader implementation that communicates over gRPC.
/// </summary>
public class GrpcDataReader : IAsyncDataReader
{
    /// <summary>
    /// The cached items.
    /// </summary>
    readonly List<DataField> items = new();

    /// <summary>
    /// The asynchronous query response object.
    /// </summary>
    readonly AsyncServerStreamingCall<ExecuteQueryResponse>? asyncResponse;
    /// <summary>
    /// The synchronous query response object.
    /// </summary>
    readonly ExecuteQuerySyncResponse? syncResponse;
    /// <summary>
    /// The synchronous query response object index.
    /// </summary>
    int syncResponseIndex;

    bool opened;

    /// <summary>
    /// Initializes a new instance of the <see cref="GrpcDataReader" /> class.
    /// </summary>
    /// <param name="asyncResponse">Query response.</param>
    public GrpcDataReader(AsyncServerStreamingCall<ExecuteQueryResponse> asyncResponse)
    {
        this.asyncResponse = asyncResponse;
        opened = true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GrpcDataReader" /> class.
    /// </summary>
    /// <param name="syncResponse">Query response.</param>
    public GrpcDataReader(ExecuteQuerySyncResponse syncResponse)
    {
        this.syncResponse = syncResponse;
        opened = true;
    }

    /// <inheritdoc />
    public object this[int i] => items[i].Value;

    /// <inheritdoc />
    public object this[string name] => items.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.Ordinal))?.Value ?? new DataField().Value;

    /// <inheritdoc />
    public int Depth => 1;

    /// <inheritdoc />
    public bool IsClosed => !opened;

    /// <inheritdoc />
    public int RecordsAffected => 0;

    /// <inheritdoc />
    public int FieldCount => items.Count;

    /// <inheritdoc />
    public void Close()
    {
        asyncResponse?.Dispose();
        opened = false;
    }

    /// <inheritdoc />
    public async Task CloseAsync()
    {
        await CloseAsync(CancellationToken.None);
    }

    /// <inheritdoc />
    public async Task CloseAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(0, cancellationToken);
        asyncResponse?.Dispose();
        opened = false;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Close();
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await CloseAsync();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public bool GetBoolean(int i) => items[i].GetValue<bool>();

    /// <inheritdoc />
    public byte GetByte(int i) => items[i].GetValue<byte>();

    /// <inheritdoc />
    public long GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferoffset, int length) => GetFieldArray(i, fieldOffset, buffer, bufferoffset, length);

    /// <inheritdoc />
    public char GetChar(int i) => items[i].GetValue<char>();

    /// <inheritdoc />
    public long GetChars(int i, long fieldoffset, char[]? buffer, int bufferoffset, int length) => GetFieldArray(i, fieldoffset, buffer, bufferoffset, length);

    /// <inheritdoc />
    public IDataReader GetData(int i) => this;

    /// <inheritdoc />
    public string GetDataTypeName(int i) => items[i].DataTypeName;

    /// <inheritdoc />
    public DateTime GetDateTime(int i) => items[i].GetValue<DateTime>();

    /// <inheritdoc />
    public decimal GetDecimal(int i) => items[i].GetValue<decimal>();

    /// <inheritdoc />
    public double GetDouble(int i) => items[i].GetValue<double>();

    /// <inheritdoc />
    public Type GetFieldType(int i) => items[i].Type;

    /// <inheritdoc />
    public float GetFloat(int i) => items[i].GetValue<float>();

    /// <inheritdoc />
    public Guid GetGuid(int i) => items[i].GetValue<Guid>();

    /// <inheritdoc />
    public short GetInt16(int i) => items[i].GetValue<short>();

    /// <inheritdoc />
    public int GetInt32(int i) => items[i].GetValue<int>();

    /// <inheritdoc />
    public long GetInt64(int i) => items[i].GetValue<long>();

    /// <inheritdoc />
    public string GetName(int i) => items[i].Name;

    /// <inheritdoc />
    public int GetOrdinal(string name) => items.FindIndex(x => string.Equals(x.Name, name, StringComparison.Ordinal));

    /// <inheritdoc />
    public DataTable? GetSchemaTable() => null;

    /// <inheritdoc />
    public string GetString(int i)
    {
        var item = items[i];
        if (!item.IsNull)
            return item.GetValue<string>();
        return string.Empty;
    }

    /// <inheritdoc />
    public object GetValue(int i) => items[i].Value;

    /// <inheritdoc />
    public int GetValues(object[] values)
    {
        var count = Math.Min(values.Length, items.Count);
        for (var i = 0; i < count; i++) values[i] = items[i].Value;
        return count;
    }

    /// <inheritdoc />
    public bool IsDBNull(int i) => items[i].IsNull;

    /// <inheritdoc />
    public bool NextResult() => false;

    /// <inheritdoc />
    public bool Read()
    {
        items.Clear();
        if (syncResponse == null)
            return false;

        if (syncResponse.DataException != null)
            GrpcDataException.ThrowDataException(syncResponse.DataException);

        if (syncResponseIndex >= syncResponse.Rows.Count)
            return false;

        foreach (var field in syncResponse.Rows[syncResponseIndex].Fields)
        {
            var f = (DataField)field;
            items.Add(f);
        }
        syncResponseIndex++;
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ReadAsync() => await ReadAsync(CancellationToken.None);

    /// <inheritdoc />
    public async Task<bool> ReadAsync(CancellationToken cancellationToken)
    {
        items.Clear();
        if (asyncResponse == null)
            return false;
        var result = await asyncResponse.ResponseStream.MoveNext(cancellationToken);
        if (!result) return false;
        var r = asyncResponse.ResponseStream.Current;
        if (r.DataException != null)
            GrpcDataException.ThrowDataException(r.DataException);
        foreach (var field in r.Fields)
        {
            var f = (DataField)field;
            items.Add(f);
        }

        return result;
    }

    /// <summary>
    /// Gets the field's content (an array) at the given index.
    /// </summary>
    /// <param name="i">Index of the field.</param>
    /// <param name="fieldOffset">offset within the array.</param>
    /// <param name="buffer">Buffer to copy the array into.</param>
    /// <param name="bufferOffset">Offset in the buffer.</param>
    /// <param name="length">Length to copy.</param>
    /// <typeparam name="T">Type of the array.</typeparam>
    /// <returns>The items copied or 0 otherwise.</returns>
    long GetFieldArray<T>(int i, long fieldOffset, T[]? buffer, int bufferOffset, int length)
    {
        if (buffer == null) return 0;
        var maxBufferLength = buffer.Length - bufferOffset;
        if (maxBufferLength <= 0 || i < 0 || i >= items.Count) return 0;

        var value = items[i].GetValue<T[]>();
        var copyCount = Math.Min(Math.Min(maxBufferLength, length), value.Length);
        if (copyCount <= 0) return 0;
        Buffer.BlockCopy(value, (int)fieldOffset, buffer, bufferOffset, copyCount);
        return copyCount;
    }
}