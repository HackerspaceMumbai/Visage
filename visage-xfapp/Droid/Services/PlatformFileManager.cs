using System;
using System.IO;
using Visage.Droid.Services;
using Visage.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(PlatformFileManager))]
namespace Visage.Droid.Services
{
    public class PlatformFileManager : IPlatformFileManager
    {
        public string GetHtmlContentAsString(string fileName)
        {
            string html = string.Empty;

            var assetManager = Forms.Context.Assets;
            using (var streamReader = new StreamReader(assetManager.Open(fileName)))
            {
                html = streamReader.ReadToEnd();
            }

            return html;
        }
    }
}
