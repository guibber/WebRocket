using System.Threading;
using System.Threading.Tasks;

namespace WebRocket.Server {
  public interface INewRocketHandler {
    Task HandleAsync(IRocket rocket, CancellationToken token);
  }
}