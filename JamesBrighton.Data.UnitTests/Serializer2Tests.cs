using JamesBrighton.Data.Common;
using NUnit.Framework;

namespace JamesBrighton.Data.UnitTests;

[TestFixture]
public class Serializer2Tests
{
    const string TestData = "This is a test string!";

    MemoryStream _memoryStream;
    TestClass _testClass;

    [SetUp]
    public void Setup()
    {
        _memoryStream = new MemoryStream();
        _testClass = new TestClass(TestData);
    }

    [TearDown]
    public void TearDown()
    {
        _memoryStream.Dispose();
    }

    [Test]
    public void Serialize_ShouldReturnSuccessfully()
    {
        var actualResult = Serializer2.Serialize(_memoryStream, _testClass);

        Assert.IsTrue(actualResult);
        Assert.That(_memoryStream.Length, Is.Not.Zero);
        Assert.That(_memoryStream.Position, Is.EqualTo(0));
    }

    [Test]
    public void Serialize_ShouldFailIfNotAbleToSerializeContentType()
    {
        var actualResult = Serializer2.Serialize(_memoryStream, null);

        Assert.IsFalse(actualResult);
        Assert.That(_memoryStream.Length, Is.Zero);
        Assert.That(_memoryStream.Position, Is.EqualTo(0));
    }

    [Test]
    public void TryDeserialize_ShouldReturnSuccessfully()
    {
        Serializer2.Serialize(_memoryStream, _testClass);
        _memoryStream.Position = 0;

        var actualResult = Serializer2.TryDeserialize(typeof(TestClass), _memoryStream, out var deserializedObject);

        Assert.IsTrue(actualResult);
        Assert.That(deserializedObject, Is.Not.Null);
        Assert.That(((TestClass)deserializedObject).Message, Is.EqualTo(TestData));
    }

    [Test]
    public void TryDeserialize_ShouldFailIfUnableToRetrieveContentType()
    {
        Serializer2.Serialize(_memoryStream, _testClass);
        _memoryStream.Position = 0;

        var actualResult = Serializer2.TryDeserialize(null, _memoryStream, out var deserializedObject);

        Assert.IsFalse(actualResult);
        Assert.That(deserializedObject, Is.Null);
    }

    [Test]
    public void TryDeserialize_ShouldFailIfContentCannotBeDeserialized()
    {
        _memoryStream.Write(new byte[] { 0xFF, 0xFF }, 0, 2);
        _memoryStream.Position = 0;

        var actualResult = Serializer2.TryDeserialize(typeof(TestClass), _memoryStream, out var deserializedObject);

        Assert.IsFalse(actualResult);
        Assert.That(deserializedObject, Is.Null);
    }

    class TestClass
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