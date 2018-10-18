using DroneRemote.Helpers;
using DroneRemote.Models;
using DroneRemote.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DroneRemote.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        public async Task<IActionResult> Index()
        {
            try
            {
                var availableData = await BlobStorageService.GetBlobList(BlobStorageService.DataContainerName);
                var model = new SettingsViewModel();
                model.Connections = Program.Connections;
                model.AvailableData = availableData;
                model.AvailableDataPackages = Program.AvailablePackages;
                ViewBag.Context = "Settings";
                return View(model);
            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
                ViewData["trace"] = ex.StackTrace;
                return View("Error");
            }
        }

        public async Task<IActionResult> DeleteConnection(string deviceID)
        {
            var conn = Program.Connections.Where(x => x.DeviceId.Equals(deviceID)).FirstOrDefault();

            try
            {
                if (conn != null)
                {
                    if (conn.Socket.State == System.Net.WebSockets.WebSocketState.Open)
                    {
                        await conn.Socket.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "Closed due to user request.", CancellationToken.None);
                        Program.Connections.Remove(conn);
                    }
                    else
                    {
                        Program.Connections.Remove(conn);
                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Program.Connections.Remove(conn);

                ViewData["message"] = ex.Message;
                ViewData["trace"] = ex.StackTrace;
                return View("Error");
            }
        }

        public async Task<IActionResult> DeleteAllConnections()
        {
            try
            {
                foreach(var conn in Program.Connections)
                {
                    if (conn != null)
                    {
                        if (conn.Socket.State == System.Net.WebSockets.WebSocketState.Open)
                        {
                            await conn.Socket.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "Closed due to user request.", CancellationToken.None);
                            Program.Connections.Remove(conn);
                        }
                        else
                        {
                            Program.Connections.Remove(conn);
                        }
                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
                ViewData["trace"] = ex.StackTrace;
                return View("Error");
            }
        }

        public async Task<IActionResult> LoadDataPackage(string packageName)
        {
            try
            {
                var dataPackage = new TelemetryPackage()
                {
                    PackageName = packageName,
                    PackageData = await BlobStorageService.GetData(BlobStorageService.DataContainerName, packageName)
                };

                Program.AvailablePackages.Add(dataPackage);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
                ViewData["trace"] = ex.StackTrace;
                return View("Error");
            }
        }

        public async Task<IActionResult> RefreshDataPackage(string packageName)
        {
            try
            {
                var dataPackage = Program.AvailablePackages.FirstOrDefault(x => x.PackageName == packageName);
                if (dataPackage != null)
                {
                    var newDataPackage = new TelemetryPackage()
                    {
                        PackageName = packageName,
                        PackageData = await BlobStorageService.GetData(BlobStorageService.DataContainerName, packageName)
                    };

                    Program.AvailablePackages.Remove(dataPackage);
                    Program.AvailablePackages.Add(newDataPackage);
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
                ViewData["trace"] = ex.StackTrace;
                return View("Error");
            }
        }

        public async Task<IActionResult> DeleteDataPackage(string packageName)
        {     
            try
            {
                await BlobStorageService.DeleteBlockBlob(BlobStorageService.DataContainerName, packageName);
                var dataPackage = Program.AvailablePackages.FirstOrDefault(x => x.PackageName == packageName);
                if (dataPackage != null)
                {
                    if (Program.CurrentData == dataPackage)
                        Program.CurrentData = null;

                    Program.AvailablePackages.Remove(dataPackage);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
                ViewData["trace"] = ex.StackTrace;
                return View("Error");
            }
        }

        public async Task<ActionResult> DeleteAllDataPackages()
        {
            try
            {
                var list = await BlobStorageService.GetBlobList(BlobStorageService.DataContainerName);

                foreach (var data in list)
                {
                    await BlobStorageService.DeleteBlockBlob(BlobStorageService.DataContainerName, data);
                }

                Program.CurrentData = null;
                Program.AvailablePackages.Clear();
                return RedirectToAction("Index");
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
