﻿using System.Net.WebSockets;

namespace WebRocket.Server.Wrappers {
  public class WebSocketReceiveResult : IWebSocketReceiveResult {
    public WebSocketReceiveResult(System.Net.WebSockets.WebSocketReceiveResult result) {
      mResult = result;
    }

    public WebSocketMessageType MessageType => mResult.MessageType;
    public int Count => mResult.Count;
    public bool EndOfMessage => mResult.EndOfMessage;
    private readonly System.Net.WebSockets.WebSocketReceiveResult mResult;
  }
}