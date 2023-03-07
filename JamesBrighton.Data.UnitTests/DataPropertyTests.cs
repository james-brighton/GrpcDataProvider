using Google.Protobuf.WellKnownTypes;
using JamesBrighton.Data.Common;
using NUnit.Framework;

namespace JamesBrighton.Data.UnitTests;

public class PropertyTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Constructor_InitializesEmptyContent()
    {
        var property = new Property();
        Assert.AreEqual(true, property.IsNull);
        Assert.AreEqual(typeof(object), property.Type);
    }

    [Test]
    public void SetAndGetValue_Succeeds()
    {
        var property = new Property();
        property.Value = 123;
        Assert.AreEqual(false, property.IsNull);
        Assert.AreEqual(123, property.Value);
        Assert.AreEqual(typeof(int), property.Type);
    }

    [Test]
    public void SetInvalidValue_CausesEmptyContent()
    {
        var property = new Property();
        property.Value = "not a number";
        Assert.AreEqual(true, property.IsNull);
    }

    [Test]
    public void SetAndGetName_Succeeds()
    {
        var propertyName = "MyPropertyName";
        var property = new Property();
        property.Name = propertyName;
        Assert.AreEqual(propertyName, property.Name);
    }

    [Test]
    public void ImplicitConversionToAny_Succeeds()
    {
        var property = new Property();
        property.Value = "Hello world";
        var any = (Any)property;
        Assert.AreEqual(property.GetType().FullName, any.TypeUrl);
        Assert.AreEqual(property.CalculateSize(), any.Value.Length);
    }

    [Test]
    public void ImplicitConversionFromAny_Succeeds()
    {
        var property = new Property();
        property.Value = "Hello world";
        var any = (Any)property;
        var copy = any.Unpack<Property>();
        Assert.AreEqual(property.IsNull, copy.IsNull);
        Assert.AreEqual(property.Name, copy.Name);
        Assert.AreEqual(property.Type, copy.Type);
        Assert.AreEqual(property.Value, copy.Value);
    }
}