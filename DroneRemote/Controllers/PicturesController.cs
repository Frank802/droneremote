using DroneRemote.Models;
using DroneRemote.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DroneRemote.Controllers
{
    [Authorize]
    public class PicturesController : Controller
    {
        public async Task<IActionResult> Index()
        {
            try
            {
                var list = await BlobStorageService.GetBlobUriList(BlobStorageService.PicturesContainerName);
                var model = new PicturesViewModel();
                model.Pictures = list;
                ViewBag.Context = "Pictures";
                return View(model);
            }
            catch(Exception ex)
            {
                ViewData["message"] = ex.Message;
                ViewData["trace"] = ex.StackTrace;
                return View("Error");
            }
        }

        public async Task<ActionResult> DeleteImage(string name)
        {
            try
            {
                await BlobStorageService.DeleteBlockBlob(BlobStorageService.PicturesContainerName, name);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
                ViewData["trace"] = ex.StackTrace;
                return View("Error");
            }
        }

        public async Task<ActionResult> DeleteAllImages()
        {
            try
            {
                var list = await BlobStorageService.GetBlobList(BlobStorageService.PicturesContainerName);

                foreach(var image in list)
                {
                    await BlobStorageService.DeleteBlockBlob(BlobStorageService.PicturesContainerName, image);
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
    }
}
