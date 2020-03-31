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
            string ConnID = Guid.NewGuid().ToString();
            _sockets.TryAdd(ConnID, socket);
            return ConnID;
        }

        public ConcurrentDictionary<string, WebSocket> GetAllSockets() => _sockets;
    }
}