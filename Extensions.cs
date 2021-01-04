/* License
 * ------------------------------------------------------------------------------
 * Copyright (c) 2019 UX Digital Systems Ltd
 *
 * Permission is hereby granted, to any person obtaining a copy of this software
 * and associated documentation files (the "Software"), to deal in the Software
 * for the continued use and development of the system on which it was installed,
 * and to permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * Any persons obtaining the software have no rights to use, copy, modify, merge,
 * publish, distribute, sublicense, and/or sell copies of the Software without
 * written persmission from UX Digital Systems Ltd, if it is not for use on the
 * system on which it was originally installed.
 * ------------------------------------------------------------------------------
 * UX.Digital
 * ----------
 * http://ux.digital
 * support@ux.digital
 */

using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UX.Lib2.Models;

namespace UX.Lib2
{
    /// <summary>
    /// Extensions static class for generic extension methods
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Split camelcase type strings into seperate words
        /// </summary>
        /// <param name="s">String to convert</param>
        /// <returns>New string with split words</returns>
        /// <example>HelloWorld would covert to Hello World</example>
        public static string SplitCamelCase(this string s)
        {
            return Regex.Replace(Regex.Replace(s, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2"), @"(\p{Ll})(\P{Ll})", "$1 $2");
        }

        /// <summary>
        /// Truncate a string
        /// </summary>
        /// <param name="value">The string to truncate</param>
        /// <param name="maxLength">Max char count</param>
        /// <returns>Truncated string</returns>
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
        }

        /// <summary>
        /// Returns true is string is all digits
        /// </summary>
        /// <param name="value">The string to check</param>
        /// <returns></returns>
        public static bool IsAllDigits(this string value)
        {
            return value.All(c => c >= '0' && c <= '9');
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static byte[] ToByteArray(this string s)
        {
            byte[] result = new byte[s.Length];

            for (int i = 0; i < s.Length; i++)
            {
                result[i] = unchecked((byte)s[i]);
            }

            return result;
        }

        /// <summary>
        /// Pretty print a duration of time to string
        /// </summary>
        /// <param name="span">TimeSpan duration of time to </param>
        /// <returns>String of result</returns>
        public static string ToPrettyFormat(this TimeSpan span)
        {
            if (span == TimeSpan.Zero) return "0 minutes";

            var sb = new StringBuilder();
            if (span.Days > 0)
                sb.AppendFormat("{0} day{1} ", span.Days, span.Days > 1 ? "s" : String.Empty);
            if (span.Hours > 0)
                sb.AppendFormat("{0} hour{1} ", span.Hours, span.Hours > 1 ? "s" : String.Empty);
            if (span.Minutes > 0)
                sb.AppendFormat("{0} minute{1} ", span.Minutes, span.Minutes > 1 ? "s" : String.Empty);

            return sb.ToString();
        }

        /// <summary>
        /// Get a string in the form of 'time ago' for a duration of time gone.
        /// </summary>
        /// <param name="ts">Duration of time</param>
        /// <returns>Formatted string</returns>
        public static string ToPrettyTimeAgo(this TimeSpan ts)
        {
            double delta = Math.Abs(ts.TotalSeconds);

            if (delta < 120)
            {
                return "just now";
            }
            if (delta < 2700) // 45 * 60
            {
                return ts.Minutes + " minutes ago";
            }
            if (delta < 7200) // 120 * 60
            {
                return "about an hour ago";
            }
            if (delta < 86400) // 24 * 60 * 60
            {
                return ts.Hours + " hours ago";
            }
            if (delta < 172800) // 48 * 60 * 60
            {
                return "yesterday";
            }
            if (delta < 2592000) // 30 * 24 * 60 * 60
            {
                return ts.Days + " days ago";
            }
            if (delta < 31104000) // 12 * 30 * 24 * 60 * 60
            {
                int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "one month ago" : months + " months ago";
            }
            int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
            return years <= 1 ? "one year ago" : years + " years ago";
        }

        public static bool IsEmailAddress(this string str)
        {
            return Regex.IsMatch(str,
                @"(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])",
                RegexOptions.IgnoreCase);
        }

        public static bool IsTelephoneNumber(this string str)
        {
            return Regex.IsMatch(str, @"^[+\ \-\(\)\d]+$");
        }

        public static string BootStrapAlertClass(this IStatusMessageItem item)
        {
            switch (item.MessageLevel)
            {
                case StatusMessageWarningLevel.Ok:
                    return "alert-primary";
                case StatusMessageWarningLevel.Notice:
                    return "alert-info";
                case StatusMessageWarningLevel.Error:
                    return "alert-danger";
                default:
                    return string.Format("alert-{0}", item.MessageLevel.ToString().ToLower());
            }
        }
    }
}