using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebRocket {
  public interface IRocketListener {
    Task StartAcceptingAsync(string address, Func<IRocket, CancellationToken, Task> handleNewRocket, CancellationToken token);
    void Stop();
  }
}