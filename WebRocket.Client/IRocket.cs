using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WebRocket.Client {
  public interface IRocket {
    bool IsOpen { get; }
    Task<RocketResult> ReceiveStreamAsync(MemoryStream stream, CancellationToken token);
    Task<RocketResult> SendStreamAsync(MemoryStream stream, CancellationToken token);
    Task CloseAsync(CancellationToken token);
  }
}