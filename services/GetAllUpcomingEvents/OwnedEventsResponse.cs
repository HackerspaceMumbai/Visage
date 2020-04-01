    //
// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using QuickType;
//
//    var getAllEventsResponse = GetAllEventsResponse.FromJson(jsonString);

namespace Visage.Function
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class GetAllEventsResponse
    {
        [JsonProperty("pagination")]
        public Pagination Pagination { get; set; }

        [JsonProperty("events")]
        public Event[] Events { get; set; }
    }

    public partial class Event
    {
        [JsonProperty("name")]
        public Description Name { get; set; }

        [JsonProperty("description")]
        public Description Description { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("vanity_url")]
        public Uri VanityUrl { get; set; }

        [JsonProperty("start")]
        public End Start { get; set; }

        [JsonProperty("end")]
        public End End { get; set; }

        [JsonProperty("organization_id")]
        public string OrganizationId { get; set; }

        [JsonProperty("created")]
        public DateTimeOffset Created { get; set; }

        [JsonProperty("changed")]
        public DateTimeOffset Changed { get; set; }

        [JsonProperty("published")]
        public DateTimeOffset Published { get; set; }

        [JsonProperty("capacity")]
        public long Capacity { get; set; }

        [JsonProperty("capacity_is_custom")]
        public bool CapacityIsCustom { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("listed")]
        public bool Listed { get; set; }

        [JsonProperty("shareable")]
        public bool Shareable { get; set; }

        [JsonProperty("invite_only")]
        public bool InviteOnly { get; set; }

        [JsonProperty("online_event")]
        public bool OnlineEvent { get; set; }

        [JsonProperty("show_remaining")]
        public bool ShowRemaining { get; set; }

        [JsonProperty("tx_time_limit")]
        public long TxTimeLimit { get; set; }

        [JsonProperty("hide_start_date")]
        public bool HideStartDate { get; set; }

        [JsonProperty("hide_end_date")]
        public bool HideEndDate { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; }

        [JsonProperty("is_locked")]
        public bool IsLocked { get; set; }

        [JsonProperty("privacy_setting")]
        public string PrivacySetting { get; set; }

        [JsonProperty("is_series")]
        public bool IsSeries { get; set; }

        [JsonProperty("is_series_parent")]
        public bool IsSeriesParent { get; set; }

        [JsonProperty("inventory_type")]
        public string InventoryType { get; set; }

        [JsonProperty("is_reserved_seating")]
        public bool IsReservedSeating { get; set; }

        [JsonProperty("show_pick_a_seat")]
        public bool ShowPickASeat { get; set; }

        [JsonProperty("show_seatmap_thumbnail")]
        public bool ShowSeatmapThumbnail { get; set; }

        [JsonProperty("show_colors_in_seatmap_thumbnail")]
        public bool ShowColorsInSeatmapThumbnail { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("is_free")]
        public bool IsFree { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("logo_id")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long LogoId { get; set; }

        [JsonProperty("organizer_id")]
        public string OrganizerId { get; set; }

        [JsonProperty("venue_id")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long VenueId { get; set; }

        [JsonProperty("category_id")]
        public object CategoryId { get; set; }

        [JsonProperty("subcategory_id")]
        public object SubcategoryId { get; set; }

        [JsonProperty("format_id")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long FormatId { get; set; }

        [JsonProperty("resource_uri")]
        public Uri ResourceUri { get; set; }

        [JsonProperty("is_externally_ticketed")]
        public bool IsExternallyTicketed { get; set; }

        [JsonProperty("logo")]
        public Logo Logo { get; set; }
    }

    public partial class Description
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("html")]
        public string Html { get; set; }
    }

    public partial class End
    {
        [JsonProperty("timezone")]
        public string Timezone { get; set; }

        [JsonProperty("local")]
        public DateTimeOffset Local { get; set; }

        [JsonProperty("utc")]
        public DateTimeOffset Utc { get; set; }
    }

    public partial class Logo
    {
        [JsonProperty("crop_mask")]
        public object CropMask { get; set; }

        [JsonProperty("original")]
        public Original Original { get; set; }

        [JsonProperty("id")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long Id { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("aspect_ratio")]
        public string AspectRatio { get; set; }

        [JsonProperty("edge_color")]
        public string EdgeColor { get; set; }

        [JsonProperty("edge_color_set")]
        public bool EdgeColorSet { get; set; }
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

    public partial class GetAllEventsResponse
    {
        public static GetAllEventsResponse FromJson(string json) => JsonConvert.DeserializeObject<GetAllEventsResponse>(json, Visage.Function.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this GetAllEventsResponse self) => JsonConvert.SerializeObject(self, Visage.Function.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }
}
