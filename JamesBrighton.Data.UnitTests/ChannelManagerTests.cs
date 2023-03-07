using JamesBrighton.Data.GrpcClient.Common;
using NUnit.Framework;

namespace JamesBrighton.Data.UnitTests;

public class ChannelManagerTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void ChannelManager_GetChannel_Returns_Valid_Channel()
    {
        // Arrange
        var address = "http://localhost:5000";

        // Act
        var channelMgr = new ChannelManager(address);
        var channel = channelMgr.Channel;

        // Assert
        Assert.IsNotNull(channel);
        Assert.AreEqual("localhost:5000", channel.Target);
    }

    [Test]
    public void ChannelManager_Reuse_Same_Channel_Returns_Same_Instance()
    {
        // Arrange
        var address = "http://localhost:5000";

        // Act
        var channelMgr1 = new ChannelManager(address);
        var channel1 = channelMgr1.Channel;
        var channelMgr2 = new ChannelManager(address);
        var channel2 = channelMgr2.Channel;

        // Assert
        Assert.IsNotNull(channel1);
        Assert.IsNotNull(channel2);
        Assert.AreEqual(channel1, channel2);
    }
}