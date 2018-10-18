using DroneRemote.Helpers;
using DroneRemote.Models;
using DroneRemote.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace DroneRemote.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        public async Task<IActionResult> Index()
        {
            try
            {
                var availableData = await BlobStorageService.GetBlobList(BlobStorageService.DataContainerName);
                if (Program.CurrentData == null)
                {
                    Program.CurrentData = new TelemetryPackage()
                    {
                        PackageName = availableData.LastOrDefault(),
                        PackageData = await BlobStorageService.GetData(BlobStorageService.DataContainerName, availableData.LastOrDefault())
                    };

                    Program.AvailablePackages.Add(Program.CurrentData);
                }
                    
                var model = new DashboardViewModel();
                model.AvailableData = availableData;
                model.CurrentData = Program.CurrentData.PackageData;
                ViewBag.Context = "Home";
                return View(model);
            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
                ViewData["trace"] = ex.StackTrace;
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<string> LoadData(string blobName)
        {
            try
            {
                var dataPackage = Program.AvailablePackages.FirstOrDefault(x => x.PackageName == blobName);
                if (dataPackage == null)
                {
                    Program.CurrentData = new TelemetryPackage()
                    {
                        PackageName = blobName,
                        PackageData = await BlobStorageService.GetData(BlobStorageService.DataContainerName, blobName)
                    };

                    Program.AvailablePackages.Add(Program.CurrentData);
                }
                else
                    Program.CurrentData = dataPackage;

                
                return JsonConvert.SerializeObject(Program.CurrentData.PackageData);
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
