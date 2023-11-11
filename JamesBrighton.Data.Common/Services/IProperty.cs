namespace JamesBrighton.Data.Common;

/// <summary>
/// This interface presents a property.
/// </summary>
public interface IProperty
{
	/// <summary>
	/// Gets/sets the name of the property.
	/// </summary>
	string Name { get; set; }

	/// <summary>
	/// Gets/sets the value of the property.
	/// </summary>
	object Value { get; set; }

	/// <summary>
	/// Gets a value indicating whether the property is empty (null).
	/// </summary>
	bool IsNull { get; }

	/// <summary>
	/// Gets the type of the property.
	/// </summary>
	Type Type { get; }
}