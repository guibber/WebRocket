using System.Threading.Tasks;

namespace WebRocket.Server.Wrappers {
  public interface IHttpListenerContext {
    IHttpListenerResponse Response {get;}
    Task<IHttpListenerWebSocketContext> AcceptWebSocketAsync();
  }
}