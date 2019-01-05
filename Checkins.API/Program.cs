using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Checkins.API
{
    public class Program
    {
        private static readonly HttpClient restClient = new HttpClient();
        public IConfiguration Configuration { get; }

                                     
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

        private static async Task getEventRegistrants()
        {
            var stringTask = restClient.GetStringAsync("https://www.eventbriteapi.com/v3/events/53008188920/attendees/?token=UAAN7EXKMVJYKGI4LJ6E");
            var msg = await stringTask;
            Console.Write(msg);
        }
    }
}
