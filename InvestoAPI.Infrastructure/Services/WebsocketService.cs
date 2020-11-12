using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InvestoAPI.Infrastructure.Services
{
    public class WebsocketService
    {
        public ClientWebSocket WebSocket { get; set; }
        public bool autoConnect { get; set; }

        private readonly ILogger<WebsocketService> _logger;

        private string lastData = "";
        private string url = "wss://ws.finnhub.io?token=bpv6tnnrh5rabkt31r10";

        public WebsocketService(ILogger<WebsocketService> logger)
        {
            _logger = logger;
            WebSocket = GetClient();
            autoConnect = true;
        }

        public bool IsOpen()
        {
            if (WebSocket.State == WebSocketState.Open) return true;
            else return false;
        }

        async public Task Connect(CancellationToken stoppingToken)
        {
            WebSocket = GetClient();
            await WebSocket.ConnectAsync(new Uri(url), stoppingToken);
        }

        async public Task Reconnect(CancellationToken stoppingToken)
        {
            WebSocket = GetClient();
            await WebSocket.ConnectAsync(new Uri(url), stoppingToken);
            await WebSocket.SendAsync(Encoding.UTF8.GetBytes(lastData), WebSocketMessageType.Text, true, stoppingToken);
        }

        private ClientWebSocket GetClient()
        {
            var ws = new ClientWebSocket();
            return ws;
        }

        async public Task Send(string data, CancellationToken stoppingToken)
        {
            lastData = data;
            if (IsOpen()) await WebSocket.SendAsync(Encoding.UTF8.GetBytes(data), WebSocketMessageType.Text, true, stoppingToken);
        }

        async public Task Disconnect(string description, CancellationToken stoppingToken)
        {
            if (WebSocket != null)
            {
                await WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, description, stoppingToken);
                WebSocket.Dispose();
            }
        }

        async public Task<string> Receive(ArraySegment<byte> buffer, CancellationToken stoppingToken)
        {
            WebSocketReceiveResult result;
            using (var ms = new MemoryStream())
            {
                do
                {
                    result = await WebSocket.ReceiveAsync(buffer, stoppingToken);
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                } while (!result.EndOfMessage);

                if (result.MessageType == WebSocketMessageType.Close)
                    return null;

                ms.Seek(0, SeekOrigin.Begin);
                using var reader = new StreamReader(ms, Encoding.UTF8);
                return await reader.ReadToEndAsync();
            };
        }
    }
}
