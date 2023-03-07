using System.Data;
using Grpc.Core;
using JamesBrighton.DataProvider.Grpc;
using JamesBrighton.Data.GrpcClient.Common;
using JamesBrighton.Data.GrpcClient.Grpc;
using Moq;
using NUnit.Framework;

namespace JamesBrighton.Data.GrpcClient.Tests
{
    [TestFixture]
    public class GrpcTransactionTests
    {
        private Mock<GrpcChannel> _mockChannel;
        private IDbConnection _mockConnection;

        [SetUp]
        public void SetUp()
        {
            _mockChannel = new Mock<GrpcChannel>();
            _mockConnection = new Mock<IDbConnection>().Object;
        }

        [Test]
        public void Constructor_WithParameters_SetsProperties()
        {
            var isolationLevel = IsolationLevel.Serializable;
            var connectionIdentifier = "1234";
            var transaction = new GrpcTransaction(_mockChannel.Object, connectionIdentifier, _mockConnection, isolationLevel);

            Assert.That(transaction.Channel, Is.EqualTo(_mockChannel.Object));
            Assert.That(transaction.ConnectionIdentifier, Is.EqualTo(connectionIdentifier));
            Assert.That(transaction.Connection, Is.EqualTo(_mockConnection));
            Assert.That(transaction.IsolationLevel, Is.EqualTo(isolationLevel));
        }

        [Test]
        public void Commit_WhenChannelIsNull_ThrowsRemoteDataException()
        {
            var transaction = new GrpcTransaction() { Channel = null };

            Assert.That(() => transaction.Commit(), Throws.TypeOf<RemoteDataException>());
        }

        // Implement additional tests for the other methods in GrpcTransaction class

    }
}
