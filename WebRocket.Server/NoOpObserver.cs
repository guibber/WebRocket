using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebRocket.Server {
  public class NoOpObserver : IObserver {
    public Task NoticeAcceptExceptionAsync(Exception ex, CancellationToken token) {
      return Task.FromResult(true);
    }
  }
}