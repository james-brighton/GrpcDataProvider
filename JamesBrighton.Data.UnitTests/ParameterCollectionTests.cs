using JamesBrighton.Data.GrpcClient.Common;
using NUnit.Framework;

namespace JamesBrighton.Data.UnitTests;

public class ParameterCollectionTests
{
	ParameterCollection? _parameterCollection;

	[SetUp]
	public void Setup()
	{
		_parameterCollection = [];
	}

	[Test]
	public void IndexOf_ShouldReturnNegative1_WhenParameterNameNotFound()
	{
		if (_parameterCollection == null) return;
		// Arrange
		const string parameterName = "testParam";

		// Act
		var result = _parameterCollection.IndexOf(parameterName);

		// Assert
		Assert.That(-1, Is.EqualTo(result));
	}

	[Test]
	public void Contains_ShouldReturnFalse_WhenParameterNameNotFound()
	{
		if (_parameterCollection == null) return;

		// Arrange
		const string parameterName = "testParam";

		// Act
		var result = _parameterCollection.Contains(parameterName);

		// Assert
		Assert.That(result, Is.False);
	}

	[Test]
	public void RemoveAt_ShouldNotRemove_WhenParameterNameNotFound()
	{
		if (_parameterCollection == null) return;

		// Arrange
		const string parameterName = "testParam";
		_parameterCollection.Add(new Parameter { ParameterName = parameterName });

		// Act
		_parameterCollection.RemoveAt("notFoundParam");

		// Assert
		Assert.That(1, Is.EqualTo(_parameterCollection.Count));
		Assert.That(parameterName, Is.EqualTo(_parameterCollection[0].ParameterName));
	}
}