using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Checkins.API
{
    
    public class Pagination
    {
        public int object_count { get; set; }
        public int page_number { get; set; }
        public int page_size { get; set; }
        public int page_count { get; set; }
        public string continuation { get; set; }
        public bool has_more_items { get; set; }
    }

      
    public class Profile
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string name { get; set; }
        public string website { get; set; }
    }

    public class Barcode
    {
        public string status { get; set; }
        public string barcode { get; set; }
        public DateTime created { get; set; }
        public DateTime changed { get; set; }
        public int checkin_type { get; set; }
        public bool is_printed { get; set; }
        public string checkin_method { get; set; }
    }

    public class Answer
    {
        public string answer { get; set; }
        public string question { get; set; }
        public string type { get; set; }
        public string question_id { get; set; }
    }

    public class Attendee
    {
        public object team { get; set; }
        
        public string resource_uri { get; set; }
        public string id { get; set; }
        public DateTime changed { get; set; }
        public DateTime created { get; set; }
        public int quantity { get; set; }
        public object variant_id { get; set; }
        public Profile profile { get; set; }
        public List<Barcode> barcodes { get; set; }
        public List<Answer> answers { get; set; }
        public bool checked_in { get; set; }
        public bool cancelled { get; set; }
        public bool refunded { get; set; }
        public string affiliate { get; set; }
        public object guestlist_id { get; set; }
        public object invited_by { get; set; }
        public string status { get; set; }
        public string ticket_class_name { get; set; }
        public string delivery_method { get; set; }
        public string event_id { get; set; }
        public string order_id { get; set; }
        public string ticket_class_id { get; set; }
    }

    public class EventBriteEventResponse
    {
        public Pagination pagination { get; set; }
        public List<Attendee> attendees { get; set; }
    }
}
