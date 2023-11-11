using JamesBrighton.DataProvider.Grpc;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Type = System.Type;

namespace JamesBrighton.Data.Common;

/// <summary>
/// Represents an item of data that can be serialized and deserialized using the ProtoBuf serialization library.
/// </summary>
public class DataParameter : IDataParameter, IMessage
{
	/// <summary>
	/// Gets the empty object.
	/// </summary>
	readonly object emptyObject = new();

	/// <summary>
	/// The inner Parameter.
	/// </summary>
	InnerParameter innerParameter;

	/// <summary>
	/// The value.
	/// </summary>
	object value;

	/// <summary>
	/// Initializes a new instance of the <see cref="DataParameter" /> class with empty content.
	/// </summary>
	public DataParameter()
	{
		innerParameter = new InnerParameter();
		value = emptyObject;
	}

	/// <inheritdoc />
	public string Name
	{
		get => innerParameter.Name;
		set => innerParameter.Name = value;
	}

	/// <inheritdoc />
	public object Value
	{
		get => value;
		set
		{
			var memoryStream = new MemoryStream();
			var val = Serializer2.Serialize(memoryStream, value);
			if (val != null)
			{
				memoryStream.Position = 0;
				innerParameter = new InnerParameter
				{
					Name = innerParameter.Name,
					Content = ByteString.FromStream(memoryStream),
					Type = val.GetType().FullName ?? ""
				};
				this.value = value;
			}
			else
			{
				innerParameter = new InnerParameter();
				this.value = emptyObject;
			}
		}
	}

	/// <inheritdoc />
	public bool IsNull => Value == emptyObject;

	/// <inheritdoc />
	public Type Type => Value.GetType();

	/// <inheritdoc />
	public MessageDescriptor Descriptor => InnerParameter.Descriptor;

	/// <inheritdoc />
	public void MergeFrom(CodedInputStream input)
	{
		innerParameter.MergeFrom(input);
		var type = Type.GetType(innerParameter.Type);
		if (type == null)
		{
			innerParameter = new InnerParameter();
			value = emptyObject;
			return;
		}
		using var memoryStream = new MemoryStream();
		innerParameter.Content.WriteTo(memoryStream);
		memoryStream.Position = 0;
		if (Serializer2.TryDeserialize(type, memoryStream, out var v))
		{
			value = v ?? emptyObject;
		}
		else
		{
			innerParameter = new InnerParameter();
			value = emptyObject;
		}
	}

	/// <inheritdoc />
	public void WriteTo(CodedOutputStream output) => innerParameter.WriteTo(output);

	/// <inheritdoc />
	public int CalculateSize() => innerParameter.CalculateSize();

	/// <summary>
	/// Converts a Any object into an instance of IParameter.
	/// </summary>
	/// <param name="any">The Any object to convert.</param>
	/// <returns>An instance of IParameter that represents the Any object.</returns>
	public static implicit operator DataParameter(Any any) => any.Unpack<DataParameter>();

	/// <summary>
	/// Converts a Parameter object into an instance of Any using an implicit operator.
	/// </summary>
	/// <param name="parameter">The Parameter object to convert.</param>
	/// <returns>An instance of Any that represents the Parameter object.</returns>
	public static implicit operator Any(DataParameter parameter) => Any.Pack(parameter);
}