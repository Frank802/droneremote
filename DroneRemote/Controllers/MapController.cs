using DroneRemote.Helpers;
using DroneRemote.Models;
using DroneRemote.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DroneRemote.Controllers
{
    [Authorize]
    public class MapController : Controller
    {
        public async Task<IActionResult> Index()
        {
            try
            {
                if (Program.CurrentData == null)
                {
                    var availableData = await BlobStorageService.GetBlobList(BlobStorageService.DataContainerName);

                    Program.CurrentData = new TelemetryPackage()
                    {
                        PackageName = availableData.LastOrDefault(),
                        PackageData = await BlobStorageService.GetData(BlobStorageService.DataContainerName, availableData.LastOrDefault())
                    };

                    Program.AvailablePackages.Add(Program.CurrentData);
                }

                var list = new List<Position>();

                foreach (var data in Program.CurrentData.PackageData)
                {
                    if(data.GpsLatitude != 0 && data.GpsLongitude != 0)
                        list.Add(new Position(data.GpsLatitude, data.GpsLongitude, data.GpsAltitude));
                }

                var model = new MapViewModel();

                if (list != null && list.Count > 0)
                {
                    model.Points = list;
                    model.LastKnownPosition = list.LastOrDefault();
                }                    

                ViewBag.Context = "Map";
                return View(model);
            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
                ViewData["trace"] = ex.StackTrace;
                return View("Error");
            }
        }
    }
}
