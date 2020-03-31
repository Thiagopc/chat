using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using chat.Manager;
using Microsoft.AspNetCore.Http;

namespace chat.Middleware
{
    public class WebSocketServerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly WebSocketServerConnectionManager _manager;

        public WebSocketServerMiddleware(RequestDelegate _next,
                                        WebSocketServerConnectionManager _manager)
        {
            this._next = _next;
            this._manager = _manager;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            if (context.WebSockets.IsWebSocketRequest)
            {
                WebSocket socket = await context.WebSockets.AcceptWebSocketAsync();
                string connId = _manager.AddSocket(socket);
                await SendConnIDAsync(socket, connId);


                await ReceiveMessage(socket, async (result, buffer) =>
                {
                    //var text =  (Encoding.UTF8.GetString(buffer)).Replace("\0",string.Empty);
                    byte[] bufferHalf = new byte[result.Count];
                    Array.Copy(buffer, bufferHalf, result.Count);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {

                        var listSockets = this._manager.GetAllSockets();
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
                        //Need create method remove closed socket
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


        private async Task SendConnIDAsync(WebSocket socket, string connID)
        {
            var buffer = Encoding.UTF8.GetBytes($"ConnID: {connID}");
            await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

    }
}