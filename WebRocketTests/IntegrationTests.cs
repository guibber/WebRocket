using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using WebRocket.Client;
using WebRocket.Client.Wrappers;
using WebRocket.Server;
using WebRocket.Server.Wrappers;
using RocketResult = WebRocket.Client.RocketResult;

namespace WebRocketTests {
  [TestFixture]
  public class IntegrationTests {
    [Test]
    public async Task TestOneNewConnection() {
      await ExecuteConnectTransferAndClose();
    }

    [Test]
    public void TestMulitpleNewConnections() {
      Task.WaitAll(ExecuteConnectTransferAndClose(),
                   ExecuteConnectTransferAndClose(),
                   ExecuteConnectTransferAndClose());
    }

    [Test]
    public void TestWithLotsOfNewConnections() {
      Task.WaitAll(ExecuteConnectTransferAndClose(),
                   ExecuteConnectTransferAndClose(),
                   ExecuteConnectTransferAndClose(),
                   ExecuteConnectTransferAndClose(),
                   ExecuteConnectTransferAndClose(),
                   ExecuteConnectTransferAndClose(),
                   ExecuteConnectTransferAndClose(),
                   ExecuteConnectTransferAndClose(),
                   ExecuteConnectTransferAndClose(),
                   ExecuteConnectTransferAndClose(),
                   ExecuteConnectTransferAndClose(),
                   ExecuteConnectTransferAndClose());
    }

    [SetUp]
    public void DoSetup() {
      mSource = new CancellationTokenSource();
      mListener = RocketListenerBuilder.Build();
      mAcceptingAsync = mListener.StartAcceptingAsync("http://localhost:9090/Test/", async (rocket, token) => {
                                                                                       using (var stream = new MemoryStream()) {
                                                                                         await rocket.ReceiveStreamAsync(stream, token);
                                                                                         await rocket.SendStreamAsync(stream, token);
                                                                                         await rocket.ReceiveStreamAsync(stream, token);
                                                                                       }
                                                                                     }, mSource.Token).GetAwaiter();
    }

    [TearDown]
    public void DoTeardown() {
      mSource.Cancel();
      mListener.Stop();
      mAcceptingAsync.GetResult();
    }

    private static async Task ExecuteConnectTransferAndClose() {
      var source = new CancellationTokenSource();
      var buffer = Encoding.UTF8.GetBytes("hello");
      var rocket = ClientRocketBuilder.Build();
      Assert.True(await rocket.ConnectAsync(new Uri("ws://localhost:9090/Test/"), source.Token));
      Assert.That(await rocket.SendStreamAsync(new MemoryStream(buffer, 0, buffer.Length, false, true), source.Token), Is.EqualTo(new RocketResult()));
      using (var stream = new MemoryStream()) {
        await rocket.ReceiveStreamAsync(stream, source.Token);
        Assert.That(stream.GetBuffer().Take(buffer.Length), Is.EqualTo(buffer));
      }
      await rocket.CloseAsync(source.Token);
    }

    private RocketListener mListener;
    private CancellationTokenSource mSource;
    private TaskAwaiter mAcceptingAsync;
  }
}