using DroneRemote.Helpers;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DroneRemote.Models
{
    public class DashboardViewModel
    {
        public List<TelemetryMessage> CurrentData { get; set; }
        public List<string> AvailableData { get; set; }
    }
}
