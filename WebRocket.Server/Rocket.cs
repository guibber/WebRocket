using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using WebRocket.Server.Wrappers;

namespace WebRocket.Server {
  public class Rocket : EqualsOverride, IRocket {
    public Rocket(IHttpListenerWebSocketContext socketContext) {
      mSocketContext = socketContext;
      mSendLock = new SemaphoreSlim(1, 1);
      mReceiveLock = new SemaphoreSlim(1, 1);
    }

    private IWebSocket Socket => mSocketContext.WebSocket;
    public bool IsOpen => Socket.State == WebSocketState.Open;

    public async Task<RocketResult> ReceiveStreamAsync(MemoryStream stream, CancellationToken token) {
      await new PublicSynchronizationContextManager();
      var result = new RocketResult();
      try {
        await mReceiveLock.WaitAsync(token);
        stream.Seek(0, SeekOrigin.Begin);
        var buffer = new ArraySegment<byte>(new byte[8192]);
        IWebSocketReceiveResult socketResult = null;
        do {
          socketResult = await Socket.ReceiveAsync(buffer, token);
          if (socketResult.MessageType == WebSocketMessageType.Close) {
            await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "response normal", token);
            return result.SetClosed();
          }

          stream.Write(buffer.Array, buffer.Offset, socketResult.Count);
        } while (!socketResult.EndOfMessage);

        stream.Seek(0, SeekOrigin.Begin);
        return result;
      } catch (WebSocketException ex) {
        return result.SetException(ex)
                     .SetClosed();
      } finally {
        try {
          mReceiveLock.Release();
        } catch (SemaphoreFullException) { }
      }
    }

    public async Task<RocketResult> SendStreamAsync(MemoryStream stream, CancellationToken token) {
      await new PublicSynchronizationContextManager();
      var result = new RocketResult();
      try {
        await mSendLock.WaitAsync(token);
        await Socket.SendAsync(new ArraySegment<byte>(stream.GetBuffer(), (int)stream.Seek(0, SeekOrigin.Begin), (int)stream.Length),
                               WebSocketMessageType.Binary,
                               true,
                               token);
        return result;
      } catch (WebSocketException ex) {
        return result.SetException(ex)
                     .SetClosed();
      } finally {
        try {
          mSendLock.Release();
        } catch (SemaphoreFullException) { }
      }
    }

    public async Task CloseAsync(CancellationToken token) {
      await new PublicSynchronizationContextManager();
      if (Socket.State != WebSocketState.Open)
        return;
      try {
        await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "initiated normal", token);
      } catch (WebSocketException) { }
    }

    public override bool Equals(object obj) {
      if (!base.Equals(obj))
        return false;

      var o = (Rocket)obj;
      return IsOpen.Equals(o.IsOpen);
    }

    public override int GetHashCode() {
      return base.GetHashCode();
    }

    private readonly SemaphoreSlim mReceiveLock;
    private readonly SemaphoreSlim mSendLock;
    private readonly IHttpListenerWebSocketContext mSocketContext;
  }
}