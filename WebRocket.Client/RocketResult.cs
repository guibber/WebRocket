using System;

namespace WebRocket.Client {
  public class RocketResult : EqualsOverride {
    public bool IsSocketClosed {get;set;}
    public Exception Exception {get;set;}

    public RocketResult SetClosed() {
      IsSocketClosed = true;
      return this;
    }

    public RocketResult SetException(Exception ex) {
      Exception = ex;
      return this;
    }
  }
}