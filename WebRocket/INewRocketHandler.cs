using System.Threading;
using System.Threading.Tasks;

namespace WebRocket {
  public interface INewRocketHandler {
    Task HandleAsync(IRocket rocket, CancellationToken token);
  }
}