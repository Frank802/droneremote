using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace DroneRemote.Helpers
{
    public enum SocketType
    {
        Data,
        Video
    }

    public class SocketConnection
    {
        public string DeviceId { get; set; }
        public string ToDeviceId { get; set; }
        public SocketType SocketType { get; set; }
        public WebSocket Socket { get; set; }
    }
}
