using FluentFTP;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace PortfolioProject.Controllers
{
    public class DownloadController : Controller
    {
        FtpClient ftp = new("win6055.site4now.net", new System.Net.NetworkCredential("downloadables", "downpass"));
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult DownloadPage(string filePath)
        {
            return View("downloadpage", filePath);
        }

        public IActionResult DownloadFile(string filePath)
        {
            try
            {
                ftp.Connect();
                MemoryStream ms = new();
                ftp.DownloadStream(ms, filePath);
                ms.Position = 0;
                ftp.Disconnect();
                return File(ms, "application/octet-stream", filePath);
            }
            catch
            {
                return Content($"The requested file '{filePath}' was not found");
            }
        }
    }
}
