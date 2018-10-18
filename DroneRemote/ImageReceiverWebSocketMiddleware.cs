using DroneRemote.Helpers;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace DroneRemote
{
    public class ImageReceiverWebSocketMiddleware
    {
        readonly RequestDelegate mNext;

        public ImageReceiverWebSocketMiddleware(RequestDelegate next)
        {
            mNext = next;
        }

        public async Task Invoke(HttpContext http)
        {
            if (http.WebSockets.IsWebSocketRequest && http.Request.Query.ContainsKey("device"))
            {
                var deviceid = http.Request.Query["device"].ToString();
                var webSocket = await http.WebSockets.AcceptWebSocketAsync();

                if (webSocket.State == WebSocketState.Open)
                {
                    var existigsocketconnection = Program.Connections.Where(x => x.DeviceId.Equals(deviceid)).FirstOrDefault();
                    if (existigsocketconnection != null)
                    {
                        Program.Connections.Remove(existigsocketconnection);
                    }

                    Program.Connections.Add(new SocketConnection { DeviceId = deviceid, SocketType = SocketType.Video, Socket = webSocket });

                    while (webSocket.State == WebSocketState.Open)
                    {
                        var buffer = new ArraySegment<byte>(new byte[4096]);
                        var received = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

                        switch (received.MessageType)
                        {
                            case WebSocketMessageType.Close:
                                var socket = Program.Connections.Where(x => x.Socket == webSocket).First();
                                Program.Connections.Remove(socket);
                                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed in server by the client", CancellationToken.None);
                                continue;
                        }
                    }
                }
            }
            else
            {
                await mNext.Invoke(http);
            }
        }
    }
}
