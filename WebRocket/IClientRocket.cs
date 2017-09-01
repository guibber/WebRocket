using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WebRocket {
  public interface IClientRocket {
    bool IsOpen { get; }
    Task<bool> ConnectAsync(Uri uri, CancellationToken token);
    Task<RocketResult> ReceiveStreamAsync(MemoryStream stream, CancellationToken token);
    Task<RocketResult> SendStreamAsync(MemoryStream stream, CancellationToken token);
    Task CloseAsync(CancellationToken token);
  }
}