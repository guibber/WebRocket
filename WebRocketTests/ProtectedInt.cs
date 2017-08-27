using System.Threading;
using System.Threading.Tasks;

namespace WebRocketTests {
  public class ProtectedInt {
    public ProtectedInt() {
      mSlim = new SemaphoreSlim(1, 1);
    }

    public async Task<int> IncrementAsync() {
      await mSlim.WaitAsync();
      mCount++;
      var ret = mCount;
      mSlim.Release();
      return ret;
    }

    public async Task<int> DecrementAsync() {
      await mSlim.WaitAsync();
      mCount--;
      var ret = mCount;
      mSlim.Release();
      return ret;
    }

    private readonly SemaphoreSlim mSlim;
    private int mCount;
  }
}