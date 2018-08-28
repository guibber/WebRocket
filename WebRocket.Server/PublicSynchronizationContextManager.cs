using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace WebRocket.Server {
  public struct PublicSynchronizationContextManager : INotifyCompletion {
    public bool IsCompleted => SynchronizationContext.Current == null;

    public void OnCompleted(Action continuation) {
      var outer = SynchronizationContext.Current;
      try {
        SynchronizationContext.SetSynchronizationContext(null);
        continuation();
      } finally {
        SynchronizationContext.SetSynchronizationContext(outer);
      }
    }

    public PublicSynchronizationContextManager GetAwaiter() {
      return this;
    }

    public void GetResult() { }
  }
}