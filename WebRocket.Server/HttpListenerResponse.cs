using System.IO;
using WebRocket.Server.Wrappers;

namespace WebRocket.Server {
  public class HttpListenerResponse : IHttpListenerResponse {
    public HttpListenerResponse(System.Net.HttpListenerResponse response) {
      mResponse = response;
    }

    public long ContentLength64 {
      get => mResponse.ContentLength64;
      set => mResponse.ContentLength64 = value;
    }

    public int StatusCode {
      get => mResponse.StatusCode;
      set => mResponse.StatusCode = value;
    }

    public Stream OutputStream => mResponse.OutputStream;
    private readonly System.Net.HttpListenerResponse mResponse;
  }
}