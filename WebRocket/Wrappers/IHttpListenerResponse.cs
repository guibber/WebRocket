using System.IO;

namespace WebRocket.Wrappers {
  public interface IHttpListenerResponse {
    long ContentLength64 {get;set;}
    int StatusCode {get;set;}
    Stream OutputStream {get;}
  }
}