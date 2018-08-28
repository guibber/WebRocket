using System;
using System.Runtime.CompilerServices;
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
      await new PublicSynchronizationContextManager();
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
          var rocket = await mAcceptor.AcceptAsync(await mListener.GetContextAsync());
          try {
            handleNewRocket(rocket, token).ContinueWith(t => ContinueHandleNewRocket(t, token), TaskContinuationOptions.OnlyOnFaulted).GetAwaiter();
          } catch (Exception ex) {
            await mObserver.NoticeHandleNewRocketExceptionAsync(ex, token);
          }
        } catch (Exception ex) {
          await mObserver.NoticeAcceptExceptionAsync(ex, token);
        }
    }

    private async Task ContinueHandleNewRocket(Task tsk, CancellationToken token) {
      await mObserver.NoticeHandleNewRocketExceptionAsync(tsk.Exception, token);
    }

    private readonly IRocketAcceptor mAcceptor;
    private readonly IHttpListerner mListener;
    private readonly IObserver mObserver;
  }
}