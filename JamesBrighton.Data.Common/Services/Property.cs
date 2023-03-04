using JamesBrighton.DataProvider.Grpc;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Type = System.Type;

namespace JamesBrighton.Data.Common;

/// <summary>
/// Represents an item of data that can be serialized and deserialized using the ProtoBuf serialization library.
/// </summary>
public class Property : IProperty, IMessage
{
    /// <summary>
    /// Gets the empty object.
    /// </summary>
    readonly object emptyObject = new();

    /// <summary>
    /// The inner Property.
    /// </summary>
    InnerProperty innerProperty;

    /// <summary>
    /// The value.
    /// </summary>
    object value;

    /// <summary>
    /// Initializes a new instance of the <see cref="Property" /> class with empty content.
    /// </summary>
    public Property()
    {
        innerProperty = new InnerProperty();
        value = emptyObject;
    }

    /// <inheritdoc />
    public MessageDescriptor Descriptor => InnerProperty.Descriptor;

    /// <inheritdoc />
    public void MergeFrom(CodedInputStream input)
    {
        innerProperty.MergeFrom(input);
        var type = Type.GetType(innerProperty.Type);
        if (type == null) return;
        using var memoryStream = new MemoryStream();
        innerProperty.Content.WriteTo(memoryStream);
        memoryStream.Position = 0;
        if (Serializer2.TryDeserialize(type, memoryStream, out var v))
        {
            value = v ?? emptyObject;
        }
        else
        {
            innerProperty = new InnerProperty();
            value = emptyObject;
        }
    }

    /// <inheritdoc />
    public void WriteTo(CodedOutputStream output) => innerProperty.WriteTo(output);

    /// <inheritdoc />
    public int CalculateSize() => innerProperty.CalculateSize();

    /// <inheritdoc />
    public string Name
    {
        get => innerProperty.Name;
        set => innerProperty.Name = value;
    }

    /// <inheritdoc />
    public object Value
    {
        get => value;
        set
        {
            var memoryStream = new MemoryStream();
            if (Serializer2.Serialize(memoryStream, value))
            {
                memoryStream.Position = 0;
                innerProperty = new InnerProperty
                {
                    Name = innerProperty.Name,
                    Content = ByteString.FromStream(memoryStream),
                    Type = value.GetType().FullName ?? ""
                };
                this.value = value;
            }
            else
            {
                innerProperty = new InnerProperty();
                this.value = emptyObject;
            }
        }
    }

    /// <inheritdoc />
    public bool IsNull => Value == emptyObject;

    /// <inheritdoc />
    public Type Type => Value.GetType();

    /// <summary>
    /// Converts a Any object into an instance of IProperty.
    /// </summary>
    /// <param name="any">The Any object to convert.</param>
    /// <returns>An instance of IProperty that represents the Any object.</returns>
    public static implicit operator Property(Any any) => any.Unpack<Property>();

    /// <summary>
    /// Converts a Property object into an instance of Any using an implicit operator.
    /// </summary>
    /// <param name="parameter">The Property object to convert.</param>
    /// <returns>An instance of Any that represents the Property object.</returns>
    public static implicit operator Any(Property parameter) => Any.Pack(parameter);
}