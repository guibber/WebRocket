using System.Threading.Tasks;

namespace WebRocket.Wrappers {
  public class HttpListener : IHttpListerner {
    public HttpListener() {
      mListener = new System.Net.HttpListener();
    }

    public bool IsListening => mListener.IsListening;

    public void AddPrefix(string address) {
      mListener.Prefixes.Add(address);
    }

    public void Start() {
      mListener.Start();
    }

    public void Stop() {
      mListener.Stop();
    }

    public async Task<IHttpListenerContext> GetContextAsync() {
      return new HttpListenerContext(await mListener.GetContextAsync());
    }

    private readonly System.Net.HttpListener mListener;
  }
}