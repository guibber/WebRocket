namespace WebRocket.Client {
  public abstract class EqualsOverride {
    public override bool Equals(object obj) {
      return obj != null && GetType() == obj.GetType();
    }

    public override int GetHashCode() {
      return 0;
    }
  }
}