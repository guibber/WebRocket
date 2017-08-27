using System.Text;
using System.Threading.Tasks;
using WebRocket.Wrappers;

namespace WebRocket {
  public class RocketAcceptor : IRocketAcceptor {
    private static void SetErrorResponse(IHttpListenerContext context) {
      try {
        context.Response.ContentLength64 = _ErrorBytes.Length;
        context.Response.StatusCode = 500;
        context.Response.OutputStream.Write(_ErrorBytes, 0, _ErrorBytes.Length);
        context.Response.OutputStream.Close();
      } catch { }
    }

    public async Task<IRocket> AcceptAsync(IHttpListenerContext context) {
      try {
        return new Rocket(await context.AcceptWebSocketAsync());
      } catch {
        SetErrorResponse(context);
        throw;
      }
    }

    private static readonly byte[] _ErrorBytes = Encoding.UTF8.GetBytes("Internal Processing Error");
  }
}