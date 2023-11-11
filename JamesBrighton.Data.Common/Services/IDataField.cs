namespace JamesBrighton.Data.Common;

/// <summary>
/// This interface presents a (database) field.
/// </summary>
public interface IDataField
{
	/// <summary>
	/// Gets/sets the name of the field.
	/// </summary>
	string Name { get; set; }

	/// <summary>
	/// Gets/sets the value of the field.
	/// </summary>
	object Value { get; set; }

	/// <summary>
	/// Gets a value indicating whether the parameter is empty (null).
	/// </summary>
	bool IsNull { get; }

	/// <summary>
	/// Gets the type of the field.
	/// </summary>
	Type Type { get; }

	/// <summary>
	/// Gets/sets the data type's name of the field.
	/// </summary>
	string DataTypeName { get; set; }
}