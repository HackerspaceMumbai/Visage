using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using Visage.Helpers;
using Visage.Models;
using Visage.Pages;
using Xamarin.Forms;

namespace Visage.ViewModels
{
    public class EventsTabItemPageViewModel : BaseViewModel
    {
        public List<Event> Events
        {
            get;
            set;
        }

        public ICommand SelectedEventCommand
        {
            get;
            set;
        }

        public EventsTabItemPageViewModel(string title)
        {
            Title = title;

            IsBusy = true;

            PopulatedEvents(title);

            SelectedEventCommand = new Command<Event>(ExecuteSelectedEventCommand);

            IsBusy = false;
        }

        //TODO: Improve rendering for upcoming and completed events
        void PopulatedEvents(string title)
        {
            if (title.Equals("Upcoming", StringComparison.OrdinalIgnoreCase))
            {
                Events = new List<Event>
                {
                    new Event
                    {
                        id = "1",
                        name = "#mumtechmeetup - Mobility",
                        organizer_name = "Augustine Correra",
                        venue = new Venue
                        {
                            address_string = "Microsoft India Pvt. Ltd, Worli",
                            address_maps_uri = "https://goo.gl/maps/LnnThjAVfC52"
                        },
                        starts = DateTime.ParseExact("27-08-2017 11:00:00", "dd-MM-yyyy hh:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
                        ends = DateTime.ParseExact("27-08-2017 06:00:00", "dd-MM-yyyy hh:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
                        sessions = new List<Session>
                        {
                            new Session
                            {
                                id = "1",
                                name = "Real world applications of MQTT",
                                speakers = new List<Speaker>
                                {
                                    new Speaker
                                    {
                                        id = "1",
                                        name = "Manoj Gudi",
                                        introduction = "Co-founder and CTO of GetFocus, plays different roles, from writing webservers, developing DSL, to testing SDKs and working on machine learning. Loves fiddling with embedded devices and learning functional languages."
                                    }
                                },
								starts = DateTime.ParseExact("27-08-2017 11:00:00", "dd-MM-yyyy hh:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
						        ends = DateTime.ParseExact("27-08-2017 12:00:00", "dd-MM-yyyy hh:mm:ss", System.Globalization.CultureInfo.InvariantCulture)
                            },
							new Session
							{
								id = "2",
								name = "Building Delightful Conversations",
								speakers = new List<Speaker>
								{
									new Speaker
									{
										id = "1",
										name = "Yogesh Singh",
										introduction = "Building mobile experiences for 3+ years, as iOS Architect at Haptik I enjoy creating stuff like Tetris blocks which fit together."
									}
								},
								starts = DateTime.ParseExact("27-08-2017 12:00:00", "dd-MM-yyyy hh:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
						        ends = DateTime.ParseExact("27-08-2017 01:00:00", "dd-MM-yyyy hh:mm:ss", System.Globalization.CultureInfo.InvariantCulture)
							},
							new Session
							{
								id = "3",
								name = "Life of a Cross-Platform Mobile App",
								speakers = new List<Speaker>
								{
									new Speaker
									{
										id = "1",
										name = "Mayur Tendulkar",
										introduction = "Mayur Tendulkar is Program Manager on Xamarin team at Microsoft, working from Pune, India. Before joining Microsoft, since 2013, he was awarded as Microsoft Most Valuable Professional on Windows Development and worked as Developer Evangelist with Xamarin. He is writing mobile applications since the days of Windows Mobile 5.0 and love to talk about everything mobile. You can find him talking at conferences, user groups and on various social channels. His co-ordinates are @mayur_Tendulkar and for git: mayur-tendulkar. You can follow his thoughts on his blog: http://mayurtendulkar.com"
									}
								},
								starts = DateTime.ParseExact("27-08-2017 01:00:00", "dd-MM-yyyy hh:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
						        ends = DateTime.ParseExact("27-08-2017 02:00:00", "dd-MM-yyyy hh:mm:ss", System.Globalization.CultureInfo.InvariantCulture)
							},
							new Session
							{
								id = "4",
								name = "Getting started with Kotlin",
								speakers = new List<Speaker>
								{
									new Speaker
									{
										id = "1",
										name = "Akshay Chordiya",
										introduction = "Akshay Chordiya is an Entrepreneur and Android Developer from Pune. He has been working on Android since 3+ years. He is an active community speaker who has been promoting Kotlin and having fun with series of articles, meetups and workshops; even before it was official."
									}
								},
								starts = DateTime.ParseExact("27-08-2017 02:30:00", "dd-MM-yyyy hh:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
						        ends = DateTime.ParseExact("27-08-2017 03:30:00", "dd-MM-yyyy hh:mm:ss", System.Globalization.CultureInfo.InvariantCulture)
							},
							new Session
							{
								id = "5",
								name = "Optimising network communications, and operating push notifications at high scale",
								speakers = new List<Speaker>
								{
									new Speaker
									{
										id = "1",
										name = "Jude Pereira",
										introduction = "Senior Full Stack Engineer at CleverTap, with an experience in building highly scalable systems, and a passion to contribute to open source software."
									}
								},
								starts = DateTime.ParseExact("27-08-2017 03:30:00", "dd-MM-yyyy hh:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
						        ends = DateTime.ParseExact("27-08-2017 04:30:00", "dd-MM-yyyy hh:mm:ss", System.Globalization.CultureInfo.InvariantCulture)
							},
							new Session
							{
								id = "6",
								name = "Product Hunt Meetup #4",
								speakers = new List<Speaker>
								{
									new Speaker
									{
										id = "1",
										name = "Mayur Rokade",
										introduction = "Mayur Rokade is an Android Developer at a startup known as Service Lee Technologies Pvt. Ltd.\nHe has worked on apps such as Servify and OnePlus Care which have above 1,00,000 downloads on Play Store. Previously he's been with Directi and LinkedIn, where he worked on DevOps."
									}
								},
								starts = DateTime.ParseExact("27-08-2017 04:30:00", "dd-MM-yyyy hh:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
						        ends = DateTime.ParseExact("27-08-2017 05:00:00", "dd-MM-yyyy hh:mm:ss", System.Globalization.CultureInfo.InvariantCulture)
							},
							new Session
							{
								id = "7",
								name = "Freecodecamp : Coding session",
								speakers = new List<Speaker>
								{
									new Speaker
									{
										id = "1",
										name = "Augustine Corera",
										introduction = "www.freecodecamp.com"
									},
									new Speaker
									{
										id = "2",
										name = "Hardik Mistry",
										introduction = "www.freecodecamp.com"
									}
								},
								starts = DateTime.ParseExact("27-08-2017 05:00:00", "dd-MM-yyyy hh:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
						        ends = DateTime.ParseExact("27-08-2017 06:00:00", "dd-MM-yyyy hh:mm:ss", System.Globalization.CultureInfo.InvariantCulture)
							},
                        }
                    }
                };
            }
            else
            {
                Events = new List<Event>();
            }
        }

        async void ExecuteSelectedEventCommand(Event selectedEvent)
        {
			try
			{
				IsBusy = true;

				await _dialogService.ShowMessage(AppHelper.ApplicationName, "You selected: " + selectedEvent.name, Actions.Close);

                await Navigation.PushAsync(new EventDetailsPage(selectedEvent));
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);

				await _dialogService.ShowMessage(AppHelper.ApplicationName, ex.Message, Actions.Close);
			}
			finally
			{
				IsBusy = false;
			}
        }
    }
}
