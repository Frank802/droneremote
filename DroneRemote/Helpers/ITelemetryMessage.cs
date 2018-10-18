using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DroneRemote.Helpers
{
    public enum TelemetryMessageType
    {
        Gps = 1,
        Signal = 2,
        Attitude = 3,
        Distance = 4,
        Generic = 5
    }

    public interface ITelemetryMessage
    {
        DateTime Timestamp { get; }

        TelemetryMessageType TelemetryMessageType { get; }

        string GetJSON();
    }
}
