using JamesBrighton.Data.GrpcClient.Common;
using NUnit.Framework;

namespace JamesBrighton.Data.UnitTests;

[TestFixture]
public class ConnectionStringBuilderTests
{
	[Test]
	public void TestConstructionWithNoArgs()
	{
		var csb = new ConnectionStringBuilder();
		var expected = "";
		Assert.That(expected, Is.EqualTo(csb.ConnectionString));
	}

	[Test]
	public void TestConstructionWithStringArg()
	{
		var csb = new ConnectionStringBuilder("key1=value1;key2=value2");
		var expected = "key1=value1;key2=value2";
		Assert.That(expected, Is.EqualTo(csb.ConnectionString));
	}

	[Test]
	public void TestSettingConnectionString()
	{
		var csb = new ConnectionStringBuilder();
		var connectionString = "key1=value1;key2=value2";
		csb.ConnectionString = connectionString;
		Assert.That(connectionString, Is.EqualTo(csb.ConnectionString));
	}

	[Test]
	public void TestAddingKeysAndValues()
	{
		var csb = new ConnectionStringBuilder();
		csb.Add("key1", "value1");
		csb.Add("key2", "value2");

		Assert.That(csb.ContainsKey("key1"), Is.True);
		Assert.That(csb.ContainsKey("key2"), Is.True);
		Assert.That(2, Is.EqualTo(csb.Count));
	}

	[Test]
	public void TestRemovingKey()
	{
		var csb = new ConnectionStringBuilder("key1=value1;key2=value2");
		var result = csb.Remove("key1");
		Assert.That(result, Is.True);
		Assert.That(csb.ContainsKey("key1"), Is.False);
		Assert.That(1, Is.EqualTo(csb.Count));
	}

	[Test]
	public void TestGettingValueByKey()
	{
		var csb = new ConnectionStringBuilder("key1=value1;key2=value2");
		var value1 = csb["key1"];
		Assert.That("value1", Is.EqualTo(value1));

		var value2 = csb["key2"];
		Assert.That("value2", Is.EqualTo(value2));
	}
}