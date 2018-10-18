using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DroneRemote.Helpers
{
    public class TelemetryPackage
    {
        public string PackageName { get; set; }

        public List<TelemetryMessage> PackageData { get; set; }

        public int DataCount { get { return PackageData.Count; } }
    }
}
