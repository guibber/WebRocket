using System.Text;
using System.Threading.Tasks;
using WebRocket.Server.Wrappers;

namespace WebRocket.Server {
  public class RocketAcceptor : IRocketAcceptor {
    public async Task<IRocket> AcceptAsync(IHttpListenerContext context) {
      try {
        return new Rocket(await context.AcceptWebSocketAsync());
      } catch {
        SetErrorResponse(context);
        throw;
      }
    }

    private static void SetErrorResponse(IHttpListenerContext context) {
      try {
        context.Response.ContentLength64 = _ErrorBytes.Length;
        context.Response.StatusCode = 500;
        context.Response.OutputStream.Write(_ErrorBytes, 0, _ErrorBytes.Length);
        context.Response.OutputStream.Close();
      } catch { }
    }

    private static readonly byte[] _ErrorBytes = Encoding.UTF8.GetBytes("Internal Processing Error");
  }
}