using System.Threading.Tasks;
using WebRocket.Server.Wrappers;

namespace WebRocket.Server {
  public interface IRocketAcceptor {
    Task<IRocket> AcceptAsync(IHttpListenerContext context);
  }
}