using System.Net.WebSockets;

namespace WebRocket.Wrappers {
  public interface IWebSocketReceiveResult {
    WebSocketMessageType MessageType {get;}
    int Count {get;}
    bool EndOfMessage {get;}
  }
}