using System.Threading.Tasks;
using WebRocket.Wrappers;

namespace WebRocket {
  public interface IRocketAcceptor {
    Task<IRocket> AcceptAsync(IHttpListenerContext context);
  }
}