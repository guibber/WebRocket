using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WebRocket.Server;
using WebRocket.Server.Wrappers;

namespace WebRocketTests.Server {
  [TestFixture]
  public class RocketAcceptorTests : BaseUnitTest {
    [Test]
    public async Task TestAcceptAsync() {
      var webSocketContext = Mock<IHttpListenerWebSocketContext>();
      var webSocket = Mock<IWebSocket>();
      webSocketContext.SetupGet(m => m.WebSocket).Returns(webSocket.Object);
      webSocket.SetupGet(m => m.State).Returns(WebSocketState.Open);
      mListenerContext
        .Setup(m => m.AcceptWebSocketAsync())
        .Returns(Task.FromResult(webSocketContext.Object));
      Assert.That(await mAcceptor.AcceptAsync(mListenerContext.Object), Is.EqualTo(new Rocket(webSocketContext.Object)));
    }

    [Test]
    public void TestAcceptAsyncSetsResponseAndThrowsOnException() {
      var response = Mock<IHttpListenerResponse>();
      var buffer = new byte[21];
      using (var stream = new MemoryStream(buffer)) {
        mListenerContext
          .Setup(m => m.AcceptWebSocketAsync())
          .Throws(new Exception());
        mListenerContext
          .SetupGet(m => m.Response)
          .Returns(response.Object);
        response.SetupSet(m => m.ContentLength64 = 21);
        response.SetupSet(m => m.StatusCode = 500);
        response.SetupGet(m => m.OutputStream).Returns(stream);
        Assert.ThrowsAsync<Exception>(async () => await mAcceptor.AcceptAsync(mListenerContext.Object));
        Assert.That(Encoding.UTF8.GetString(buffer), Is.EqualTo("Internal Server Error"));
        Assert.False(stream.CanWrite);
        Assert.False(stream.CanRead);
      }
    }

    [SetUp]
    public void DoSetup() {
      mListenerContext = Mock<IHttpListenerContext>();
      mAcceptor = new RocketAcceptor();
    }

    private RocketAcceptor mAcceptor;
    private Mock<IHttpListenerContext> mListenerContext;
  }
}