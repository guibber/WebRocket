using WebRocket.Server.Wrappers;

namespace WebRocket.Server {
  public class RocketListenerBuilder {
    public static RocketListener Build() {
      return Build(new NoOpObserver());
    }

    public static RocketListener Build(IObserver observer) {
      return new RocketListener(new HttpListener(), new RocketAcceptor(), observer);
    }
  }
}