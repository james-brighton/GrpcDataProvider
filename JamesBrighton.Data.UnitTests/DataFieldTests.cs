using NUnit.Framework;
using Google.Protobuf.WellKnownTypes;

namespace JamesBrighton.Data.Common.Tests;

[TestFixture]
public class DataFieldTests
{
    [Test]
    public void EmptyConstructor_Test()
    {
        DataField dataField = new();

        Assert.IsEmpty(dataField.Name);
        Assert.AreEqual(dataField.Value, null);
        Assert.AreEqual(dataField.IsNull, true);
        Assert.IsInstanceOf<Empty>(dataField.Value);
        Assert.IsInstanceOf<Empty>(dataField.GetValue<Empty>()); 
    }

    [Test]
    public void SetValueAndGetTypeName_Test()
    {
        DataField dataField = new();
        var expectedValue = 10;
        var expectedDataTypeName = expectedValue.GetType().FullName ?? "";

        dataField.Value = expectedValue;

        Assert.AreEqual(dataField.Value, expectedValue);
        Assert.AreEqual(dataField.DataTypeName, expectedDataTypeName);
        Assert.AreEqual(dataField.Type.FullName, typeof(int).FullName);
        Assert.AreEqual(dataField.IsNull, false);
        Assert.IsInstanceOf<int>(dataField.GetValue<int>());
        Assert.AreEqual(dataField.GetValue<int>(), expectedValue);
    }

    [Test]
    public void ImplicitOperator_AnyToDataField_Test()
    {
        var value = "John";
        var dataField = new DataField { Name = "test", Value = value };

        Any any = dataField;
        var newDataField = any.Unpack<DataField>();

        Assert.AreEqual(dataField.Name, newDataField.Name);
        Assert.AreEqual(dataField.Value, newDataField.Value);
        Assert.AreEqual(dataField.DataTypeName, newDataField.DataTypeName);
    }

    [Test]
    public void ImplicitOperator_DataFieldToAny_Test()
    {
        var value = "John";
        var dataField = new DataField { Name = "test", Value = value };

        Any any = dataField;
        var newDataField = any.Unpack<DataField>();

        Assert.AreEqual(dataField.Name, newDataField.Name);
        Assert.AreEqual(dataField.Value, newDataField.Value);
        Assert.AreEqual(dataField.DataTypeName, newDataField.DataTypeName);
    }
}