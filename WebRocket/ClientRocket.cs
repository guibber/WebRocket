﻿using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using WebRocket.Wrappers;

namespace WebRocket {
  public class ClientRocket : EqualsOverride, IClientRocket {
    public ClientRocket(IClientWebSocketBuilder builder) {
      mBuilder = builder;
      mSendLock = new SemaphoreSlim(1, 1);
      mReceiveLock = new SemaphoreSlim(1, 1);
    }

    public bool IsOpen => mSocket.State == WebSocketState.Open;

    public async Task<bool> ConnectAsync(Uri uri, CancellationToken token) {
      try {
        mSocket = mBuilder.Build();
        await mSocket.ConnectAsync(uri, token);
        return true;
      } catch (WebSocketException) { }
      return false;
    }

    public async Task<RocketResult> ReceiveStreamAsync(MemoryStream stream, CancellationToken token) {
      var resultRocket = new RocketResult();
      try {
        await mReceiveLock.WaitAsync(token);
        stream.Seek(0, SeekOrigin.Begin);
        var buffer = new ArraySegment<byte>(new byte[8192]);
        IWebSocketReceiveResult socketResult = null;
        do {
          socketResult = await mSocket.ReceiveAsync(buffer, token);
          if (socketResult.MessageType == WebSocketMessageType.Close) {
            await mSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "response normal", token);
            return resultRocket.SetClosed();
          }
          stream.Write(buffer.Array, buffer.Offset, socketResult.Count);
        } while (!socketResult.EndOfMessage);
        stream.Seek(0, SeekOrigin.Begin);
        return resultRocket;
      } catch (WebSocketException) {
        return resultRocket.SetClosed();
      } finally {
        try {
          mReceiveLock.Release();
        } catch (SemaphoreFullException) { }
      }
    }

    public async Task<RocketResult> SendStreamAsync(MemoryStream stream, CancellationToken token) {
      var result = new RocketResult();
      try {
        await mSendLock.WaitAsync(token);
        await mSocket.SendAsync(new ArraySegment<byte>(stream.GetBuffer(), (int)stream.Seek(0, SeekOrigin.Begin), (int)stream.Length),
                                WebSocketMessageType.Binary,
                                true,
                                token);
        return result;
      } catch (WebSocketException) {
        return result.SetClosed();
      } finally {
        try {
          mSendLock.Release();
        } catch (SemaphoreFullException) { }
      }
    }

    public async Task CloseAsync(CancellationToken token) {
      if (mSocket.State != WebSocketState.Open)
        return;
      try {
        await mSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "initiated normal", token);
      } catch (WebSocketException) { }
    }

    public override bool Equals(object obj) {
      if (!base.Equals(obj))
        return false;

      var o = (ClientRocket)obj;
      return IsOpen.Equals(o.IsOpen);
    }

    public override int GetHashCode() {
      return base.GetHashCode();
    }

    private readonly IClientWebSocketBuilder mBuilder;
    private readonly SemaphoreSlim mReceiveLock;
    private readonly SemaphoreSlim mSendLock;
    private IClientWebSocket mSocket;
  }
}