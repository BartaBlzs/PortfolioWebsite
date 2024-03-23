using Aspose.Zip;
using Aspose.Zip.Rar;
using FluentFTP;
using ftp.Models;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
using PortfolioProject.Models;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Web;

namespace PortfolioProject.Controllers
{
    public class FTPController : Controller
    {
        private string APP_PASS_ba = "miahbageci";
        private string APP_PASS_ma = "marcigeci";
        FtpClient ftp = new("win6055.site4now.net", new System.Net.NetworkCredential { UserName = "ftpba", Password = "4700216001Sh" });

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Demo()
        {
            Response.Cookies.Append("logged", "yes");
            return RedirectToAction(nameof(Files));
        }

        public IActionResult Files(string path)
        {
            if (Request.Cookies["logged"] != "yes") return RedirectToAction(nameof(Index));

            FtpListItem[] listing = ftp.GetListing("/", FtpListOption.Recursive);

            return View(new FileModel(listing.OrderByDescending(x => x.Type == FtpObjectType.Directory)
                                             .ThenBy(x => x.FullName)
                                             .Select(x => (x.FullName, x.Modified, x.Size, x.Type))
                                             .ToJson(),
                        string.IsNullOrEmpty(path) ? "/" : path));
        }

		[HttpGet]
		public IActionResult File(string path)
		{
			var stream = DownloadStream(path);
			if (path.EndsWith(".png") || path.EndsWith(".jpg") || path.EndsWith(".webp") || path.EndsWith(".jfif") || path.EndsWith(".jpeg"))
			{
                return View("file", $"<img src='data:image/png;base64,{Convert.ToBase64String(stream.ToArray(), 0, stream.ToArray().Length)}'>");
			}
			else if (path.EndsWith(".mp3"))
			{
				var url = $"/FTP/Embed?path={HttpUtility.UrlEncode(path)}";
                return View("file", $"<audio controls><source src='{url}' type='audio/mpeg'>Your browser does not support the audio tag.</audio>");
			}
			else if (path.EndsWith(".mp4"))
			{
				var url = $"/FTP/Embed?path={HttpUtility.UrlEncode(path)}";
                return View("file", $"<video controls><source src='{url}' type='video/mp4'>Your browser does not support the video tag.</video>");
			}
			else if (path.EndsWith(".ppt") || path.EndsWith(".pptx") || path.EndsWith(".doc") || path.EndsWith(".docx") || path.EndsWith(".xls") || path.EndsWith(".xlsx"))
			{
				var url = $"/FTP/Embed?path={HttpUtility.UrlEncode(path)}";
                return View("file", $"<iframe src='https://view.officeapps.live.com/op/embed.aspx?src={url}' width='100%' height='100%' frameborder='0'>");
			}
			else
			{
				StreamReader reader = new(stream);
                return View("file", $"<pre>{reader.ReadToEnd()}</pre>");
			}
		}

        public IActionResult DownloadFile(string path)
        {
            return File(DownloadStream(path), "APPLICATION/octet-stream", path);
        }

		public IActionResult DownloadFolder(string path)
		{
            var files = ftp.GetListing(path, FtpListOption.Recursive).Where(x => x.Type == FtpObjectType.File);
			var streams = DownloadStream(files.Select(x => x.FullName).ToArray());

			MemoryStream outputMemStream = new();
			ZipOutputStream zipStream = new(outputMemStream);

			zipStream.SetLevel(3);

			for(int i = 0; i < streams.Length; i++)
			{
                var curfile = files.ElementAt(i);
				var newEntry = new ZipEntry(curfile.FullName[path.Length..])
				{
					DateTime = curfile.Modified
				};

				zipStream.PutNextEntry(newEntry);
                StreamUtils.Copy(streams[i], zipStream, new byte[4096]);
                streams[i].Close();
				zipStream.CloseEntry();
			}

			zipStream.IsStreamOwner = false;
			zipStream.Close();

			outputMemStream.Position = 0;
			return File(outputMemStream.ToArray(), "APPLICATION/octet-stream", Path.GetFileName(path[..^1]) + ".zip");
		}

		public IActionResult OpenZip()
        {
            var stream = DownloadStream(Request.Cookies["OpenedZipPath"]);
            Archive zip;
            if (Request.Cookies["OpenedZipPath"].EndsWith(".rar")) zip = RarToZip(stream);
            else zip = new Archive(stream);
            return View(new ZipFileModel(zip, Request.Cookies["ZipWorkingDir"]));
        }

        public Archive RarToZip(Stream stream)
        {
            RarArchive zipin = new(stream);
            var zip = new Archive();
            foreach (var entry in zipin.Entries)
            {
                if (!entry.IsDirectory)
                {
                    var ms = new MemoryStream();
                    entry.Extract(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    zip.CreateEntry(entry.Name, ms);
                }
            }
            return zip;
        }

        public IActionResult ZipFolder(string path)
        {
            if (path == null) Response.Cookies.Append("ZipWorkingDir", "/");
            if (path == "../") Response.Cookies.Append("ZipWorkingDir", string.Join("/", Request.Cookies["ZipWorkingDir"].Split("/")[0..^2]) + "/");
            else Response.Cookies.Append("ZipWorkingDir", Request.Cookies["ZipWorkingDir"] + path + "/");
            return RedirectToAction(nameof(OpenZip));
        }

        public IActionResult OpenZippedFile(string path)
        {
            var stream = DownloadStream(Request.Cookies["OpenedZipPath"]);
            if (Request.Cookies["OpenedZipPath"].EndsWith(".rar"))
            {
                var zip = new RarArchive(stream);
                if (path.EndsWith(".png") || path.EndsWith(".jpg") || path.EndsWith(".webp") || path.EndsWith(".jfif"))
                {
                    var ms = new MemoryStream();
                    zip.Entries.FirstOrDefault(x => x.Name == Request.Cookies["ZipWorkingDir"][1..] + path).Open().CopyTo(ms);
                    return Content($"<html><link rel='stylesheet' href='/css/display.css' /><body class='flex'><img src='{"data:image/png;base64," + Convert.ToBase64String(ms.ToArray(), 0, ms.ToArray().Length)}'></body></html>", "text/html");
                }
                else
                {
                    StreamReader reader = new(zip.Entries.FirstOrDefault(x => x.Name == Request.Cookies["ZipWorkingDir"][1..] + path).Open());

                    if (path.EndsWith(".svg")) return Content($"<html><link rel='stylesheet' href='/css/display.css' /><body class='flex'>{reader.ReadToEnd()}</body></html>", "text/html");
                    return Content($"<html><body><pre>{reader.ReadToEnd()}</pre></body></html>", "text/html");
                }
            }
            else
            {
                var zip = new Archive(stream);
                if (path.EndsWith(".png") || path.EndsWith(".jpg") || path.EndsWith(".webp") || path.EndsWith(".jfif"))
                {
                    var ms = new MemoryStream();
                    zip.Entries.FirstOrDefault(x => x.Name == Request.Cookies["ZipWorkingDir"][1..] + path).Open().CopyTo(ms);
                    return Content($"<html><link rel='stylesheet' href='/css/display.css' /><body class='flex'><img src='{"data:image/png;base64," + Convert.ToBase64String(ms.ToArray(), 0, ms.ToArray().Length)}'></body></html>", "text/html");
                }
                else
                {
                    StreamReader reader = new(zip.Entries.FirstOrDefault(x => x.Name == Request.Cookies["ZipWorkingDir"][1..] + path).Open());

                    if (path.EndsWith(".svg")) return Content($"<html><link rel='stylesheet' href='/css/display.css' /><body class='flex'>{reader.ReadToEnd()}</body></html>", "text/html");
                    return Content($"<html><body><pre>{reader.ReadToEnd()}</pre></body></html>", "text/html");
                }
            }

        }

        public IActionResult DownloadZippedFile(string path)
        {
            var stream = DownloadStream(Request.Cookies["OpenedZipPath"]);
            if (Request.Cookies["OpenedZipPath"].EndsWith(".rar"))
            {
                var zip = new RarArchive(stream);
                return File(zip.Entries.FirstOrDefault(x => x.Name == Request.Cookies["ZipWorkingDir"][1..] + path).Open(), "APPLICATION/octet-stream", path);
            }
            else
            {
                var zip = new Archive(stream);
                return File(zip.Entries.FirstOrDefault(x => x.Name == Request.Cookies["ZipWorkingDir"][1..] + path).Open(), "APPLICATION/octet-stream", path);
            }
        }

        public IActionResult OpenFileRel(string path)
        {
            return null;
        }

        [HttpGet]
        public IActionResult Embed(string path)
        {
            return File(DownloadStream(path), "APPLICATION/octet-stream", path);
        }

        public IActionResult DeleteFile(string path)
        {
            return null;
        }

        public IActionResult NewDir(string path)
        {
			return null;
		}

        [HttpPost]
        [DisableRequestSizeLimit]
        public JsonResult UploadFile(string filepath, [FromBody] string file)
        {
			return null;
		}

        public IActionResult MoveFile(string filepath, string destpath)
        {
			return null;
		}

        [HttpPost]
        public void MoveFileByDragDrop(string filename, [FromBody] string destpath)
        {
			
		}

        public IActionResult Logout()
        {
			return null;
		}

		private MemoryStream DownloadStream(string path)
		{
			var stream = new MemoryStream();
			ftp.Connect();
			ftp.DownloadStream(stream, path);
			stream.Seek(0, SeekOrigin.Begin);
			ftp.Disconnect();
			return stream;
		}

		private MemoryStream[] DownloadStream(string[] paths)
		{
			var streams = new MemoryStream[paths.Length];
			ftp.Connect();
            for(int i = 0; i < paths.Length; i++)
			{
                streams[i] = new MemoryStream();
				ftp.DownloadStream(streams[i], paths[i]);
                streams[i].Seek(0, SeekOrigin.Begin);
			}
			ftp.Disconnect();
			return streams;
		}
	}
}
