using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using chat.Manager;
using chat.Model;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace chat.Middleware
{
    public class WebSocketServerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly WebSocketNotifyManager _notify;
        private readonly WebSocketServerConnectionManager _manager;

        public WebSocketServerMiddleware(RequestDelegate _next,
                                        WebSocketServerConnectionManager _manager)
        {
            this._next = _next;
            this._manager = _manager;
            this._notify = new WebSocketNotifyManager();
        }

        public async Task InvokeAsync(HttpContext context)
        {

            if (context.WebSockets.IsWebSocketRequest)
            {
                WebSocket socket = await context.WebSockets.AcceptWebSocketAsync();
                var connId = _manager.AddSocket(socket);               
                await _notify.NotifyAll(_manager.GetAllSockets(), connId);                

                await ReceiveMessage(socket, async (result, buffer) =>
                {
                   
                    byte[] bufferHalf = new byte[result.Count];
                    Array.Copy(buffer, bufferHalf, result.Count);
                    var socketReceiver = JsonConvert.DeserializeObject<MessageModel>(Encoding.UTF8.GetString(bufferHalf));
                    
                    if (result.MessageType == WebSocketMessageType.Text)
                    {

                         var listSockets = this._manager.GetAllSockets()
                        .Where(s => s.Key == socketReceiver.Receiver)
                        .ToList();

                        foreach (var item in listSockets)
                        {
                            await item.Value.SendAsync(
                               bufferHalf,
                                 WebSocketMessageType.Text,
                                 true, CancellationToken.None
                            );

                        }

                        return;
                    }

                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        string id = _manager.GetAllSockets().FirstOrDefault(s => s.Value == socket).Key;
                        _manager.GetAllSockets().TryRemove(id,out socket);
                        return;
                    }

                });
            }
            else
            {
                await _next(context);
            }
        }

        private async Task ReceiveMessage(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[1024 * 4];
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer),
                                                                cancellationToken: CancellationToken.None
                );                

                handleMessage(result, buffer);
            }
        }


        private async Task SendMessage(WebSocket socket, string connID)
        {
            var buffer = Encoding.UTF8.GetBytes($"ConnID: {connID}");
            await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task SendMessage(ConcurrentDictionary<string, WebSocket> scokets, string connID)
        {
        //     //this._manager.GetAllSockets();
        //     var buffer = Encoding.UTF8.GetBytes($"ConnID: {connID}");
        //     await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

    }
}