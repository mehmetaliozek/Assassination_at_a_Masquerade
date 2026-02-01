using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class WebSocketService
{
    private ClientWebSocket _webSocket;
    private CancellationTokenSource _cts;

    public event Action<string> OnMessageReceived;
    public event Action OnConnected;
    public event Action<string> OnError;

    public bool IsConnected => _webSocket?.State == WebSocketState.Open;

    public async Task Connect(string url)
    {
        _webSocket = new ClientWebSocket();
        _cts = new CancellationTokenSource();

        try
        {
            await _webSocket.ConnectAsync(new Uri(url), _cts.Token);
            OnConnected?.Invoke();
            _ = ReceiveLoop();
        }
        catch (Exception e)
        {
            OnError?.Invoke(e.Message);
        }
    }

    private async Task ReceiveLoop()
    {
        byte[] buffer = new byte[2048];
        while (IsConnected)
        {
            var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
            if (result.MessageType == WebSocketMessageType.Text)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                OnMessageReceived?.Invoke(message);
            }
        }
    }

    public async Task Disconnect()
    {
        if (_webSocket != null)
        {
            _cts?.Cancel();
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
        }
    }
}