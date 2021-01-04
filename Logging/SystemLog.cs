using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UX.Lib2.Logging
{
    public class SystemLog : ILogEntry
    {
        internal SystemLog()
        {
            Info = string.Empty;
        }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; internal set; }
        [JsonConverter(typeof(IsoDateTimeConverter)), JsonProperty(PropertyName = "time")]
        public DateTime Time { get; internal set; }
        [JsonProperty(PropertyName = "level")]
        public LoggingLevel Level { get; internal set; }
        [JsonProperty(PropertyName = "process")]
        public string Process { get; internal set; }
        [JsonProperty(PropertyName = "message")]
        public string Message { get; internal set; }
        [JsonProperty(PropertyName = "info")]
        public string Info { get; internal set; }
        [JsonProperty(PropertyName = "appIndex")]
        public uint AppIndex { get; set; }
    }
}