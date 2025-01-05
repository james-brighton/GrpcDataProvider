using Google.Protobuf.WellKnownTypes;
using JamesBrighton.Data.Common;
using NUnit.Framework;

namespace JamesBrighton.Data.UnitTests;

[TestFixture]
public class DataFieldTests
{
	[Test]
	public void EmptyConstructor_Test()
	{
		DataField dataField = new();

		Assert.That(dataField.Name, Is.Empty);
		Assert.That(dataField.IsNull, Is.EqualTo(true));
	}

	[Test]
	public void SetValueAndGetTypeName_Test()
	{
		DataField dataField = new();
		var expectedValue = 10;

		dataField.Value = expectedValue;

		Assert.That(dataField.Value, Is.EqualTo(expectedValue));
		Assert.That(dataField.DataTypeName, Is.EqualTo(""));
		Assert.That(dataField.Type.FullName, Is.EqualTo(typeof(int).FullName));
		Assert.That(dataField.IsNull, Is.EqualTo(false));
		Assert.That(dataField.GetValue<int>(), Is.InstanceOf<int>());
		Assert.That(dataField.GetValue<int>(), Is.EqualTo(expectedValue));
	}

	[Test]
	public void ImplicitOperator_AnyToDataField_Test()
	{
		var value = "John";
		var dataField = new DataField { Name = "test", Value = value };

		Any any = dataField;
		var newDataField = any.Unpack<DataField>();

		Assert.That(dataField.Name, Is.EqualTo(newDataField.Name));
		Assert.That(dataField.Value, Is.EqualTo(newDataField.Value));
		Assert.That(dataField.DataTypeName, Is.EqualTo(newDataField.DataTypeName));
	}

	[Test]
	public void ImplicitOperator_DataFieldToAny_Test()
	{
		var value = "John";
		var dataField = new DataField { Name = "test", Value = value };

		Any any = dataField;
		var newDataField = any.Unpack<DataField>();

		Assert.That(dataField.Name, Is.EqualTo(newDataField.Name));
		Assert.That(dataField.Value, Is.EqualTo(newDataField.Value));
		Assert.That(dataField.DataTypeName, Is.EqualTo(newDataField.DataTypeName));
	}
}