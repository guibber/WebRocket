using System;
using System.Threading;
using System.Threading.Tasks;
using WebRocket.Wrappers;

namespace WebRocket {
  public class RocketListener : IRocketListener {
    public RocketListener(IHttpListerner listener, IRocketAcceptor acceptor) {
      mListener = listener;
      mAcceptor = acceptor;
    }

    public void Start(string address) {
      mListener.AddPrefix(address);
      mListener.Start();
    }

    public void Stop() {
      mListener.Stop();
    }

    public async Task DoAcceptingLoopAsync(Func<IRocket, CancellationToken, Task> handleNewRocket, CancellationToken token) {
      while (mListener.IsListening && !token.IsCancellationRequested)
        try {
          handleNewRocket(await mAcceptor.AcceptAsync(await mListener.GetContextAsync()), token).GetAwaiter();
        } catch { }
    }

    private readonly IRocketAcceptor mAcceptor;
    private readonly IHttpListerner mListener;
  }
}