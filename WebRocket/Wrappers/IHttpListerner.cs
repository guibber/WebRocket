using System.Threading.Tasks;

namespace WebRocket.Wrappers {
  public interface IHttpListerner {
    bool IsListening {get;}
    void AddPrefix(string address);
    void Start();
    void Stop();
    Task<IHttpListenerContext> GetContextAsync();
  }
}