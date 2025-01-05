using JamesBrighton.Data.Common;
using NUnit.Framework;

namespace JamesBrighton.Data.UnitTests;

[TestFixture]
public class Serializer2Tests
{
	const string TestData = "This is a test string!";

	MemoryStream? _memoryStream;
	TestClass? _testClass;

	[SetUp]
	public void Setup()
	{
		_memoryStream = new MemoryStream();
		_testClass = new TestClass(TestData);
	}

	[TearDown]
	public void TearDown()
	{
		_memoryStream?.Dispose();
	}

	[Test]
	public void Serialize_ShouldReturnSuccessfully()
	{
		if (_memoryStream == null) return;
		if (_testClass == null) return;
		var actualResult = Serializer2.Serialize(_memoryStream, _testClass);

		Assert.That(actualResult, Is.Not.Null);
		Assert.That(_memoryStream.Length, Is.Not.Zero);
		Assert.That(_memoryStream.Position, Is.EqualTo(0));
	}

	[Test]
	public void TryDeserialize_ShouldFailIfContentCannotBeDeserialized()
	{
		if (_memoryStream == null) return;

		_memoryStream.Write(new byte[] { 0xFF, 0xFF }, 0, 2);
		_memoryStream.Position = 0;

		var actualResult = Serializer2.TryDeserialize(typeof(TestClass), _memoryStream, out var deserializedObject);

		Assert.That(actualResult, Is.False);
		Assert.That(deserializedObject, Is.Null);
	}

	public class TestClass
	{
		public string Message { get; set; }

		public TestClass()
		{
			Message = string.Empty;
		}

		public TestClass(string message)
		{
			Message = message;
		}
	}
}