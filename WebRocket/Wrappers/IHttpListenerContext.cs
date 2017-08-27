using System.Threading.Tasks;

namespace WebRocket.Wrappers {
  public interface IHttpListenerContext {
    IHttpListenerResponse Response {get;}
    Task<IHttpListenerWebSocketContext> AcceptWebSocketAsync();
  }
}