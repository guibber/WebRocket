using NUnit.Framework;
using WebRocket.Server;

namespace WebRocketTests {
  [TestFixture]
  public class RocketResultTests {
    [Test]
    public void TestDefault() {
      Assert.False(mResult.IsSocketClosed);
    }

    [Test]
    public void TestSetClosed() {
      Assert.True(mResult.SetClosed().IsSocketClosed);
    }

    [SetUp]
    public void DoSetup() {
      mResult = new RocketResult();
    }

    private RocketResult mResult;
  }
}