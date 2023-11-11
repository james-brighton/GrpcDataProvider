using JamesBrighton.Data.Common.Helpers;
using NUnit.Framework;

namespace JamesBrighton.Data.UnitTests;

[TestFixture]
public class IsolationLevelConverterTests
{
	[Test]
	public void Convert_SystemToGrpc_IsolationLevels()
	{
		// Arrange
		var expected = DataProvider.Grpc.IsolationLevel.Unspecified;

		// Act
		var result = IsolationLevelConverter.Convert(System.Data.IsolationLevel.Unspecified);

		// Assert
		Assert.AreEqual(expected, result);
	}

	[Test]
	public void Convert_GrpcToSystem_IsolationLevels()
	{
		// Arrange
		var expected = System.Data.IsolationLevel.Unspecified;

		// Act
		var result = IsolationLevelConverter.Convert(DataProvider.Grpc.IsolationLevel.Unspecified);

		// Assert
		Assert.AreEqual(expected, result);
	}
}
