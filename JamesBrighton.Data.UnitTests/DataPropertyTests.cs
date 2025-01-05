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
		Assert.That(true, Is.EqualTo(property.IsNull));
		Assert.That(typeof(object), Is.EqualTo(property.Type));
	}

	[Test]
	public void SetAndGetValue_Succeeds()
	{
		var property = new Property
		{
			Value = 123
		};
		Assert.That(false, Is.EqualTo(property.IsNull));
		Assert.That(123, Is.EqualTo(property.Value));
		Assert.That(typeof(int), Is.EqualTo(property.Type));
	}

	[Test]
	public void SetAndGetName_Succeeds()
	{
		var propertyName = "MyPropertyName";
		var property = new Property
		{
			Name = propertyName
		};
		Assert.That(propertyName, Is.EqualTo(property.Name));
	}

	[Test]
	public void ImplicitConversionToAny_Succeeds()
	{
		var property = new Property
		{
			Value = "Hello world"
		};
		var any = (Any)property;
		Assert.That(property.CalculateSize(), Is.EqualTo(any.Value.Length));
	}

	[Test]
	public void ImplicitConversionFromAny_Succeeds()
	{
		var property = new Property
		{
			Value = "Hello world"
		};
		var any = (Any)property;
		var copy = any.Unpack<Property>();
		Assert.That(property.IsNull, Is.EqualTo(copy.IsNull));
		Assert.That(property.Name, Is.EqualTo(copy.Name));
		Assert.That(property.Type, Is.EqualTo(copy.Type));
		Assert.That(property.Value, Is.EqualTo(copy.Value));
	}
}