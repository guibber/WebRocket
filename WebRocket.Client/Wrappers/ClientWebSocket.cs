using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace WebRocket.Client.Wrappers {
  public class ClientWebSocket : IClientWebSocket {
    public ClientWebSocket(System.Net.WebSockets.ClientWebSocket socket) {
      mSocket = socket;
    }

    public WebSocketState State => mSocket.State;

    public Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken token) {
      return mSocket.SendAsync(buffer, messageType, endOfMessage, token);
    }

    public Task ConnectAsync(Uri uri, CancellationToken token) {
      return mSocket.ConnectAsync(uri, token);
    }

    public async Task<IWebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken token) {
      return new WebSocketReceiveResult(await mSocket.ReceiveAsync(buffer, token));
    }

    public Task CloseAsync(WebSocketCloseStatus status, string description, CancellationToken token) {
      return mSocket.CloseAsync(status, description, token);
    }

    private readonly System.Net.WebSockets.ClientWebSocket mSocket;
  }
}