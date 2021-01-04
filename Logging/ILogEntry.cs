using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UX.Lib2.Logging
{
    /// <summary>
    /// The basis for a log entry in ILogger
    /// </summary>
    public interface ILogEntry
    {
        /// <summary>
        /// Unique Id of the log entry
        /// </summary>
        string Id { get; }
        /// <summary>
        /// The time and date of the log occurance
        /// </summary>
        DateTime Time { get; }
        /// <summary>
        /// The level of the log entry type
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        LoggingLevel Level { get; }
        /// <summary>
        /// The program name
        /// </summary>
        string Process { get; }
        /// <summary>
        /// The message of the log
        /// </summary>
        string Message { get; }
        /// <summary>
        /// The message details / more info
        /// </summary>
        string Info { get; }
    }

    /// <summary>
    /// Different levels of logging
    /// </summary>
    public enum LoggingLevel
    {
        /// <summary>
        /// Fatal error level probably resulting in system halt
        /// </summary>
        Fatal = 5,

        /// <summary>
        /// An error caused by an exception or other
        /// </summary>
        Error = 4,

        /// <summary>
        /// Warning log
        /// </summary>
        Warning = 3,

        /// <summary>
        /// A general notice
        /// </summary>
        Notice = 2,

        /// <summary>
        /// Other information
        /// </summary>
        Info = 1,

        /// <summary>
        /// The whole ball of wax!
        /// </summary>
        Ok = 0
    }
}