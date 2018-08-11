using WebRocket.Client.Wrappers;

namespace WebRocket.Client {
  public class ClientRocketBuilder {
    public static ClientRocket Build() {
      return new ClientRocket(new ClientWebSocketBuilder());
    }
  }
}