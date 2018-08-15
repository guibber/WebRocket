using System;
using System.Threading;
using System.Threading.Tasks;
using WebRocket.Server.Wrappers;

namespace WebRocket.Server {
  public class RocketListener : IRocketListener {
    public RocketListener(IHttpListerner listener, IRocketAcceptor acceptor, IObserver observer) {
      mListener = listener;
      mAcceptor = acceptor;
      mObserver = observer;
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
          await handleNewRocket(await mAcceptor.AcceptAsync(await mListener.GetContextAsync()), token);
        } catch (Exception ex) {
          await mObserver.NoticeAcceptExceptionAsync(ex, token);
        }
    }

    private readonly IRocketAcceptor mAcceptor;
    private readonly IHttpListerner mListener;
    private readonly IObserver mObserver;
  }
}