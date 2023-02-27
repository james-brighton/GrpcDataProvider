using Brighton.James.Dataprovider.Grpc;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Type = System.Type;

namespace JamesBrighton.Data.Common;

/// <summary>
/// Represents an item of data that can be serialized and deserialized using the ProtoBuf serialization library.
/// </summary>
public class DataField : IDataField, IMessage
{
    /// <summary>
    /// Gets the empty object.
    /// </summary>
    readonly object emptyObject = new();

    /// <summary>
    /// The inner field.
    /// </summary>
    InnerField innerField;

    /// <summary>
    /// The value.
    /// </summary>
    object value;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataField" /> class with empty content.
    /// </summary>
    public DataField()
    {
        innerField = new InnerField();
        value = emptyObject;
    }

    /// <inheritdoc />
    public string Name
    {
        get => innerField.Name;
        set => innerField.Name = value;
    }

    /// <inheritdoc />
    public object Value
    {
        get => value;
        set
        {
            var memoryStream = new MemoryStream();
            try
            {
                Serializer2.Serialize(memoryStream, value);
                memoryStream.Position = 0;
                innerField = new InnerField
                {
                    Name = innerField.Name,
                    Content = ByteString.FromStream(memoryStream),
                    Type = value.GetType().FullName ?? "",
                    DataTypeName = innerField.DataTypeName
                };
                this.value = value;
            }
            catch (InvalidOperationException)
            {
                innerField = new InnerField();
                this.value = emptyObject;
            }
        }
    }

    /// <inheritdoc />
    public bool IsNull => Value == emptyObject;

    /// <inheritdoc />
    public Type Type => Value.GetType();

    /// <inheritdoc />
    public string DataTypeName
    {
        get => innerField.DataTypeName;
        set => innerField.DataTypeName = value;
    }

    /// <inheritdoc />
    public MessageDescriptor Descriptor => InnerField.Descriptor;

    /// <inheritdoc />
    public void MergeFrom(CodedInputStream input)
    {
        innerField.MergeFrom(input);
        var type = Type.GetType(innerField.Type);
        if (type == null) return;
        using var memoryStream = new MemoryStream();
        innerField.Content.WriteTo(memoryStream);
        memoryStream.Position = 0;
        try
        {
            value = Serializer2.Deserialize(type, memoryStream) ?? emptyObject;
        }
        catch (InvalidOperationException)
        {
            innerField = new InnerField();
            value = emptyObject;
        }
    }

    /// <inheritdoc />
    public void WriteTo(CodedOutputStream output) => innerField.WriteTo(output);

    /// <inheritdoc />
    public int CalculateSize() => innerField.CalculateSize();

    public T GetValue<T>() => (T)value;

    /// <summary>
    /// Converts a Any object into an instance of IDataField.
    /// </summary>
    /// <param name="any">The Any object to convert.</param>
    /// <returns>An instance of IDataField that represents the Any object.</returns>
    public static implicit operator DataField(Any any) => any.Unpack<DataField>();

    /// <summary>
    /// Converts a Field object into an instance of Any using an implicit operator.
    /// </summary>
    /// <param name="field">The Field object to convert.</param>
    /// <returns>An instance of Any that represents the Field object.</returns>
    public static implicit operator Any(DataField field) => Any.Pack(field);
}