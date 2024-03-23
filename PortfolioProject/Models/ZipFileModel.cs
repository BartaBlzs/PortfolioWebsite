using Aspose.Zip;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ftp.Models
{
	public class ZipFileModel
	{
		public HashSet<string> folderNames = new();
		public HashSet<string> fileNames = new();
		public ZipFileModel(Archive arch, string workingFolder)
		{
			foreach (var item in arch.Entries)
			{
				var wd = workingFolder == "/" ? "" : workingFolder[1..];

				if (item.Name.StartsWith(wd))
				{

					if (workingFolder == "/")
					{
						if (item.Name.Split('/').FirstOrDefault().Contains('.')) fileNames.Add(item.Name.Split('/').FirstOrDefault());
						else folderNames.Add(item.Name.Split('/').FirstOrDefault());
					}
					else
					{
						var regex = new Regex(Regex.Escape(wd));
						var tx = regex.Replace(item.Name, "", 1);
						var name = tx.Split('/').FirstOrDefault();

						// some zipper programs make "folder files" and some of them replicate folders from file names
						// like: "/something/asdf.png" 'something' is a folder but really it doesn't exist

						if (name != "")
						{
                            if (!tx.Contains("/")) fileNames.Add(name);
							else folderNames.Add(name);
						}

					}
				}
			}
		}
	}
}
