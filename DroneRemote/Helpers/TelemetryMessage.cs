using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DroneRemote.Helpers
{
    public class TelemetryMessage
    {
        public int GpsFix { get; set; }
        public int GpsNumSat { get; set; }
        public double GpsLatitude { get; set; }
        public double GpsLongitude { get; set; }
        public double GpsAltitude { get; set; }
        public double GpsSpeed { get; set; }
        public double GpsGroundCourse { get; set; }
        public float Angx { get; set; }
        public float Angy { get; set; }
        public float Head { get; set; }
        public float Headfree { get; set; }
        public int Signal { get; set; }
        public double Distance { get; set; }
        public double BatteryVoltage { get; set; }
        public int BatteryPercentage { get; set; }
        public DateTime Timestamp { get; set; }
        public int TelemetryMessageType { get; set; }
        public string EventProcessedUtcTime { get; set; }
        public int PartitionId { get; set; }
        public string EventEnqueuedUtcTime { get; set; }
        public IoTHub IoTHub { get; set; }
    }

    public class IoTHub
    {
        public object MessageId { get; set; }
        public object CorrelationId { get; set; }
        public string ConnectionDeviceId { get; set; }
        public string ConnectionDeviceGenerationId { get; set; }
        public string EnqueuedTime { get; set; }
        public object StreamId { get; set; }
    }
}
