using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using chat.Enum;
using chat.Model;
using Newtonsoft.Json;

namespace chat.Manager
{
    public class WebSocketNotifyManager
    {
        public async Task NotifyAll(ConcurrentDictionary<string, WebSocket> sockets, string id){
             var message = new MessageModel();
             message.Id = id;

            message.Status = EnTypeMessage.UpdateUsers;
            foreach(var socket in sockets){
                message.Members = sockets.Where(s => s.Key != socket.Key)
                                          .Select(c => new MemberModel(c.Key))
                                          .ToList();

                await this.Send(socket.Value, message);
            }

        }

        private async Task Send (WebSocket socket, MessageModel message){
            var buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            await socket.SendAsync(buffer,WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}