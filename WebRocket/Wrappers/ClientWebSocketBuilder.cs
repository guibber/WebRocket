namespace WebRocket.Wrappers {
  public class ClientWebSocketBuilder : IClientWebSocketBuilder {
    public IClientWebSocket Build() {
      return new ClientWebSocket(new System.Net.WebSockets.ClientWebSocket());
    }
  }
}