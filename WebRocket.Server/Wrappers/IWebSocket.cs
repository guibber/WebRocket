using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace WebRocket.Server.Wrappers {
  public interface IWebSocket {
    WebSocketState State {get;}
    Task<IWebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken token);
    Task CloseAsync(WebSocketCloseStatus status, string description, CancellationToken token);
    Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken token);
  }
}