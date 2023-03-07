using Google.Protobuf;
using JamesBrighton.Data.Common;
using NUnit.Framework;

namespace JamesBrighton.Data.UnitTests;

public class DataParameterTests
{
    [Test]
    public void Name_Get_ReturnsInnerParameterName()
    {
        var param = new DataParameter();
        param.Name = "param1";
        Assert.AreEqual("param1", param.Name);
    }

    [Test]
    public void Value_Set_SetsValueAndInnerParameterContent()
    {
        var param = new DataParameter();
        param.Value = "hello";
        Assert.AreEqual("hello", param.Value);
        Assert.AreEqual("System.String", param.Type.FullName);
        Assert.IsFalse(param.IsNull);
    }

    [Test]
    public void Value_Set_InvalidValue_SetsEmptyObject()
    {
        var param = new DataParameter();
        param.Value = new object(); // cannot serialize object as string
        Assert.AreEqual("", param.Value);
        Assert.AreEqual("", param.Type.FullName);
        Assert.IsTrue(param.IsNull);
    }

    [Test]
    public void IsNull_True_ByDefault()
    {
        var param = new DataParameter();
        Assert.IsTrue(param.IsNull);
    }

    [Test]
    public void Type_Get_ReturnsValueType()
    {
        var param = new DataParameter();
        param.Value = 3.14;
        Assert.AreEqual(typeof(double), param.Type);
    }

    [Test]
    public void Deserialize_WithValidData_SetsValue()
    {
        var param = new DataParameter();
        var stream = new MemoryStream();
        Serializer2.Serialize(stream, "foo");
        stream.Position = 0;
        param.MergeFrom(new CodedInputStream(stream.ToArray()));
        Assert.AreEqual("foo", param.Value);
        Assert.IsFalse(param.IsNull);
    }

    [Test]
    public void Deserialize_WithInvalidData_SetsEmptyObject()
    {
        var param = new DataParameter();
        var stream = new MemoryStream();
        Serializer2.Serialize(stream, new object()); // cannot deserialize object
        stream.Position = 0;
        param.MergeFrom(new CodedInputStream(stream.ToArray()));
        Assert.AreEqual("", param.Value);
        Assert.IsTrue(param.IsNull);
    }
}