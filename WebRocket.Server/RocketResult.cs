namespace WebRocket.Server {
  public class RocketResult : EqualsOverride {
    public bool IsSocketClosed {get;set;}

    public RocketResult SetClosed() {
      IsSocketClosed = true;
      return this;
    }
  }
}