using DroneRemote.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace DroneRemote
{
    public class ImageSenderWebSocketMiddleware
    {
        readonly RequestDelegate mNext;

        public ImageSenderWebSocketMiddleware(RequestDelegate next)
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
                    while (webSocket.State == WebSocketState.Open)
                    {
                        var buffer = new ArraySegment<byte>(new byte[4096]);
                        var received = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

                        if (received.MessageType == WebSocketMessageType.Binary)
                        { 
                            List<byte> data = new List<byte>(buffer.Take(received.Count));
                            while (received.EndOfMessage == false)
                            {
                                received = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                                data.AddRange(buffer.Take(received.Count));
                            }

                            var socketConnection = Program.Connections.Where(x => x.DeviceId.Equals(deviceid)).FirstOrDefault();
                            if (socketConnection == null)
                                continue;

                            var destsocket = socketConnection.Socket;
                            if (destsocket.State == WebSocketState.Open)
                            {
                                var type = WebSocketMessageType.Binary;

                                try
                                {
                                    await destsocket.SendAsync(new ArraySegment<byte>(data.ToArray()), type, true, CancellationToken.None);
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"Error: {ex.Message}");
                                }
                            }
                            else
                            {
                                // Removing closed camera connection
                                Program.Connections.Remove(socketConnection);
                            }
                        }
                        else if (received.MessageType == WebSocketMessageType.Close)
                        {
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
