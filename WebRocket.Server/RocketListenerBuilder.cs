using WebRocket.Server.Wrappers;

namespace WebRocket.Server {
  public class RocketListenerBuilder {
    public static RocketListener Build() {
      return new RocketListener(new HttpListener(), new RocketAcceptor());
    }
  }
}