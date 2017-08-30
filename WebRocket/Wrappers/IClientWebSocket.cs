using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebRocket.Wrappers {
  public interface IClientWebSocket : IWebSocket {
    Task ConnectAsync(Uri uri, CancellationToken token);
  }
}