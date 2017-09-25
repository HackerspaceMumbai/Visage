using System;
using System.IO;
using Visage.Droid.Services;
using Visage.Services;

[assembly: Xamarin.Forms.Dependency(typeof(FileHelper))]
namespace Visage.Droid.Services
{
    public class FileHelper : IFileHelper
    {
        public string GetLocalFilePath(string filename)
        {
			string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			return Path.Combine(path, filename);
        }
    }
}
