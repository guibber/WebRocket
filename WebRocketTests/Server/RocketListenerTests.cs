using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WebRocket.Server;
using WebRocket.Server.Wrappers;

namespace WebRocketTests.Server {
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
    public async Task TestStartAcceptingAsyncContinuesOnExceptionInHandleNewRocketWhenCompletesSynchronouslyAsync() {
      var callCount = 0;
      var source = new CancellationTokenSource();
      const string address = "address";
      var expectedException = new Exception("expected");
      mHttpListener.Setup(m => m.AddPrefix(address));
      mHttpListener.Setup(m => m.Start());
      mHttpListener.SetupGet(m => m.IsListening).Returns(true);
      mHttpListener.Setup(m => m.GetContextAsync()).Returns(Task.FromResult(mListenerContext.Object));
      mAcceptor.Setup(m => m.AcceptAsync(mListenerContext.Object))
               .Returns(Task.FromResult((IRocket)new Rocket(null)));
      mObserver.Setup(m => m.NoticeHandleNewRocketExceptionAsync(expectedException, source.Token))
               .Returns(Task.FromResult(true));
      source.CancelAfter(5000);
      await mListener.StartAcceptingAsync(address, (rocket, token) => {
                                                     if (callCount++ < 10)
                                                       throw expectedException;
                                                     return Task.FromResult(callCount);
                                                   }, source.Token);
      Assert.That(callCount, Is.GreaterThan(10));
      mObserver.Verify(m => m.NoticeHandleNewRocketExceptionAsync(expectedException, source.Token), Times.AtLeast(10));
    }

    [Test]
    public async Task TestStartAcceptingAsyncContinuesOnExceptionInHandleNewRocketWhenRunsAsynchronouslyAsync() {
      var callCount = 0;
      var source = new CancellationTokenSource();
      const string address = "address";
      var expectedException = new Exception("expected");
      mHttpListener.Setup(m => m.AddPrefix(address));
      mHttpListener.Setup(m => m.Start());
      mHttpListener.SetupGet(m => m.IsListening).Returns(true);
      mHttpListener.Setup(m => m.GetContextAsync()).Returns(Task.FromResult(mListenerContext.Object));
      mAcceptor.Setup(m => m.AcceptAsync(mListenerContext.Object))
               .Returns(Task.FromResult((IRocket)new Rocket(null)));
      mObserver.Setup(m => m.NoticeHandleNewRocketExceptionAsync(It.Is<AggregateException>(ex => ex.InnerExceptions[0].Equals(expectedException)), source.Token))
               .Returns(Task.FromResult(true));
      source.CancelAfter(5000);
      await mListener.StartAcceptingAsync(address, async (rocket, token) => {
                                                     await Task.Delay(100, token);
                                                     if (callCount++ < 10)
                                                       throw expectedException;
                                                     await Task.FromResult(callCount);
                                                   }, source.Token);
      Assert.That(callCount, Is.GreaterThan(10));
      mObserver.Verify(m => m.NoticeHandleNewRocketExceptionAsync(It.Is<AggregateException>(ex => ex.InnerExceptions[0].Equals(expectedException)), source.Token), Times.AtLeast(10));
    }

    [Test]
    public async Task TestStartAcceptingAsyncContinuesOnExceptionAcceptingAsync() {
      var source = new CancellationTokenSource();
      const string address = "address";
      var expectedException = new Exception("expected");
      mHttpListener.Setup(m => m.AddPrefix(address));
      mHttpListener.Setup(m => m.Start());
      mHttpListener.SetupGet(m => m.IsListening).Returns(true);
      mHttpListener.Setup(m => m.GetContextAsync()).Returns(Task.FromResult(mListenerContext.Object));
      mAcceptor.Setup(m => m.AcceptAsync(mListenerContext.Object))
               .Throws(expectedException);
      mObserver.Setup(m => m.NoticeAcceptExceptionAsync(expectedException, source.Token))
               .Returns(Task.FromResult(true));
      source.CancelAfter(200);
      await mListener.StartAcceptingAsync(address, (rocket, token) => Task.FromResult(true), source.Token);
      mObserver.Verify(m => m.NoticeAcceptExceptionAsync(expectedException, source.Token), Times.AtLeast(10));
    }

    [Test]
    public async Task TestStartAcceptingAsyncContinuesOnExceptionGetContextAsync() {
      var source = new CancellationTokenSource();
      const string address = "address";
      var expectedException = new Exception("expected");
      mHttpListener.Setup(m => m.AddPrefix(address));
      mHttpListener.Setup(m => m.Start());
      mHttpListener.SetupGet(m => m.IsListening).Returns(true);
      mHttpListener.Setup(m => m.GetContextAsync())
                   .Throws(expectedException);
      mObserver.Setup(m => m.NoticeAcceptExceptionAsync(expectedException, source.Token))
               .Returns(Task.FromResult(true));
      source.CancelAfter(200);
      await mListener.StartAcceptingAsync(address, (rocket, token) => Task.FromResult(true), source.Token);
      mObserver.Verify(m => m.NoticeAcceptExceptionAsync(expectedException, source.Token), Times.AtLeast(10));
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
      mObserver = Mock<IObserver>();
      mListener = new RocketListener(mHttpListener.Object, mAcceptor.Object, mObserver.Object);
    }

    private RocketListener mListener;
    private Mock<IHttpListerner> mHttpListener;
    private Mock<IRocketAcceptor> mAcceptor;
    private Mock<IHttpListenerContext> mListenerContext;
    private Mock<IObserver> mObserver;
  }
}