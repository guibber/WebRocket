using System;
using NUnit.Framework;
using WebRocket.Server;

namespace WebRocketTests.Server {
  [TestFixture]
  public class RocketResultTests {
    [Test]
    public void TestDefault() {
      Assert.False(mResult.IsSocketClosed);
      Assert.IsNull(mResult.Exception);
    }

    [Test]
    public void TestSetClosed() {
      Assert.True(mResult.SetClosed().IsSocketClosed);
    }

    [Test]
    public void TestSetException() {
      var expected = new Exception();
      Assert.AreEqual(expected, mResult.SetException(expected).Exception);
    }

    [SetUp]
    public void DoSetup() {
      mResult = new RocketResult();
    }

    private RocketResult mResult;
  }
}