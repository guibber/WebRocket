using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using WebRocket.Client;
using WebRocket.Server;
using RocketResult = WebRocket.Client.RocketResult;

namespace WebRocketTests {
  internal class HandlerMonitor {
    public HandlerMonitor() {
      mStartsLock = new SemaphoreSlim(1, 1);
      mFinishesLock = new SemaphoreSlim(1, 1);
      Starts = new List<string>();
      Finishes = new List<string>();
    }

    public List<string> Starts {get;}
    public List<string> Finishes {get;}

    public async Task StartHandle(string id, CancellationToken token) {
      Console.WriteLine($"start handling {id}");
      await mStartsLock.WaitAsync(token);
      Starts.Add(id);
      mStartsLock.Release();
    }

    public async Task FinishHandle(string id, CancellationToken token) {
      Console.WriteLine($"finish handling {id}");
      await mFinishesLock.WaitAsync(token);
      Finishes.Add(id);
      mFinishesLock.Release();
    }

    private readonly SemaphoreSlim mFinishesLock;
    private readonly SemaphoreSlim mStartsLock;
  }

  [TestFixture]
  public class IntegrationTests {
    [Test]
    public async Task TestOneNewConnection() {
      var source = new CancellationTokenSource();
      source.CancelAfter(20000);
      await ExecuteConnectTransferAndClose(source.Token);
    }

    [Test]
    public async Task TestMultipleNewConnections() {
      var source = new CancellationTokenSource();
      source.CancelAfter(20000);
      await Task.WhenAll(ExecuteConnectTransferAndClose(source.Token),
                         ExecuteConnectTransferAndClose(source.Token),
                         ExecuteConnectTransferAndClose(source.Token));
    }

    [Test]
    public async Task TestWithLotsOfNewConnections() {
      var source = new CancellationTokenSource();
      source.CancelAfter(30000);
      await Task.WhenAll(ExecuteConnectTransferAndClose(source.Token),
                         ExecuteConnectTransferAndClose(source.Token),
                         ExecuteConnectTransferAndClose(source.Token),
                         ExecuteConnectTransferAndClose(source.Token),
                         ExecuteConnectTransferAndClose(source.Token),
                         ExecuteConnectTransferAndClose(source.Token),
                         ExecuteConnectTransferAndClose(source.Token),
                         ExecuteConnectTransferAndClose(source.Token),
                         ExecuteConnectTransferAndClose(source.Token),
                         ExecuteConnectTransferAndClose(source.Token),
                         ExecuteConnectTransferAndClose(source.Token),
                         ExecuteConnectTransferAndClose(source.Token),
                         ExecuteConnectTransferAndClose(source.Token),
                         ExecuteConnectTransferAndClose(source.Token),
                         ExecuteConnectTransferAndClose(source.Token),
                         ExecuteConnectTransferAndClose(source.Token),
                         ExecuteConnectTransferAndClose(source.Token),
                         ExecuteConnectTransferAndClose(source.Token),
                         ExecuteConnectTransferAndClose(source.Token),
                         ExecuteConnectTransferAndClose(source.Token),
                         ExecuteConnectTransferAndClose(source.Token),
                         ExecuteConnectTransferAndClose(source.Token),
                         ExecuteConnectTransferAndClose(source.Token));
    }

    [Test]
    public async Task TestAcceptingOneInProgressDoesNotBlockAcceptingNewConnections() {
      var source = new CancellationTokenSource();
      source.CancelAfter(10000);
      var tsk = ExecuteConnectTransferAndClose(5000, source.Token);
      await Task.WhenAll(ExecuteConnectTransferAndClose(source.Token),
                         ExecuteConnectTransferAndClose(source.Token),
                         ExecuteConnectTransferAndClose(source.Token),
                         ExecuteConnectTransferAndClose(source.Token),
                         ExecuteConnectTransferAndClose(source.Token));
      await tsk;
      await Task.Delay(1000, source.Token);
      Assert.AreEqual(6, mHandlerMonitor.Starts.Count);
      Assert.AreEqual(6, mHandlerMonitor.Finishes.Count);
      Assert.AreEqual(mHandlerMonitor.Starts.First(), mHandlerMonitor.Finishes.Last());
    }

    [Test]
    public async Task TestRegularHttpConnectionGetsException() {
      var source = new CancellationTokenSource();
      source.CancelAfter(1000);
      var client = new HttpClient();
      var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get,
                                                                   new Uri("http://localhost:9090/Test/")),
                                            source.Token);
      Assert.AreEqual("Internal Server Error", await response.Content.ReadAsStringAsync());
    }

    [SetUp]
    public void DoSetup() {
      mSource = new CancellationTokenSource();
      mListener = RocketListenerBuilder.Build();
      mHandlerMonitor = new HandlerMonitor();
      mAcceptingAsync = mListener.StartAcceptingAsync("http://localhost:9090/Test/", async (rocket, token) => {
                                                                                       var id = Guid.NewGuid().ToString();
                                                                                       await mHandlerMonitor.StartHandle(id, token);
                                                                                       using (var stream = new MemoryStream()) {
                                                                                         await rocket.ReceiveStreamAsync(stream, token);
                                                                                         await rocket.SendStreamAsync(stream, token);
                                                                                         await rocket.ReceiveStreamAsync(stream, token);
                                                                                       }

                                                                                       await mHandlerMonitor.FinishHandle(id, token);
                                                                                     }, mSource.Token).GetAwaiter();
    }

    [TearDown]
    public void DoTeardown() {
      mSource.Cancel();
      mListener.Stop();
      mAcceptingAsync.GetResult();
    }

    private static async Task ExecuteConnectTransferAndClose(CancellationToken token) {
      await ExecuteConnectTransferAndClose(0, token);
    }

    private static async Task ExecuteConnectTransferAndClose(int delayPostConnect, CancellationToken token) {
      var buffer = Encoding.UTF8.GetBytes("hello");
      var rocket = ClientRocketBuilder.Build();
      await rocket.ConnectAsync(new Uri("ws://localhost:9090/Test/"), token);
      await Task.Delay(delayPostConnect, token);
      Assert.That(await rocket.SendStreamAsync(new MemoryStream(buffer, 0, buffer.Length, false, true), token), Is.EqualTo(new RocketResult()));
      using (var stream = new MemoryStream()) {
        await rocket.ReceiveStreamAsync(stream, token);
        Assert.That(stream.GetBuffer().Take(buffer.Length), Is.EqualTo(buffer));
      }

      await rocket.CloseAsync(token);
    }

    private RocketListener mListener;
    private CancellationTokenSource mSource;
    private TaskAwaiter mAcceptingAsync;
    private HandlerMonitor mHandlerMonitor;
  }
}