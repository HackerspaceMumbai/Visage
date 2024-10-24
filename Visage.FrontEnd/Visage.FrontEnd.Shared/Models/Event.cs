using System;

namespace Visage.FrontEnd.Shared.Models
{
    public class Event
    {
        public string Title { get; set; }
        public string CoverPicture { get; set; }
        public DateTime Date { get; set; }
        public string Venue { get; set; }
        public int? AttendeesPercentage { get; set; }
    }
}
