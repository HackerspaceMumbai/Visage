using System;
using System.Collections.Generic;

namespace Visage.Models
{
    public class EventItem
    {
        public string id { get; set; }

		public string name { get; set; }

		public string organizer_name { get; set; }

        public string starts { get; set; }

        public Venue venue { get; set; }

        public string rsvp { get; set; }
    }
    
    public class Event
    {
		public string id { get; set; }
		
        public string url { get; set; }

		public int pin { get; set; }
		
        public string name { get; set; }
		
        public string organizer_name { get; set; }

		public string starts { get; set; }
		
        public string ends { get; set; }
		
        public string contact { get; set; }
		
        public string email { get; set; }
		
        public Venue venue { get; set; }
		
        public List<string> terms { get; set; }
		
        public List<Session> sessions { get; set; }
    }

	public class Venue
	{
		public string address_string { get; set; }
		
        public string address_maps_uri { get; set; }
	}

    public class SpeakerProfile
	{
		public string source { get; set; }
		
        public string link { get; set; }
	}

	public class Speaker
	{
		public string id { get; set; }
		
        public string name { get; set; }
		
        public string photo { get; set; }
		
        public string introduction { get; set; }
		
        public List<SpeakerProfile> profiles { get; set; }
	}

	public class Location
	{
		public double latitude { get; set; }
		
        public double longitude { get; set; }
		
        public double altitude { get; set; }
	}

	public class Room
	{
		public string name { get; set; }
		
        public string floor { get; set; }
		
        public string section { get; set; }
		
        public Location location { get; set; }
	}

	public class Session
	{
		public string id { get; set; }
		
        public string name { get; set; }
		
        public int pin { get; set; }
		
        public List<Speaker> speakers { get; set; }
		
        public Room room { get; set; }
		
        public string url { get; set; }
		
        public string starts { get; set; }
		
        public string ends { get; set; }
		
        public List<string> terms { get; set; }
	}
}
