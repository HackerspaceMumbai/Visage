using System;
using System.IO;
using Visage.iOS.Services;
using Visage.Services;

[assembly: Xamarin.Forms.Dependency(typeof(FileHelper))]
namespace Visage.iOS.Services
{
    public class FileHelper : IFileHelper
    {
        public string GetLocalFilePath(string filename)
        {
			string docFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			string libFolder = Path.Combine(docFolder, "..", "Library", "Databases");

			if (!Directory.Exists(libFolder))
			{
				Directory.CreateDirectory(libFolder);
			}

			return Path.Combine(libFolder, filename);
        }
    }
}
