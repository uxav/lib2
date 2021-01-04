using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UX.Lib2.Logging;

namespace UX.Lib2.Cloud.Logger
{
    /// <summary>
    /// A log entry for the cloud logging service
    /// </summary>
    public class LogEntry : ILogEntry
    {
        private string _cloudId;

        internal LogEntry()
        {
            _cloudId = "";
        }

        internal LogEntry(IDictionary<string, JToken> data)
        {
            Time = data["time"].Value<DateTime>();
            Level = (LoggingLevel) Enum.Parse(typeof (LoggingLevel), data["level"].Value<string>(), false);
            Process = data["process"].Value<string>();
            Message = data["message"].Value<string>();
            Info = data["info"].Value<string>();
            Stack = data["stack_trace"].Value<string>();
        }

        #region Implementation of ILogEntry

        /// <summary>
        /// Unique Id of the log entry
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; internal set; }

        /// <summary>
        /// The time and date of the log occurance
        /// </summary>
        [JsonProperty(PropertyName = "time")]
        public DateTime Time { get; internal set; }

        /// <summary>
        /// The level of the log entry type
        /// </summary>
        [JsonProperty(PropertyName = "level")]
        public LoggingLevel Level { get; internal set; }

        /// <summary>
        /// The program name
        /// </summary>
        [JsonProperty(PropertyName = "process")]
        public string Process { get; internal set; }

        /// <summary>
        /// The message of the log
        /// </summary>
        [JsonProperty(PropertyName = "message")]
        public string Message { get; internal set; }

        /// <summary>
        /// The message details / more info
        /// </summary>
        [JsonProperty(PropertyName = "info")]
        public string Info { get; internal set; }

        #endregion

        [JsonProperty(PropertyName = "level_int")]
        public int LevelValue
        {
            get { return (int) Level; }
        }

        /// <summary>
        /// The Cloud ID of the log entry. Empty if has not been submitted to the cloud.
        /// </summary>
        [JsonProperty(PropertyName = "cloud_id")]
        public string CloudId
        {
            get { return _cloudId; } 
            internal set { _cloudId = value; }
        }

        /// <summary>
        /// The calling stack info
        /// </summary>
        [JsonProperty(PropertyName = "stack_trace")]
        public string Stack { get; internal set; }

        public override string ToString()
        {
            return string.Format("{0}: {1:yyyy-MM-dd HH:mm:ss} # {2}", Level, Time.ToLocalTime(), Message);
        }

        public string ToString(bool showAll)
        {
            if (showAll && Info.Length > 0)
            {
                return ToString() + "\r\n" + Info;
            }
            return ToString();
        }
    }
}