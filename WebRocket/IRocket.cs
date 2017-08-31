using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WebRocket {
  public interface IRocket {
    Task<RocketResult> ReceiveStreamAsync(MemoryStream stream, CancellationToken token);
    Task<RocketResult> SendStreamAsync(MemoryStream stream, CancellationToken token);
  }
}