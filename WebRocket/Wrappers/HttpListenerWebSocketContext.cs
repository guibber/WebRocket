namespace WebRocket.Wrappers {
  public class HttpListenerWebSocketContext : IHttpListenerWebSocketContext {
    public HttpListenerWebSocketContext(System.Net.WebSockets.HttpListenerWebSocketContext context) {
      mContext = context;
    }

    public IWebSocket WebSocket => new WebSocket(mContext.WebSocket);
    private readonly System.Net.WebSockets.HttpListenerWebSocketContext mContext;
  }
}