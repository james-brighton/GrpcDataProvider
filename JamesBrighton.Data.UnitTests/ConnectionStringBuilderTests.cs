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
        Assert.AreEqual(expected, csb.ConnectionString);
    }

    [Test]
    public void TestConstructionWithStringArg()
    {
        var csb = new ConnectionStringBuilder("key1=value1;key2=value2");
        var expected = "key1=value1;key2=value2";
        Assert.AreEqual(expected, csb.ConnectionString);
    }

    [Test]
    public void TestSettingConnectionString()
    {
        var csb = new ConnectionStringBuilder();
        var connectionString = "key1=value1;key2=value2";
        csb.ConnectionString = connectionString;
        Assert.AreEqual(connectionString, csb.ConnectionString);
    }

    [Test]
    public void TestAddingKeysAndValues()
    {
        var csb = new ConnectionStringBuilder();
        csb.Add("key1", "value1");
        csb.Add("key2", "value2");

        Assert.True(csb.ContainsKey("key1"));
        Assert.True(csb.ContainsKey("key2"));
        Assert.AreEqual(2, csb.Count);
    }

    [Test]
    public void TestRemovingKey()
    {
        var csb = new ConnectionStringBuilder("key1=value1;key2=value2");
        var result = csb.Remove("key1");
        Assert.True(result);
        Assert.False(csb.ContainsKey("key1"));
        Assert.AreEqual(1, csb.Count);
    }

    [Test]
    public void TestGettingValueByKey()
    {
        var csb = new ConnectionStringBuilder("key1=value1;key2=value2");
        var value1 = csb["key1"];
        Assert.AreEqual("value1", value1);

        var value2 = csb["key2"];
        Assert.AreEqual("value2", value2);
    }
}