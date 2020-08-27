using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;

namespace Visage.Core.Models
{

    public partial class Event
    {
        JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;

        [JsonProperty("name")]
        public Description Name { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("vanity_url")]
        public Uri VanityUrl { get; set; }

        [JsonProperty("start")]
        public CompleteDateTime Start { get; set; }

        [JsonProperty("end")]
        public CompleteDateTime End { get; set; }

        [JsonProperty("organization_id")]
        public string OrganizationId { get; set; }

        [JsonProperty("created")]
        public DateTimeOffset Created { get; set; }


        [JsonProperty("organizer_id")]
        public string OrganizerId { get; set; }

        [JsonProperty("venue_id")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long VenueId { get; set; }


        [JsonProperty("resource_uri")]
        public Uri ResourceUri { get; set; }

    }

    public partial class Description
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("html")]
        public string Html { get; set; }
    }

    public partial class CompleteDateTime
    {
        [JsonProperty("timezone")]
        public string Timezone { get; set; }

        [JsonProperty("local")]
        public DateTimeOffset Local { get; set; }

        [JsonProperty("utc")]
        public DateTimeOffset Utc { get; set; }
    }



    public partial class Original
    {
        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("width")]
        public object Width { get; set; }

        [JsonProperty("height")]
        public object Height { get; set; }
    }

    public partial class Pagination
    {
        [JsonProperty("object_count")]
        public long ObjectCount { get; set; }

        [JsonProperty("page_number")]
        public long PageNumber { get; set; }

        [JsonProperty("page_size")]
        public long PageSize { get; set; }

        [JsonProperty("page_count")]
        public long PageCount { get; set; }

        [JsonProperty("continuation")]
        public string Continuation { get; set; }

        [JsonProperty("has_more_items")]
        public bool HasMoreItems { get; set; }
    }
}