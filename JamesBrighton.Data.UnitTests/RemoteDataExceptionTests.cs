using JamesBrighton.Data.GrpcClient.Common;
using NUnit.Framework;

namespace JamesBrighton.Data.UnitTests;

[TestFixture]
public class RemoteDataExceptionTests
{
    [Test]
    public void Constructor_WithMessage_SetsMessageProperty()
    {
        // Arrange
        var message = "Test error message";

        // Act
        var ex = new RemoteDataException(message);

        // Assert
        Assert.That(ex.Message, Is.EqualTo(message));
    }

    [Test]
    public void ClassName_Property_CanBeSetAndGet()
    {
        // Arrange
        var className = "Test.Class.Name";
        var ex = new RemoteDataException(null)
        {
            // Act
            ClassName = className
        };

        var result = ex.ClassName;

        // Assert
        Assert.That(result, Is.EqualTo(className));
    }
        
    [Test]
    public void Indexer_WithIntArgument_CanBeSetAndGetPropertyValue()
    {
        // Arrange
        var propName = "TestPropertyName";
        var propValue = "TestPropertyValue";
        var ex = new RemoteDataException(null)
        {
            [propName] = propValue
        };

        // Act
        var result = (string)ex[0];

        // Assert
        Assert.That(result, Is.EqualTo(propValue));
    }

    [Test]
    public void Indexer_WithStringArgument_CanBeSetAndGetPropertyValue()
    {
        // Arrange
        var propName = "TestPropertyName";
        var propValue = "TestPropertyValue";
        var ex = new RemoteDataException(null)
        {
            [propName] = propValue
        };

        // Act
        var result = (string)ex[propName];

        // Assert
        Assert.That(result, Is.EqualTo(propValue));
    }

    [Test]
    public void TryGetPropertyValue_WithExistingPropertyName_ReturnsTrueAndPropertyValue()
    {
        // Arrange
        var propName = "TestPropertyName";
        var propValue = "TestPropertyValue";
        var ex = new RemoteDataException(null)
        {
            [propName] = propValue
        };

        // Act
        var result = ex.TryGetPropertyValue(propName, out var value);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(value, Is.EqualTo(propValue));
        });
    }

    [Test]
    public void TryGetPropertyValue_WithNonExistingPropertyName_ReturnsFalseAndNullValue()
    {
        // Arrange
        var ex = new RemoteDataException(null);

        // Act
        var result = ex.TryGetPropertyValue("NotARealProperty", out var value);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.False);
            Assert.That(value, Is.Null);
        });
    }
}