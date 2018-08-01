using System;
using System.Threading;
using System.Threading.Tasks;
using WebRocket.Server.Wrappers;

namespace WebRocket.Server {
  public class RocketListener : IRocketListener {
    public RocketListener(IHttpListerner listener, IRocketAcceptor acceptor) {
      mListener = listener;
      mAcceptor = acceptor;
    }

    public async Task StartAcceptingAsync(string address, Func<IRocket, CancellationToken, Task> handleNewRocket, CancellationToken token) {
      mListener.AddPrefix(address);
      mListener.Start();
      await DoAcceptingLoopAsync(handleNewRocket, token);
    }

    public void Stop() {
      mListener.Stop();
    }

    private async Task DoAcceptingLoopAsync(Func<IRocket, CancellationToken, Task> handleNewRocket, CancellationToken token) {
      while (mListener.IsListening && !token.IsCancellationRequested)
        try {
          handleNewRocket(await mAcceptor.AcceptAsync(await mListener.GetContextAsync()), token).GetAwaiter();
        } catch { }
    }

    private readonly IRocketAcceptor mAcceptor;
    private readonly IHttpListerner mListener;
  }
}