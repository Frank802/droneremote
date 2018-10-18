using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using DroneRemote.Helpers;

namespace DroneRemote
{
    public class Program
    {
        public static TelemetryPackage CurrentData;
        public static List<TelemetryPackage> AvailablePackages;
        public static List<SocketConnection> Connections;

        public static void Main(string[] args)
        {
            AvailablePackages = new List<TelemetryPackage>();
            Connections = new List<SocketConnection>();

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
