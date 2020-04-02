using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace chat.Manager
{
    public class WebSocketServerConnectionManager
    {
        private ConcurrentDictionary<string, WebSocket> _sockets;
        public WebSocketServerConnectionManager(){
            this._sockets = new ConcurrentDictionary<string, WebSocket>();
        }

        public string AddSocket(WebSocket socket){
            var id = Guid.NewGuid().ToString();
            _sockets.TryAdd(id, socket);
            return id;
        }

        public ConcurrentDictionary<string, WebSocket> GetAllSockets() => _sockets;
    }
}