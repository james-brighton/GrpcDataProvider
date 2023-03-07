using JamesBrighton.Data.GrpcClient.Common;
using JamesBrighton.Data.GrpcClient.Grpc;
using NUnit.Framework;

namespace JamesBrighton.Data.UnitTests;

[TestFixture]
public class GrpcClientFactoryTests
{
    [Test]
    public void CreateCommand_ReturnsInstanceOfGrpcCommand()
    {
        // Arrange
        var factory = GrpcClientFactory.Instance;
            
        // Act
        var command = factory.CreateCommand();
            
        // Assert
        Assert.IsInstanceOf<GrpcCommand>(command);
    }

    [Test]
    public void CreateConnection_ReturnsInstanceOfGrpcConnection()
    {
        // Arrange
        var factory = GrpcClientFactory.Instance;

        // Act
        var connection = factory.CreateConnection();

        // Assert
        Assert.IsInstanceOf<GrpcConnection>(connection);
    }

    [Test]
    public void CreateConnectionString_ReturnsInstanceOfConnectionStringBuilder()
    {
        // Arrange
        var factory = GrpcClientFactory.Instance;

        // Act
        var csBuilder = factory.CreateConnectionStringBuilder();

        // Assert
        Assert.IsInstanceOf<ConnectionStringBuilder>(csBuilder);
    }

    [Test]
    public void CreateParameter_ReturnsInstanceOfParameter()
    {
        // Arrange
        var factory = GrpcClientFactory.Instance;

        // Act
        var parameter = factory.CreateParameter();

        // Assert
        Assert.IsInstanceOf<Parameter>(parameter);
    }

    [Test]
    public void CreateTransaction_ReturnsInstanceOfGrpcTransaction()
    {
        // Arrange
        var factory = GrpcClientFactory.Instance;

        // Act
        var transaction = factory.CreateTransaction();

        // Assert
        Assert.IsInstanceOf<GrpcTransaction>(transaction);
    }
}