using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebRocket.Server {
  public interface IObserver {
    Task NoticeAcceptExceptionAsync(Exception ex, CancellationToken token);
    Task NoticeHandleNewRocketExceptionAsync(Exception ex, CancellationToken token);

  }
}