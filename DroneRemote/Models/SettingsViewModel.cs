using DroneRemote.Helpers;
using System.Collections.Generic;

namespace DroneRemote.Models
{
    public class SettingsViewModel
    {
        public List<SocketConnection> Connections { get; set; }
        public List<string> AvailableData { get; set; }
        public List<TelemetryPackage> AvailableDataPackages { get; set; }
    }
}
