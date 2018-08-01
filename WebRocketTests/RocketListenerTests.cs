using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WebRocket.Server;
using WebRocket.Server.Wrappers;

namespace WebRocketTests {
  [TestFixture]
  public class RocketListenerTests : BaseUnitTest {
    [Test]
    public async Task TestStartAcceptingAsyncAccepts() {
      var callCount = 0;
      var source = new CancellationTokenSource();
      var expected = Mock<IHttpListenerWebSocketContext>();
      const string address = "address";
      mHttpListener.Setup(m => m.AddPrefix(address));
      mHttpListener.Setup(m => m.Start());
      mHttpListener
        .SetupGet(m => m.IsListening)
        .Returns(true);
      mHttpListener
        .Setup(m => m.GetContextAsync())
        .Returns(Task.FromResult(mListenerContext.Object));
      mAcceptor
        .Setup(m => m.AcceptAsync(mListenerContext.Object))
        .Returns(Task.FromResult((IRocket)new Rocket(expected.Object)));
      source.CancelAfter(100);
      await mListener.StartAcceptingAsync(address, (rocket, token) => Task.FromResult(callCount++), source.Token);
      Assert.That(callCount, Is.GreaterThan(0));
    }

    [Test]
    public async Task TestStartAcceptingAsyncContinuesOnException() {
      var callCount = 0;
      var source = new CancellationTokenSource();
      const string address = "address";
      mHttpListener.Setup(m => m.AddPrefix(address));
      mHttpListener.Setup(m => m.Start());
      mHttpListener.SetupGet(m => m.IsListening).Returns(true);
      mHttpListener.Setup(m => m.GetContextAsync()).Returns(Task.FromResult(mListenerContext.Object));
      mAcceptor.Setup(m => m.AcceptAsync(mListenerContext.Object))
               .Returns(Task.FromResult((IRocket)new Rocket(null)));
      source.CancelAfter(100);
      await mListener.StartAcceptingAsync(address, (rocket, token) => {
                                             if (callCount++ < 10)
                                               throw new Exception("Any Exception");
                                             return Task.FromResult(callCount++);
                                           }, source.Token);
      Assert.That(callCount, Is.GreaterThan(10));
    }

    [Test]
    public async Task TestStartAcceptingAsyncDoesNothingIfNotListening() {
      var callCount = 0;
      const string address = "address";
      mHttpListener.Setup(m => m.AddPrefix(address));
      mHttpListener.Setup(m => m.Start());
      mHttpListener.SetupGet(m => m.IsListening).Returns(false);
      await mListener.StartAcceptingAsync(address, (rocket, token) => Task.FromResult(callCount++), CancellationToken.None);
      Assert.That(callCount, Is.EqualTo(0));
    }

    [Test]
    public async Task TestStartAcceptingAsyncDoesNothingIfTokenCancelled() {
      var callCount = 0;
      var source = new CancellationTokenSource();
      source.Cancel();
      const string address = "address";
      mHttpListener.Setup(m => m.AddPrefix(address));
      mHttpListener.Setup(m => m.Start());
      mHttpListener.SetupGet(m => m.IsListening).Returns(true);
      await mListener.StartAcceptingAsync(address, (rocket, token) => Task.FromResult(callCount++), source.Token);
      Assert.That(callCount, Is.EqualTo(0));
    }
    
    [Test]
    public void TestStop() {
      mHttpListener.Setup(m => m.Stop());
      mListener.Stop();
    }

    [SetUp]
    public void DoSetup() {
      mHttpListener = Mock<IHttpListerner>();
      mAcceptor = new Mock<IRocketAcceptor>();
      mListenerContext = Mock<IHttpListenerContext>();
      mListener = new RocketListener(mHttpListener.Object, mAcceptor.Object);
    }

    private RocketListener mListener;
    private Mock<IHttpListerner> mHttpListener;
    private Mock<IRocketAcceptor> mAcceptor;
    private Mock<IHttpListenerContext> mListenerContext;
  }
}