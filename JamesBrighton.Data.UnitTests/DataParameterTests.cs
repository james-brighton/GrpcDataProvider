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
		Assert.That("param1", Is.EqualTo(param.Name));
	}

	[Test]
	public void Value_Set_SetsValueAndInnerParameterContent()
	{
		var param = new DataParameter
		{
			Value = "hello"
		};
		Assert.That("hello", Is.EqualTo(param.Value));
		Assert.That("System.String", Is.EqualTo(param.Type.FullName));
		Assert.That(param.IsNull, Is.False);
	}

	[Test]
	public void IsNull_True_ByDefault()
	{
		var param = new DataParameter();
		Assert.That(param.IsNull, Is.True);
	}

	[Test]
	public void Type_Get_ReturnsValueType()
	{
		var param = new DataParameter
		{
			Value = 3.14
		};
		Assert.That(typeof(double), Is.EqualTo(param.Type));
	}
}