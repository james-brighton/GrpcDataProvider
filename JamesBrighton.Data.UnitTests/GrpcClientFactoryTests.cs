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
		Assert.That(command, Is.InstanceOf<GrpcCommand>());
	}

	[Test]
	public void CreateConnection_ReturnsInstanceOfGrpcConnection()
	{
		// Arrange
		var factory = GrpcClientFactory.Instance;

		// Act
		var connection = factory.CreateConnection();

		// Assert
		Assert.That(connection, Is.InstanceOf<GrpcConnection>());
	}

	[Test]
	public void CreateConnectionString_ReturnsInstanceOfConnectionStringBuilder()
	{
		// Arrange
		var factory = GrpcClientFactory.Instance;

		// Act
		var csBuilder = factory.CreateConnectionStringBuilder();

		// Assert
		Assert.That(csBuilder, Is.InstanceOf<ConnectionStringBuilder>());
	}

	[Test]
	public void CreateParameter_ReturnsInstanceOfParameter()
	{
		// Arrange
		var factory = GrpcClientFactory.Instance;

		// Act
		var parameter = factory.CreateParameter();

		// Assert
		Assert.That(parameter, Is.InstanceOf<Parameter>());
	}

	[Test]
	public void CreateTransaction_ReturnsInstanceOfGrpcTransaction()
	{
		// Arrange
		var factory = GrpcClientFactory.Instance;

		// Act
		var transaction = factory.CreateTransaction();

		// Assert
		Assert.That(transaction, Is.InstanceOf<GrpcTransaction>());
	}
}