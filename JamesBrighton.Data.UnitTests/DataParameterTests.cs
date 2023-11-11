using JamesBrighton.Data.Common;
using NUnit.Framework;

namespace JamesBrighton.Data.UnitTests;

public class DataParameterTests
{
	[Test]
	public void Name_Get_ReturnsInnerParameterName()
	{
		var param = new DataParameter
		{
			Name = "param1"
		};
		Assert.AreEqual("param1", param.Name);
	}

	[Test]
	public void Value_Set_SetsValueAndInnerParameterContent()
	{
		var param = new DataParameter
		{
			Value = "hello"
		};
		Assert.AreEqual("hello", param.Value);
		Assert.AreEqual("System.String", param.Type.FullName);
		Assert.IsFalse(param.IsNull);
	}

	[Test]
	public void IsNull_True_ByDefault()
	{
		var param = new DataParameter();
		Assert.IsTrue(param.IsNull);
	}

	[Test]
	public void Type_Get_ReturnsValueType()
	{
		var param = new DataParameter
		{
			Value = 3.14
		};
		Assert.AreEqual(typeof(double), param.Type);
	}
}