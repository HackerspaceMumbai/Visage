using System;
using System.Collections.Generic;
using Visage.Models;

namespace Visage.ViewModels
{
    public class EventsTabItemPageViewModel : BaseViewModel
    {
        public List<EventItem> Events
        {
            get;
            set;
        }

        public EventsTabItemPageViewModel(string title)
        {
            Title = title;

            IsBusy = true;

            PopulatedEvents(title);

            IsBusy = false;
        }

        //TODO: Improve rendering for upcoming and completed events
        void PopulatedEvents(string title)
        {
            if(title.Equals("Upcoming", StringComparison.OrdinalIgnoreCase))
            {
				Events = new List<EventItem>
    			{
    				new EventItem
    				{
    					id = "1",
    					name = "Dockers on Azure",
    					organizer_name = "Augustine Correra",
    					venue = new Venue
    					{
    						address_string = "Microsoft India Pvt. Ltd, Worli",
    						address_maps_uri = "https://maps.google.com"
    					},
                        starts = DateTime.Now.AddDays(1).ToString("d")
    				},
					new EventItem
					{
						id = "2",
						name = "CI/CD with VSTS",
						organizer_name = "Mani S Bindra",
						venue = new Venue
						{
							address_string = "Microsoft India Pvt. Ltd, Worli",
							address_maps_uri = "https://maps.google.com"
						},
                        starts = DateTime.Now.AddDays(2).ToString("d")
					},
					new EventItem
					{
						id = "3",
						name = "Xamarin for .Net Develoeprs",
						organizer_name = "Prachi Khushwah",
						venue = new Venue
						{
							address_string = "Microsoft India Pvt. Ltd, Worli",
							address_maps_uri = "https://maps.google.com"
						},
						starts = DateTime.Now.AddDays(5).ToString("d")
					},
					new EventItem
					{
						id = "4",
						name = "IoT with Azure",
						organizer_name = "Hardik Mistry",
						venue = new Venue
						{
							address_string = "Microsoft India Pvt. Ltd, Worli",
							address_maps_uri = "https://maps.google.com"
						},
						starts = DateTime.Now.AddDays(5).ToString("d")
					},
    			};  
            }
            else
            {
				Events = new List<EventItem>
				{
					new EventItem
					{
						id = "1",
						name = "Courage in Chaos with Xamarin",
						organizer_name = "Hardik Mistry",
						venue = new Venue
						{
							address_string = "AppMattic HQ, Vadodara",
							address_maps_uri = "https://maps.google.com"
						},
                        starts = DateTime.Now.AddDays(-1).ToString("d"),
                        rsvp = "Checked In"
					}
				};
			}
        }
    }
}
