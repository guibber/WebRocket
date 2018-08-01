using System.Net.WebSockets;

namespace WebRocket.Server.Wrappers {
  public interface IWebSocketReceiveResult {
    WebSocketMessageType MessageType {get;}
    int Count {get;}
    bool EndOfMessage {get;}
  }
}