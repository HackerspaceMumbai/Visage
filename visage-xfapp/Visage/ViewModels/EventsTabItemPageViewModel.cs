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

        public ICommand EventSelectedCommand
        {
            get;
            set;
        }

        public EventsTabItemPageViewModel(string title)
        {
            Title = title;

            PopulatedEvents(title);

            EventSelectedCommand = new Command<Event>(ExecuteEventSelectedCommand);
        }

        //TODO: Improve rendering for upcoming and completed events
        async void PopulatedEvents(string title)
        {
            try
            {
                IsBusy = true;

				if (title.Equals("Upcoming", StringComparison.OrdinalIgnoreCase))
				{
                    Events = new List<Event>
    				{
                        new Event
    					{
    						id = "1",
    						name = "Dockers on Azure",
    						organizer_name = "Augustine Correra",
    						venue = new Venue
    						{
    							address_string = "Microsoft India Pvt. Ltd, Worli",
    							address_maps_uri = "https://maps.google.com"
    						},
    						starts = DateTime.Now.AddDays(1),
                            ends = DateTime.Now.AddDays(1).AddHours(5),
                            sessions = new List<Session>
                            {
                                new Session
                                {
                                    id = "1",
                                    name = "Introduction to containerization",
                                    pin = 12345,
                                    speakers = new List<Speaker>
                                    {
                                        new Speaker
                                        {
                                            id = "1",
                                            name = "Augustine Correra",
                                            introduction = "Augustine is organizer of Mumbai Hacker Space",
                                            profiles = new List<SpeakerProfile>()
                                        }
                                    },
									starts = DateTime.Now.AddDays(1),
							        ends = DateTime.Now.AddDays(1).AddHours(2)
                                },
                                new Session
                                {
                                    id = "2",
                                    name = "Introduction to Dockers on Azure",
                                    pin = 12345,
									speakers = new List<Speaker>
									{
                                        new Speaker
                                        {
											id = "1",
    										name = "Maninder Sigh Bindra",
    										introduction = "Mani is a Sr. Technology Evangelist on Azure from Microsoft",
    										profiles = new List<SpeakerProfile>()
                                        }
									},
                                    starts = DateTime.Now.AddDays(1).AddHours(3),
									ends = DateTime.Now.AddDays(1).AddHours(5)
                                }
                            }
    					},
                        new Event
    					{
    						id = "2",
    						name = "CI/CD with VSTS",
    						organizer_name = "Mani S Bindra",
    						venue = new Venue
    						{
    							address_string = "Microsoft India Pvt. Ltd, Worli",
    							address_maps_uri = "https://maps.google.com"
    						},
    						starts = DateTime.Now.AddDays(2),
                            ends = DateTime.Now.AddDays(2).AddHours(5),
							sessions = new List<Session>
                            {
                                new Session
                                {
                                    id = "1",
                                    name = "CI/CD with VSTS",
                                    pin = 12345,
                                    speakers = new List<Speaker>
                                    {
                                        new Speaker
                                        {
                                            id = "1",
                                            name = "Mani S Bindra",
                                            introduction = "Mani is a Sr. Technology Evangelist on Azure from Microsoft",
                                            profiles = new List<SpeakerProfile>()
                                        }
                                    },
									starts = DateTime.Now.AddDays(2),
							        ends = DateTime.Now.AddDays(2).AddHours(5)
                                },
                            }
    					},
    					new Event
    					{
    						id = "3",
    						name = "Xamarin for .Net Developers",
    						organizer_name = "Prachi Khushwah",
    						venue = new Venue
    						{
    							address_string = "Microsoft India Pvt. Ltd, Worli",
    							address_maps_uri = "https://maps.google.com"
    						},
    						starts = DateTime.Now.AddDays(5),
							ends = DateTime.Now.AddDays(5).AddHours(5),
							sessions = new List<Session>
							{
								new Session
								{
									id = "1",
									name = "Xamarin for .Net Developers",
                                    pin = 12345,
									speakers = new List<Speaker>
									{
										new Speaker
										{
											id = "1",
											name = "Prachi Khushwah",
											introduction = "Prachi is a Technology Evangelist on Xamarin from Microsoft",
											profiles = new List<SpeakerProfile>()
										}
									},
									starts = DateTime.Now.AddDays(5),
							        ends = DateTime.Now.AddDays(5).AddHours(5)
								},
							}
    					},
    					new Event
    					{
    						id = "4",
    						name = "IoT with Azure",
    						organizer_name = "Hardik Mistry",
    						venue = new Venue
    						{
    							address_string = "Microsoft India Pvt. Ltd, Worli",
    							address_maps_uri = "https://maps.google.com"
    						},
    						starts = DateTime.Now,
                            ends = DateTime.Now.AddHours(5),
							sessions = new List<Session>
							{
								new Session
								{
									id = "1",
									name = "IoT with Azure",
                                    pin = 12345,
									speakers = new List<Speaker>
									{
										new Speaker
										{
											id = "1",
											name = "Hardik Mistry",
                                            introduction = "Hardik is a Digital Consultant helping brands and individuals deliver mobile first and cloud enabled experience",
											profiles = new List<SpeakerProfile>()
										}
									},
									starts = DateTime.Now,
							        ends = DateTime.Now.AddHours(5),
								},
							}
    					},
    				};
				}
				else
				{
					Events = new List<Event>
    				{
    					new Event
    					{
    						id = "1",
    						name = "Courage in Chaos with Xamarin",
    						organizer_name = "Hardik Mistry",
    						venue = new Venue
    						{
    							address_string = "AppMattic HQ, Vadodara",
    							address_maps_uri = "https://maps.google.com"
    						},
    						starts = DateTime.Now.AddDays(-1),
                            ends = DateTime.Now.AddDays(-1).AddHours(5),
							sessions = new List<Session>
							{
								new Session
								{
									id = "1",
									name = "Courage in Chaos with Xamarin",
                                    pin = 12345,
									speakers = new List<Speaker>
									{
										new Speaker
										{
											id = "1",
											name = "Hardik Mistry",
											introduction = "Hardik is a Digital Consultant helping brands and individuals deliver mobile first and cloud enabled experience",
											profiles = new List<SpeakerProfile>()
										}
									},
									starts = DateTime.Now.AddDays(-1),
							        ends = DateTime.Now.AddDays(-1).AddHours(5)
								},
							}
    					}
    				};
				}
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);

                await _dialogService.ShowMessage(AppHelper.ApplicationName, ex.Message, Actions.Close);
            }
            finally
            {
                IsBusy = false;
            }
        }
    
        async void ExecuteEventSelectedCommand(Event selectedItem)
        {
            try
            {
                IsBusy = true;

                await _dialogService.ShowMessage(AppHelper.ApplicationName, "You selected: " + selectedItem.name, Actions.Close);

                await Navigation.PushAsync(new EventDetailsPage(selectedItem));
            }
            catch(Exception ex)
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
