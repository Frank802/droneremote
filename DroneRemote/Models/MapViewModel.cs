using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DroneRemote.Models
{
    public class MapViewModel
    {
        public Position LastKnownPosition { get; set; }

        public List<Position> Points { get; set; }
    }

    public class Position
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }

        public Position(double latitude, double longitude, double altitude)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Altitude = altitude;
        }
    }
}
