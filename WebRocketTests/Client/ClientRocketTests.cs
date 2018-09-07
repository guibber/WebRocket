using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WebRocket.Client;
using WebRocket.Client.Wrappers;

namespace WebRocketTests.Client {
  [TestFixture]
  public class ClientRocketTests : BaseUnitTest {
    [TestCase(WebSocketState.Open, true)]
    [TestCase(WebSocketState.None, false)]
    [TestCase(WebSocketState.Aborted, false)]
    [TestCase(WebSocketState.CloseReceived, false)]
    [TestCase(WebSocketState.CloseSent, false)]
    [TestCase(WebSocketState.Connecting, false)]
    public void TestIsOpen(WebSocketState state, bool expected) {
      mSocket.SetupGet(m => m.State).Returns(state);
      Assert.That(mRocket.IsOpen, Is.EqualTo(expected));
    }

    [Test]
    public async Task TestReceiveStreamAsyncWhenClosedMessageReceived() {
      var source = new CancellationTokenSource();
      var buffer = new byte[32];
      mResult.SetupGet(m => m.MessageType)
             .Returns(WebSocketMessageType.Close);
      mSocket.Setup(m => m.ReceiveAsync(new ArraySegment<byte>(new byte[8192]), source.Token))
             .Returns(Task.FromResult(mResult.Object));
      mSocket.Setup(m => m.CloseAsync(WebSocketCloseStatus.NormalClosure, "normal closure", source.Token))
             .Returns(Task.FromResult(true));
      using (var stream = new MemoryStream(buffer)) {
        stream.Position = 32;
        Assert.That(await mRocket.ReceiveStreamAsync(stream, source.Token), Is.EqualTo(new RocketResult().SetClosed()));
        Assert.That(stream.Position, Is.Zero);
      }
    }

    [Test]
    public void TestReceiveStreamAsyncWithExceptionThrows() {
      var source = new CancellationTokenSource();
      mSocket.Setup(m => m.ReceiveAsync(new ArraySegment<byte>(new byte[8192]), source.Token))
             .Throws(new Exception());

      using (var stream = new MemoryStream()) {
        Assert.ThrowsAsync<Exception>(async () => await mRocket.ReceiveStreamAsync(stream, source.Token));
      }
    }

    [Test]
    public async Task TestReceiveStreamAsyncWithMultipartMessage() {
      var source = new CancellationTokenSource();
      var receiveBuffer = new byte[64];
      var expected = new byte[] {
                                  0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8,
                                  0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8,
                                  0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8,
                                  0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8,
                                  0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8,
                                  0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8
                                };
      var endMessageValues = new Queue<bool>(new[] {false, false, true});
      mResult.SetupGet(m => m.MessageType).Returns(WebSocketMessageType.Binary);
      mResult.SetupGet(m => m.Count).Returns(16);
      mResult.SetupGet(m => m.EndOfMessage).Returns(() => endMessageValues.Dequeue());
      mSocket.Setup(m => m.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), source.Token))
             .Callback<ArraySegment<byte>, CancellationToken>((b, t) => {
                                                                Array.Copy(expected, b.Array, 48);
                                                                b.Array[48] = 0x11;
                                                              })
             .Returns(Task.FromResult(mResult.Object));

      using (var stream = new MemoryStream(receiveBuffer)) {
        stream.Position = 1;
        Assert.That(await mRocket.ReceiveStreamAsync(stream, source.Token), Is.EqualTo(new RocketResult()));
        Assert.That(stream.Position, Is.Zero);
        Assert.That(receiveBuffer.Take(48), Is.EqualTo(expected));
        Assert.That(receiveBuffer.Skip(48).Take(16), Is.EqualTo(new byte[16]));
      }
    }

    [Test]
    public async Task TestReceiveStreamAsyncWithOnePartMessage() {
      var source = new CancellationTokenSource();
      var receiveBuffer = new byte[32];
      var expected = new byte[] {
                                  0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8,
                                  0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8
                                };
      mResult.SetupGet(m => m.MessageType).Returns(WebSocketMessageType.Binary);
      mResult.SetupGet(m => m.Count).Returns(16);
      mResult.SetupGet(m => m.EndOfMessage).Returns(true);
      mSocket.Setup(m => m.ReceiveAsync(new ArraySegment<byte>(new byte[8192]), source.Token))
             .Callback<ArraySegment<byte>, CancellationToken>((b, t) => {
                                                                Array.Copy(expected, b.Array, 16);
                                                                b.Array[17] = 0x11;
                                                              })
             .Returns(Task.FromResult(mResult.Object));

      using (var stream = new MemoryStream(receiveBuffer)) {
        stream.Position = 1;
        Assert.That(await mRocket.ReceiveStreamAsync(stream, source.Token), Is.EqualTo(new RocketResult()));
        Assert.That(stream.Position, Is.Zero);
        Assert.That(receiveBuffer.Take(16), Is.EqualTo(expected));
        Assert.That(receiveBuffer.Skip(16).Take(16), Is.EqualTo(new byte[16]));
      }
    }

    [Test]
    public async Task TestReceiveStreamAsyncProtectsToOneAtATime() {
      var source = new CancellationTokenSource();
      var expected = new byte[] {
                                  0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8,
                                  0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8
                                };
      var activeReceives = new ProtectedInt();
      var startCounts = new List<int>();
      var endCounts = new List<int>();
      mResult.SetupGet(m => m.MessageType).Returns(WebSocketMessageType.Binary);
      mResult.SetupGet(m => m.Count).Returns(16);
      mResult.SetupGet(m => m.EndOfMessage).Returns(true);
      mSocket.Setup(m => m.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), source.Token))
             .Callback<ArraySegment<byte>, CancellationToken>(async (b, t) => {
                                                                startCounts.Add(await activeReceives.IncrementAsync());
                                                                Array.Copy(expected, b.Array, 16);
                                                                Thread.Sleep(10);
                                                                endCounts.Add(await activeReceives.DecrementAsync());
                                                              })
             .Returns(Task.FromResult(mResult.Object));

      await Task.WhenAll(StartReceivAsyncTasks(expected, source.Token));
      Assert.False(startCounts.Any(c => c != 1));
      Assert.False(endCounts.Any(c => c != 0));
    }

    [Test]
    public async Task TestReceiveStreamAsyncWithWebSocketExceptionReturnsClosedResult() {
      var source = new CancellationTokenSource();
      var expected = new WebSocketException();
      mSocket.Setup(m => m.ReceiveAsync(new ArraySegment<byte>(new byte[8192]), source.Token))
             .Throws(expected);

      using (var stream = new MemoryStream()) {
        Assert.That(await mRocket.ReceiveStreamAsync(stream, source.Token),
                    Is.EqualTo(new RocketResult().SetException(expected)
                                                 .SetClosed()));
      }
    }

    [Test]
    public async Task TestSendStreamAsync() {
      var source = new CancellationTokenSource();
      var sendBuffer = new byte[] {
                                    0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8,
                                    0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8
                                  };
      mSocket.Setup(m => m.SendAsync(new ArraySegment<byte>(sendBuffer, 0, sendBuffer.Length), WebSocketMessageType.Binary, true, source.Token))
             .Returns(Task.FromResult(true));

      using (var stream = new MemoryStream(sendBuffer, 0, sendBuffer.Length, false, true)) {
        Assert.That(await mRocket.SendStreamAsync(stream, source.Token), Is.EqualTo(new RocketResult()));
      }
    }

    [Test]
    public async Task TestSendStreamAsyncReturnsClosedWhenWebSocketException() {
      var source = new CancellationTokenSource();
      var expected = new WebSocketException();
      var sendBuffer = new byte[] {
                                    0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8,
                                    0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8
                                  };
      mSocket.Setup(m => m.SendAsync(new ArraySegment<byte>(sendBuffer, 0, sendBuffer.Length), WebSocketMessageType.Binary, true, source.Token))
             .Throws(expected);

      using (var stream = new MemoryStream(sendBuffer, 0, sendBuffer.Length, false, true)) {
        Assert.That(await mRocket.SendStreamAsync(stream, source.Token),
                    Is.EqualTo(new RocketResult().SetException(expected)
                                                 .SetClosed()));
      }
    }

    [Test]
    public async Task TestSendStreamAsyncProtectsToOneAtATime() {
      var source = new CancellationTokenSource();
      var activeSends = new ProtectedInt();
      var startCounts = new List<int>();
      var endCounts = new List<int>();
      mSocket.Setup(m => m.SendAsync(It.IsAny<ArraySegment<byte>>(), WebSocketMessageType.Binary, true, source.Token))
             .Callback<ArraySegment<byte>, WebSocketMessageType, bool, CancellationToken>(async (b, t, f, token) => {
                                                                                            startCounts.Add(await activeSends.IncrementAsync());
                                                                                            Thread.Sleep(10);
                                                                                            endCounts.Add(await activeSends.DecrementAsync());
                                                                                          })
             .Returns(Task.FromResult(true));
      await Task.WhenAll(StartSendAsyncTasks(source.Token));
      Assert.False(startCounts.Any(c => c != 1));
      Assert.False(endCounts.Any(c => c != 0));
    }

    [TestCase(WebSocketState.None)]
    [TestCase(WebSocketState.Aborted)]
    [TestCase(WebSocketState.CloseReceived)]
    [TestCase(WebSocketState.CloseSent)]
    [TestCase(WebSocketState.Connecting)]
    public async Task TestCloseAsyncDoesNothingWhenSocketNotOpen(WebSocketState state) {
      mSocket.SetupGet(m => m.State).Returns(state);
      await mRocket.CloseAsync(CancellationToken.None);
    }

    [Test]
    public async Task TestCloseAsyncWhenSocketIsOpen() {
      var source = new CancellationTokenSource();
      mSocket.SetupGet(m => m.State).Returns(WebSocketState.Open);
      mSocket.Setup(m => m.CloseAsync(WebSocketCloseStatus.NormalClosure, "normal closure", source.Token))
             .Returns(Task.FromResult(true));
      await mRocket.CloseAsync(source.Token);
    }

    [Test]
    public async Task TestCloseAsyncSilencesWebSocketException() {
      var source = new CancellationTokenSource();
      mSocket.SetupGet(m => m.State).Returns(WebSocketState.Open);
      mSocket.Setup(m => m.CloseAsync(WebSocketCloseStatus.NormalClosure, "normal closure", source.Token))
             .Throws<WebSocketException>();
      await mRocket.CloseAsync(source.Token);
    }

    [Test]
    public async Task TestConnectAsyncSilencesWebSocketExceptionAndReturnsFalse() {
      var uri = new Uri("ws://localhost2");
      mSocket.Setup(m => m.ConnectAsync(uri, CancellationToken.None))
             .Throws<WebSocketException>();
      Assert.False(await mRocket.ConnectAsync(uri, CancellationToken.None));
    }

    [SetUp]
    public async Task DoSetup() {
      mResult = Mock<IWebSocketReceiveResult>();
      mSocket = Mock<IClientWebSocket>();
      mBuilder = Mock<IClientWebSocketBuilder>();
      mBuilder.Setup(m => m.Build())
              .Returns(mSocket.Object);
      var uri = new Uri("ws://localhost");
      mSocket.Setup(m => m.ConnectAsync(uri, CancellationToken.None))
             .Returns(Task.FromResult(true));
      mRocket = new ClientRocket(mBuilder.Object);
      Assert.True(await mRocket.ConnectAsync(uri, CancellationToken.None));
    }

    private Task[] StartSendAsyncTasks(CancellationToken token) {
      return Enumerable.Range(0, 10).Select(i => {
                                              return Task.Run(() => {
                                                                using (var stream = new MemoryStream()) {
                                                                  var awaiter = mRocket.SendStreamAsync(stream, token).GetAwaiter();
                                                                  Assert.That(awaiter.GetResult(), Is.EqualTo(new RocketResult()));
                                                                }
                                                              }, token);
                                            }).ToArray();
    }

    private Task[] StartReceivAsyncTasks(byte[] expected, CancellationToken token) {
      return Enumerable.Range(0, 10).Select(i => {
                                              return Task.Run(() => {
                                                                using (var stream = new MemoryStream()) {
                                                                  var awaiter = mRocket.ReceiveStreamAsync(stream, token).GetAwaiter();
                                                                  Assert.That(awaiter.GetResult(), Is.EqualTo(new RocketResult()));
                                                                  Assert.That(stream.Position, Is.Zero);
                                                                  Assert.That(stream.GetBuffer().Take(16), Is.EqualTo(expected));
                                                                }
                                                              }, token);
                                            }).ToArray();
    }

    private ClientRocket mRocket;
    private Mock<IClientWebSocketBuilder> mBuilder;
    private Mock<IWebSocketReceiveResult> mResult;
    private Mock<IClientWebSocket> mSocket;
  }
}