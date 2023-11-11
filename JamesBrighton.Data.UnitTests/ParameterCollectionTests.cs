using JamesBrighton.Data.GrpcClient.Common;
using NUnit.Framework;

namespace JamesBrighton.Data.UnitTests;

public class ParameterCollectionTests
{
	ParameterCollection? _parameterCollection;

	[SetUp]
	public void Setup()
	{
		_parameterCollection = new ParameterCollection();
	}

	[Test]
	public void IndexOf_ShouldReturnNegative1_WhenParameterNameNotFound()
	{
		// Arrange
		const string parameterName = "testParam";

		// Act
		var result = _parameterCollection.IndexOf(parameterName);

		// Assert
		Assert.AreEqual(-1, result);
	}

	[Test]
	public void Contains_ShouldReturnFalse_WhenParameterNameNotFound()
	{
		// Arrange
		const string parameterName = "testParam";

		// Act
		var result = _parameterCollection.Contains(parameterName);

		// Assert
		Assert.IsFalse(result);
	}

	[Test]
	public void RemoveAt_ShouldNotRemove_WhenParameterNameNotFound()
	{
		// Arrange
		const string parameterName = "testParam";
		_parameterCollection.Add(new Parameter { ParameterName = parameterName });

		// Act
		_parameterCollection.RemoveAt("notFoundParam");

		// Assert
		Assert.AreEqual(1, _parameterCollection.Count);
		Assert.AreEqual(parameterName, _parameterCollection[0].ParameterName);
	}
}