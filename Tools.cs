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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.CrestronXml;
using Crestron.SimplSharp.CrestronXmlLinq;
using Crestron.SimplSharp.Reflection;
using Crestron.SimplSharpPro.DM;
using UX.Lib2.Models;

namespace UX.Lib2
{
    /// <summary>
    /// A generic static class for useful tools
    /// </summary>
    public static class Tools
    {
        /// <summary>
        /// Print the version of an assembly with the branded header to console. Use on program boot at start of Ctor()
        /// </summary>
        /// <param name="projectAssembly">The assembly to get the version info from</param>
        public static void PrintLibInfo(Assembly projectAssembly)
        {
            var info = GetProgramInfo();
            CrestronConsole.PrintLine("");
            CrestronConsole.PrintLine(@"_________________________________________________________");
            CrestronConsole.PrintLine(@"                                                         ");
            CrestronConsole.Print(Debug.AnsiCyan);
            CrestronConsole.PrintLine(@"     _   ___  __  ____  _       _ _        _                ");
            CrestronConsole.PrintLine(@"    | | | \ \/ / |  _ \(_) __ _(_) |_ __ _| |               ");
            CrestronConsole.PrintLine(@"    | | | |\  /  | | | | |/ _` | | __/ _` | |               ");
            CrestronConsole.PrintLine(@"    | |_| |/  \ _| |_| | | (_| | | || (_| | |               ");
            CrestronConsole.PrintLine(@"     \___//_/\_(_)____/|_|\__, |_|\__\__,_|_|               ");
            CrestronConsole.PrintLine(@"                          |___/                             ");
            CrestronConsole.Print(Debug.AnsiReset);
            CrestronConsole.PrintLine(@"                                                         ");
            CrestronConsole.PrintLine(@"    UX Digital Systems Limited                           ");
            CrestronConsole.Print(Debug.AnsiPurple);
            CrestronConsole.PrintLine(@"    http://ux.digital                                    ");
            CrestronConsole.Print(Debug.AnsiReset);
            CrestronConsole.PrintLine(@"_________________________________________________________");
            CrestronConsole.PrintLine(@"");
            CrestronConsole.PrintLine("System Name: {0}", info["System Name"]);
            CrestronConsole.PrintLine(GetAssemblyNameAndVersionString(projectAssembly));
            CrestronConsole.PrintLine(@"_________________________________________________________");
            CrestronConsole.PrintLine(@"");
            foreach (var pair in info)
            {
                CrestronConsole.PrintLine("{0}: {1}", pair.Key, pair.Value);
            }
            CrestronConsole.PrintLine(@"_________________________________________________________");
        }

        public static string GetAssemblyNameAndVersionString(Assembly assembly)
        {
            var version = assembly.GetName().Version;
            return string.Format("{0} v{1}.{2:D2}.{3:D2} ({4})", assembly.GetName().Name,
                version.Major, version.Minor, version.Build, version.Revision);
        }

        public static ReadOnlyDictionary<string, string> GetProgramInfo()
        {
            using (
                var stream =
                    new FileStream(
                        string.Format("{0}\\ProgramInfo.config", InitialParametersClass.ProgramDirectory),
                        FileMode.Open))
            {
                using (var xml = new XmlReader(stream))
                {
                    var doc = XDocument.Load(xml);

                    var dict = new Dictionary<string, string>();

                    var requiredInfo = doc.Root.Element("RequiredInfo");
                    dict["System Name"] = requiredInfo.Element("SystemName").Value;

                    var optionalInfo = doc.Root.Element("OptionalInfo");
                    try
                    {
                        dict["Programmer Name"] = optionalInfo.Element("ProgrammerName").Value;
                    }
                    catch
                    {
                        dict["Programmer Name"] = "Unknown";
                    }
                    dict["App Build Date"] = optionalInfo.Element("CompiledOn").Value;
                    dict["App Version"] = optionalInfo.Element("CompilerRev").Value;

                    var plugin = doc.Root.Element("Plugin");
                    dict["SIMPLSharp Version"] = plugin.Element("Version").Value;
                    dict["Include4.dat Version"] = plugin.Element("Include4.dat").Value;

                    return new ReadOnlyDictionary<string, string>(dict);
                }
            }
        }

        /// <summary>
        /// Print an array of bytes to the console
        /// </summary>
        /// <param name="bytes">Byte array to print</param>
        /// <param name="startIndex">Start index of the array</param>
        /// <param name="length">Count of bytes to print</param>
        public static void PrintBytes(byte[] bytes, int startIndex, int length)
        {
            PrintBytes(bytes, startIndex, length, false);
        }

        /// <summary>
        /// Print a collection of bytes to the console for debugging
        /// </summary>
        /// <param name="bytes">Byte array to print</param>
        /// <param name="startIndex">Start index of the array</param>
        /// <param name="length">Number of bytes to print</param>
        /// <param name="showReadable">true if you want to display readable characters as normal ascii</param>
        public static void PrintBytes(byte[] bytes, int startIndex, int length, bool showReadable)
        {
            CrestronConsole.PrintLine(GetBytesAsReadableString(bytes, startIndex, length, showReadable));
        }

        public static string GetBytesAsReadableString(byte[] bytes, int startIndex, int length, bool showReadable)
        {
            var result = string.Empty;

            for (int i = startIndex; i < length; i++)
            {
                if (showReadable && bytes[i] >= 32 && bytes[i] < 127)
                {
                    result = result + string.Format("{0}", (char)bytes[i]);
                }
                else
                {
                    result = result + @"\x" + bytes[i].ToString("X2");
                }
            }

            return result;
        }

        /// <summary>
        /// Scale a number range
        /// </summary>
        /// <param name="value">Number to scale</param>
        /// <param name="fromMinValue">The current min value of the number</param>
        /// <param name="fromMaxValue">The current max value of the number</param>
        /// <param name="toMinValue">The new min value of the new range</param>
        /// <param name="toMaxValue">The new max value of the new range</param>
        /// <returns></returns>
        public static double ScaleRange(double value,
          double fromMinValue, double fromMaxValue,
          double toMinValue, double toMaxValue)
        {
            try
            {
                return (value - fromMinValue) *
                    (toMaxValue - toMinValue) /
                    (fromMaxValue - fromMinValue) + toMinValue;
            }
            catch
            {
                return double.NaN;
            }
        }

        public static void Increment(this IAudioLevelControl level, uint percentage)
        {
            if (percentage > 100)
            {
                throw new IndexOutOfRangeException("perecentage out of range");
            }
            var scaledLevel = ScaleRange(level.Level, ushort.MinValue, ushort.MaxValue, 0, 100);
            scaledLevel = scaledLevel + percentage;
            if (scaledLevel > 100)
            {
                scaledLevel = 100;
            }
            level.Level = (ushort) ScaleRange(scaledLevel, 0, 100, ushort.MinValue, ushort.MaxValue);
        }

        public static void Decrement(this IAudioLevelControl level, uint percentage)
        {
            if (percentage > 100)
            {
                throw new IndexOutOfRangeException("perecentage out of range");
            }
            var scaledLevel = ScaleRange(level.Level, ushort.MinValue, ushort.MaxValue, 0, 100);
            scaledLevel = scaledLevel - percentage;
            if (scaledLevel < 0)
            {
                scaledLevel = 0;
            }
            level.Level = (ushort)ScaleRange(scaledLevel, 0, 100, ushort.MinValue, ushort.MaxValue);
        }

        public static ushort GetPercentageLevel(this IAudioLevelControl control)
        {
            return (ushort) ScaleRange(control.Level, ushort.MinValue, ushort.MaxValue, 0, 100);
        }

        /// <summary>
        ///  Save a stream as a file
        /// </summary>
        /// <param name="input">The stream to save</param>
        /// <param name="filePath">The full file path where it will be saved to</param>
        public static void CreateFileFromResourceStream(Crestron.SimplSharp.CrestronIO.Stream input, string filePath)
        {
            using (Crestron.SimplSharp.CrestronIO.Stream output = Crestron.SimplSharp.CrestronIO.File.Create(filePath))
            {
                CopyStream(input, output);
            }
        }

        /// <summary>
        /// Copy a stream byte for byte
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        public static void CopyStream(Crestron.SimplSharp.CrestronIO.Stream input, Crestron.SimplSharp.CrestronIO.Stream output)
        {
            // Insert null checking here for production
            var buffer = new byte[8192];

            int bytesRead;
            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
        }

        private static readonly string[] SizeSuffixes =
        {"bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"};

        /// <summary>
        /// Get a string showing size from bytes
        /// </summary>
        /// <param name="value">number of bytes</param>
        /// <param name="decimalPlaces">number of decimal places</param>
        public static string PrettyByteSize(Int64 value, int decimalPlaces)
        {
            var i = 0;
            var dValue = (decimal) value;
            while (Math.Round(dValue, decimalPlaces) >= 1000)
            {
                dValue /= 1024;
                i++;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}", dValue, SizeSuffixes[i]);
        }

        public static IEnumerable<IpTableResult> GetIpTable()
        {
            var reply = string.Empty;
            if (CrestronConsole.SendControlSystemCommand("ipt -t", ref reply) == false)
            {
                return new IpTableResult[]
                {

                };
            }

            var matches = Regex.Matches(reply,
                @"\ *(\w+)\ *\|(\w+)\ *\|(\w+)\ *\|(\w+)?\ *\|(\d+)\ *\|([\d\.]+)\ *\|\ *([\w\-\ ]+)\ *?\|\ *([\w ]+)");

            return new ReadOnlyCollection<IpTableResult>((from Match match in matches
                select
                    new IpTableResult(int.Parse(match.Groups[1].Value.Trim(), NumberStyles.HexNumber), match.Groups[3].Value.Trim() == "ONLINE",
                        int.Parse(match.Groups[5].Value.Trim()), match.Groups[6].Value.Trim(),
                        match.Groups[7].Value.Trim())).ToList());
        }

        public static string GetDmInputEventIdName(int value)
        {
            var type = typeof (DMInputEventIds).GetCType();
            foreach (var field in type.GetFields())
            {
                if (field.FieldType != typeof (int)) continue;
                var v = (int) field.GetValue(null);
                if (v == value) return field.Name;
            }
            return "Unknown ID " + value;
        }

        public static string GetDmOutputEventIdName(int value)
        {
            var type = typeof(DMOutputEventIds).GetCType();
            foreach (var field in type.GetFields())
            {
                if (field.FieldType != typeof(int)) continue;
                var v = (int)field.GetValue(null);
                if (v == value) return field.Name;
            }
            return "Unknown ID " + value;
        }
    }

    public class IpTableResult
    {
        private readonly int _cipId;
        private readonly bool _online;
        private readonly int _port;
        private readonly string _ipAddress;
        private readonly string _modelName;

        public IpTableResult(int cipId, bool online, int port, string ipAddress, string modelName)
        {
            _cipId = cipId;
            _online = online;
            _port = port;
            _ipAddress = ipAddress;
            _modelName = modelName;
        }

        public int CipId
        {
            get { return _cipId; }
        }

        public bool Online
        {
            get { return _online; }
        }

        public int Port
        {
            get { return _port; }
        }

        public string IPAddress
        {
            get { return _ipAddress; }
        }

        public string ModelName
        {
            get { return _modelName; }
        }
    }
}