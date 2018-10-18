using DroneRemote.Helpers;
using DroneRemote.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace DroneRemote
{
	public class WebSocketMiddleware
	{
		readonly RequestDelegate mNext;

		public WebSocketMiddleware(RequestDelegate next)
		{
			mNext = next;
		}

		public async Task Invoke(HttpContext http)
		{
			if (http.WebSockets.IsWebSocketRequest && http.Request.Query.ContainsKey("from") && http.Request.Query.ContainsKey("to"))
			{
				var from = http.Request.Query["from"].ToString();
				var to = http.Request.Query["to"].ToString();
				var webSocket = await http.WebSockets.AcceptWebSocketAsync();

				if (webSocket.State == WebSocketState.Open)
				{
					var existigsocketconnection = Program.Connections.Where(x => x.DeviceId.Equals(from)).FirstOrDefault();
					if (existigsocketconnection != null)
					{
                        Program.Connections.Remove(existigsocketconnection);
					}

                    Program.Connections.Add(new SocketConnection { DeviceId = from, ToDeviceId = to, SocketType = SocketType.Data, Socket = webSocket });

					while (webSocket.State == WebSocketState.Open)
					{
						var buffer = new ArraySegment<byte>(new byte[100]);
						var received = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

						if (received.MessageType == WebSocketMessageType.Binary)
						{
							List<byte> data = new List<byte>(buffer.Take(received.Count));
							while (received.EndOfMessage == false)
							{
								received = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
								data.AddRange(buffer.Take(received.Count));
							}

							var socketConnection = Program.Connections.Where(x => x.DeviceId.Equals(to)).FirstOrDefault();
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
                                    // Removing corrupted connection
                                    Program.Connections.Remove(socketConnection);
                                    await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, ex.Message, CancellationToken.None);
                                }
							}
							else
							{
                                // Removing closed connection
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