using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebRocket.Client.Wrappers {
  public interface IClientWebSocket : IWebSocket {
    Task ConnectAsync(Uri uri, CancellationToken token);
  }
}