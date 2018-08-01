using System.Threading.Tasks;

namespace WebRocket.Server.Wrappers {
  public class HttpListenerContext : IHttpListenerContext {
    public HttpListenerContext(System.Net.HttpListenerContext context) {
      mContext = context;
    }

    public IHttpListenerResponse Response => new HttpListenerResponse(mContext.Response);

    public async Task<IHttpListenerWebSocketContext> AcceptWebSocketAsync() {
      return new HttpListenerWebSocketContext(await mContext.AcceptWebSocketAsync(null));
    }

    private readonly System.Net.HttpListenerContext mContext;
  }
}