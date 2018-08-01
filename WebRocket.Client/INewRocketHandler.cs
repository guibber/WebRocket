using System.Threading;
using System.Threading.Tasks;

namespace WebRocket.Client {
  public interface INewRocketHandler {
    Task HandleAsync(IRocket rocket, CancellationToken token);
  }
}