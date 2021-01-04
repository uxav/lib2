using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.Net.Https;
using Crestron.SimplSharp.Reflection;
using Crestron.SimplSharpPro.CrestronThread;
using Crestron.SimplSharpPro.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using UX.Lib2.Logging;

namespace UX.Lib2.Cloud.Logger
{
    /// <summary>
    /// The client used for the cloud logging service.
    /// </summary>
    public static class CloudLog
    {
        private static readonly CCriticalSection InternalDictionaryLock = new CCriticalSection();
        private static readonly Dictionary<string, LogEntry> InternalDictionary = new Dictionary<string, LogEntry>();
        private static CTimer _startUploadProcessTimer;
        private static CTimer _startFileWriteProcessTimer;
        private static bool _programEnding;
        private static readonly CEvent UploadEvent = new CEvent(false, true);
        private static bool _started;
        private static bool _linkDown;
        private static string _projectKey;
        private static Assembly _assembly;
        private static DateTime _startupTime;
        private static ReadOnlyDictionary<string, string> _programInfo;
        private static DirectoryInfo _logDirectory;
        private static readonly CrestronQueue<LogEntry> FileWriteQueue = new CrestronQueue<LogEntry>(100);
        private static LoggingLevel _systemLogLevel = LoggingLevel.Notice;
        private static long _idCount = 0;
        private static DateTime _lastLogWrite;
        private static string _logText;

        /// <summary>
        /// Start the logging service. Sets level to default value of Notice
        /// </summary>
        /// <param name="assembly">The executing assembly of the app</param>
        /// <param name="projectKey">The cloud url safe key for the project in the database</param>
        public static void Start(Assembly assembly, string projectKey)
        {
            Start(assembly, projectKey, LoggingLevel.Notice);
        }

        /// <summary>
        /// Start the logging service
        /// </summary>
        /// <param name="assembly">The executing assembly of the app</param>
        /// <param name="projectKey">The cloud url safe key for the project in the database</param>
        /// <param name="level"></param>
        public static void Start(Assembly assembly, string projectKey, LoggingLevel level)
        {
            if (_started) return;
            _started = true;
            _startupTime = DateTime.UtcNow;
            _projectKey = projectKey;
            _assembly = assembly;
            _programInfo = Tools.GetProgramInfo();
            Level = level;

            CrestronEnvironment.ProgramStatusEventHandler += OnProgramStatusEventHandler;

            CrestronEnvironment.EthernetEventHandler += OnEthernetEventHandler;

            _startUploadProcessTimer = new CTimer(specific =>
            {
                var uploadThread = new Thread(UploadProcess, null, Thread.eThreadStartOptions.CreateSuspended)
                {
                    Name = "CloudLog Upload Process"
                };
                uploadThread.Start();
                //_startUploadProcessTimer.Dispose();
            }, 60000);

            Notice("CloudLog Client Started");
        }

        /// <summary>
        /// Setup logging to a file. This should really be a external SD Card path!!
        /// Suggested path: \RM\Logs
        /// </summary>
        /// <param name="directoryPath">Path of directory to use for the log files</param>
        public static void EnableLoggingToFile(string directoryPath)
        {
            if(!Directory.Exists(directoryPath))
                Directory.Create(directoryPath);

            _logDirectory = new DirectoryInfo(directoryPath);

            _startFileWriteProcessTimer = new CTimer(specific =>
            {
                var fileWriteThread = new Thread(FileWriteProcess, null, Thread.eThreadStartOptions.CreateSuspended)
                {
                    Name = "CloudLog FileWrite Process",
                    Priority = Thread.eThreadPriority.LowestPriority
                };
                fileWriteThread.Start();
                //_startFileWriteProcessTimer.Dispose();
            }, 60000);
        }

        private static FileInfo CurrentLogFile
        {
            get
            {
                if (_logDirectory == null) return null;
                var files = _logDirectory.GetFiles("CloudLog_*.log").OrderByDescending(f => f.LastWriteTime);
                if (!files.Any()) return CreateNewLogFile();
                return files.First().Length < (1024*1024) ? files.First() : CreateNewLogFile();
            }
        }

        private static FileInfo CreateNewLogFile()
        {
            var dateTimeString = string.Format("{0:yyyy-MM-dd_HH-mm-ss}", DateTime.Now);
            var file = File.Create(_logDirectory.FullName +
                                   string.Format("\\CloudLog_{0}.log", dateTimeString));
            file.Write(string.Format("Log file created {0}\r\n", dateTimeString), Encoding.UTF8);
            file.Close();
            return new FileInfo(file.Name);
        }

        private static void OnEthernetEventHandler(EthernetEventArgs ethernetEventArgs)
        {
            if (ethernetEventArgs.EthernetAdapter != EthernetAdapterType.EthernetLANAdapter) return;
            _linkDown = ethernetEventArgs.EthernetEventType == eEthernetEventType.LinkDown;
            if (_linkDown)
                Warn("{0} link down", ethernetEventArgs.EthernetAdapter);
            else
                Notice("{0} link up", ethernetEventArgs.EthernetAdapter);
        }

        private static void OnProgramStatusEventHandler(eProgramStatusEventType programEventType)
        {
            _programEnding = programEventType == eProgramStatusEventType.Stopping;
            if (_programEnding)
            {
                Notice(string.Format("CloudLog Client Stopping - App {0} is stopping",
                    InitialParametersClass.ApplicationNumber));
            }
        }

        public static string GetCurrentBootLogFromSystem()
        {
            var dir = new DirectoryInfo(@"/Sys/PLog/CurrentBoot");
            var logText = string.Empty;
            var sw = new Stopwatch();
            sw.Start();
#if DEBUG
            Lib2.Debug.WriteInfo("GetCurrentBootLogFromSystem()", "Looking in \"{0}\"", dir.FullName);
#endif
            var files = dir.GetFiles("Crestron_*.log").OrderBy(f => f.CreationTime);

            var lastLogWrite = files.Last().LastWriteTime;
            if (_lastLogWrite == lastLogWrite)
            {
                sw.Stop();
                return _logText + CrestronEnvironment.NewLine + "Cached, loaded in " + sw.Elapsed;
            }

            _lastLogWrite = lastLogWrite;

            foreach (var file in files)
            {
#if DEBUG
                Lib2.Debug.WriteInfo("GetCurrentBootLogFromSystem()", "Getting contents of \"{0}\" ({1})", file.Name, file.CreationTime);
#endif
                using (var contents = new StreamReader(File.Open(file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    logText = logText + contents.ReadToEnd();
                }
            }

            _logText = logText;

            return _logText + CrestronEnvironment.NewLine + "Loaded from file in " + sw.Elapsed;
        }

        public static IEnumerable<ILogEntry> GetSystemLogs()
        {
            var logs = new List<ILogEntry>();

            var logData = GetCurrentBootLogFromSystem();

            var firstTimeMatch = Regex.Match(logData, @"(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})");
            var time = DateTime.ParseExact(firstTimeMatch.Groups[1].Value, @"yyyy-MM-dd HH:mm:ss",
                CultureInfo.InvariantCulture);
            time = time.ToUniversalTime();

            var lines = logData.Split(CrestronEnvironment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            var lineCount = 0;
            SystemLog log = null;

            foreach (var line in lines)
            {
                lineCount++;

                var match = Regex.Match(line,
                    @"^(\w+): ([\w\.]+)(?: +\[App +(\d+)\])? +# +(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}) +# +(.+)");
                if (match.Success)
                {
                    try
                    {
                        time = DateTime.ParseExact(match.Groups[4].Value, @"yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        time = time.ToUniversalTime();
                        log = new SystemLog
                        {
                            Id = string.Format("{0}_{1}", time.ToString("s"), lineCount),
                            Level = (LoggingLevel) Enum.Parse(typeof (LoggingLevel), match.Groups[1].Value, true),
                            Time = time,
                            Message = match.Groups[5].Value,
                            Process = match.Groups[2].Value
                        };

                        if (match.Groups[3].Success && !string.IsNullOrEmpty(match.Groups[3].Value))
                        {
                            log.AppIndex = uint.Parse(match.Groups[3].Value);
                        }

                        logs.Add(log);
                        continue;
                    }
                    catch
                    {
                        log = null;
                    }
                }

                var exceptionMatch = Regex.Match(line, @"^Exception (.+): (.+)");
                if (exceptionMatch.Success)
                {
                    log = new SystemLog
                    {
                        Id = string.Format("{0}_{1}", time.ToString("s"), lineCount),
                        Level = LoggingLevel.Error,
                        Time = time,
                        Message = "Exception " + match.Groups[1].Value + " - " + match.Groups[2].Value
                    };

                    logs.Add(log);
                    continue;
                }

                if (log != null && log.Message.StartsWith("Exception"))
                {
                    log.Info = log.Info + "   " + line;
                    continue;
                }

                var infoMatch = Regex.Match(line, @"^   (.+)");
                if (log != null && infoMatch.Success)
                {
                    log.Info = log.Info + (string.IsNullOrEmpty(log.Info) ? string.Empty : CrestronEnvironment.NewLine) +
                               infoMatch.Groups[1].Value;
                    continue;
                }

                if(!Regex.IsMatch(line, @"\w")) continue;

                logs.Add(new SystemLog
                {
                    Id = string.Format("{0}_{1}", time.ToString("s"), lineCount),
                    Time = time,
                    Level = LoggingLevel.Ok,
                    Message = line
                });
            }

            return logs;
        } 

        /// <summary>
        /// Register the service in the console
        /// </summary>
        public static void RegisterConsoleService()
        {
            try
            {
                Debug("CloudLog.RegisterConsoleService() called");
                CrestronConsole.AddNewConsoleCommand(parameters =>
                {
                    try
                    {
                        var args = parameters.Split(' ');
                        switch (args[0].ToLower())
                        {
                            case "mode":
                                Level = (LoggingLevel) Enum.Parse(typeof (LoggingLevel), args[1], true);
                                CrestronConsole.ConsoleCommandResponse("Logging level set to: {0}", Level);
                                break;
                            case "cloud":
                                foreach (var entry in GetCloud(int.Parse(args[1]), 0))
                                {
                                    CrestronConsole.ConsoleCommandResponse(entry + "\r\n");
                                }
                                break;
                            case "clear":
                                if (args.Length > 1 && args[1] == "all")
                                    ClearAll();
                                else
                                    Clear();
                                break;
                            case "test":
                                try
                                {
                                    throw new Exception(args[1]);
                                }
                                catch (Exception e)
                                {
                                    Exception(e);
                                }
                                break;
                            default:
                                foreach (var entry in Get())
                                {
                                    CrestronConsole.ConsoleCommandResponse(entry + "\r\n");
                                }
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        CrestronConsole.ConsoleCommandResponse("Error: {0}\r\n{1}", e.Message, e.StackTrace);
                    }
                }, "Log", "Cloud logger", ConsoleAccessLevelEnum.AccessOperator);
            }
            catch (Exception e)
            {
                Exception("Error occured trying to register CloudLog commands to console service", e);
            }
        }

        private static string Post(string method, object args)
        {
            var system = new
            {
                mac_address = CrestronEthernetHelper.GetEthernetParameter(
                    CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_MAC_ADDRESS,
                    CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(EthernetAdapterType.EthernetLANAdapter)),
                ip_address = CrestronEthernetHelper.GetEthernetParameter(
                    CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS,
                    CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(EthernetAdapterType.EthernetLANAdapter)),
                host_name = CrestronEthernetHelper.GetEthernetParameter(
                    CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_HOSTNAME,
                    CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(EthernetAdapterType.EthernetLANAdapter)),
                model = InitialParametersClass.ControllerPromptName,
                version = InitialParametersClass.FirmwareVersion,
                ram_free = SystemMonitor.RAMFree * 1000
            };

            var app = new
            {
                index = InitialParametersClass.ApplicationNumber,
                name = InitialParametersClass.ProgramIDTag,
                version = _assembly.GetName().Version.ToString(),
                last_start_time = _startupTime,
                build_date = _programInfo["App Build Date"],
                programmer = _programInfo["Programmer Name"],
                simpl_sharp_version = _programInfo["SIMPLSharp Version"],
                include_4_dat_version = _programInfo["Include4.dat Version"]
            };

            var requestObject = new
            {
                method,
                @params = args,
                project_key = _projectKey,
                system,
                app
            };

            var request = new HttpsClientRequest
            {
                RequestType = RequestType.Post,
                ContentSource = ContentSource.ContentString,
                ContentString =
                    JsonConvert.SerializeObject(requestObject, Formatting.Indented, new IsoDateTimeConverter()),
                Header = {ContentType = "application/json"},
                KeepAlive = false,
                Url = new Crestron.SimplSharp.Net.Http.UrlParser(@"https://crestroncloudlogger.appspot.com/api")
            };

            using (var client = new HttpsClient {IncludeHeaders = false})
            {
                var response = client.Dispatch(request);
                CrestronConsole.PrintLine("Logger response: {0}", response.Code);
                CrestronConsole.PrintLine("Logger rx:\r\n{0}", response.ContentString);
                return response.ContentString;
            }
        }

        private static object UploadProcess(object userSpecific)
        {
            Notice("CloudLog UploadProcess Started");

            while (true)
            {
                CrestronEnvironment.AllowOtherAppsToRun();

                if (_linkDown && !_programEnding)
                    Thread.Sleep(60000);
                else if (!_linkDown)
                {
                    try
                    {
                        var entries = PendingLogs.Take(50);

                        try
                        {
#if DEBUG
                            //CrestronConsole.PrintLine("Updating autodiscovery query...");
#endif
                            var query = EthernetAutodiscovery.Query();

#if DEBUG
                            //CrestronConsole.PrintLine("Autodiscovery {0}", query);
#endif
                            if (query ==
                                EthernetAutodiscovery.eAutoDiscoveryErrors.AutoDiscoveryOperationSuccess)
                            {
                                var adresults = EthernetAutodiscovery.DiscoveredElementsList;
#if DEBUG
                                //CrestronConsole.PrintLine("Found {0} devivces:", adresults.Count);
#endif
                                foreach (var adresult in adresults)
                                {
                                    //CrestronConsole.PrintLine("{0}, {1}, {2}", adresult.IPAddress,
                                        //adresult.HostName, adresult.DeviceIdString);
                                }
                            }
#if DEBUG
                            //CrestronConsole.PrintLine("CloudLog uploading to cloud...");
#endif

                            var contents = Post("submit_logs", entries);
                            var data = JObject.Parse(contents);
#if DEBUG
                            //CrestronConsole.PrintLine("CloudLog client response: {0}", data["status"].Value<string>());
#endif
                            var results = data["results"];
                            foreach (var id in results.Select(result => result["id"].Value<string>()))
                            {
                                InternalDictionary[id].CloudId = id;
                            }
                        }
                        catch (Exception e)
                        {
                            Error("Error with CloudLog upload process, {0}", e.Message);
                            if (_programEnding)
                                return null;
                            CrestronConsole.PrintLine("CloudLog could not upload to cloud service. {0} items pending",
                                PendingLogs.Count);
                            Thread.Sleep(60000);
                        }
                    }
                    catch (Exception e)
                    {
                        ErrorLog.Error("Error in CloudLog.PostToCloudProcess, {1}", e.Message);
                    }
                }

                if (_programEnding)
                    return null;

                if (PendingLogs.Count == 0)
                    UploadEvent.Wait(60000);
            }
        }

        private static object FileWriteProcess(object userSpecific)
        {
            Notice("CloudLog FileWriteProcess Started");

            while (true)
            {
                CrestronEnvironment.AllowOtherAppsToRun();

                var log = FileWriteQueue.Dequeue();

                using (var writer = CurrentLogFile.AppendText())
                {
                    writer.WriteLine(log);
                    while (!FileWriteQueue.IsEmpty)
                    {
                        writer.WriteLine(FileWriteQueue.Dequeue());
                    }
                }

                if (_programEnding && FileWriteQueue.IsEmpty)
                    return null;
            }
        }

        public static void Clear()
        {
            InternalDictionaryLock.Enter();
            InternalDictionary.Clear();
            InternalDictionaryLock.Leave();
        }

        public static void ClearAll()
        {
            Post("clear_logs", null);

            InternalDictionaryLock.Enter();
            InternalDictionary.Clear();
            InternalDictionaryLock.Leave();
        }

        /// <summary>
        /// Get the current local error log
        /// </summary>
        /// <returns>List of log entries since boot</returns>
        public static IEnumerable<LogEntry> Get()
        {
            return InternalDictionary.Values;
        }

        /// <summary>
        /// Get logs which are stored in the cloud
        /// </summary>
        /// <param name="limit">limit of entries to be returned</param>
        /// <param name="offset">offset the query by an amount</param>
        /// <returns>List of cloud entries</returns>
        public static IEnumerable<LogEntry> GetCloud(int limit, int offset)
        {
            var args = new
            {
                limit,
                offset
            };
            var results = JObject.Parse(Post("get_logs", args))["results"];
            var logs = (from JObject result in results select new LogEntry(result));
            return logs.OrderBy(e => e.Time);
        } 

        internal static void WriteLog(LoggingLevel level, string message, string process, string stack, bool printToConsole)
        {
            var now = DateTime.UtcNow;
            _idCount ++;
            var id = now.ToString("s") + "_" + _idCount;
            var m = message;
            var info = string.Empty;
            if (message.Contains(CrestronEnvironment.NewLine))
            {
                var lines = message.Split(CrestronEnvironment.NewLine.ToCharArray());
                m = lines.First();
                info = String.Join(CrestronEnvironment.NewLine, lines
                    .Where(l => l.Length > 0)
                    .Skip(1)
                    .ToArray());
            }
            var entry = new LogEntry
            {
                Time = now,
                Id = id,
                Level = level,
                Message = m,
                Info = info,
                Process = process,
                Stack = stack
            };

            InternalDictionaryLock.Enter();
            InternalDictionary[entry.Id] = entry;
            InternalDictionaryLock.Leave();
            if (CurrentLogFile != null)
            {
                if(!FileWriteQueue.TryToEnqueue(entry))
                    Error("Could not enqueue log entry to file write queue!");
            }

            try
            {
                UploadEvent.Set();
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error setting UploadEvent in WriteLog, {0}", e.Message);
            }

            if (!printToConsole) return;
            if (entry.Level >= LoggingLevel.Error)
            {
                Lib2.Debug.WriteError(entry.ToString(true));
            }
            else switch (entry.Level)
            {
                case LoggingLevel.Warning:
                    Lib2.Debug.WriteWarn(entry.ToString(true));
                    break;
                case LoggingLevel.Notice:
                    Lib2.Debug.WriteInfo(entry.ToString(true));
                    break;
                case LoggingLevel.Info:
                    Lib2.Debug.WriteSuccess(entry.ToString(true));
                    break;
                default:
                    Lib2.Debug.WriteNormal(entry.ToString(true));
                    break;
            }
        }

        internal static void Log(LoggingLevel level, string message, params object[] args)
        {
            if (level < Level) return;
            WriteLog(level, args.Length > 0 ? string.Format(message, args) : message, "", "",
                Level == LoggingLevel.Ok);
        }

        /// <summary>
        /// Write a debug level message
        /// </summary>
        /// <param name="message">The message to write to the log</param>
        /// <param name="args">Optional args</param>
        public static void Debug(string message, params object[] args)
        {
            if (SystemLogLevel <= LoggingLevel.Ok)
            {
                if (args.Length > 0)
                    ErrorLog.Ok(message, args);
                else
                    ErrorLog.Ok(message);
            }
            Log(LoggingLevel.Ok, message, args);
        }

        /// <summary>
        /// Write a notice level message. Also writes to system log
        /// </summary>
        /// <param name="message">The message to write to the log</param>
        /// <param name="args">Optional args</param>
        public static void Notice(string message, params object[] args)
        {
            if (SystemLogLevel <= LoggingLevel.Notice)
            {
                if (args.Length > 0)
                    ErrorLog.Notice(message, args);
                else
                    ErrorLog.Notice(message);
            }
            Log(LoggingLevel.Notice, message, args);
        }

        /// <summary>
        /// Write a info level message
        /// </summary>
        /// <param name="message">The message to write to the log</param>
        /// <param name="args">Optional args</param>
        public static void Info(string message, params object[] args)
        {
            if (SystemLogLevel <= LoggingLevel.Info)
            {
                if (args.Length > 0)
                    ErrorLog.Info(message, args);
                else
                    ErrorLog.Info(message);
            }
            Log(LoggingLevel.Info, message, args);
        }

        /// <summary>
        /// Write a warning level message. Also writes to system log
        /// </summary>
        /// <param name="message">The message to write to the log</param>
        /// <param name="args">Optional args</param>
        public static void Warn(string message, params object[] args)
        {
            if (SystemLogLevel <= LoggingLevel.Warning)
            {
                if (args.Length > 0)
                    ErrorLog.Warn(message, args);
                else
                    ErrorLog.Warn(message);
            }
            Log(LoggingLevel.Warning, message, args);
        }

        /// <summary>
        /// Write a error level message. Also writes to system log
        /// </summary>
        /// <param name="message">The message to write to the log</param>
        /// <param name="args">Optional args</param>
        public static void Error(string message, params object[] args)
        {
            if (SystemLogLevel <= LoggingLevel.Error)
            {
                if (args.Length > 0)
                    ErrorLog.Error(message, args);
                else
                    ErrorLog.Error(message);
            }
            Log(LoggingLevel.Error, message, args);
        }

        /// <summary>
        /// Write a fatal error level message. Also writes to system log
        /// </summary>
        /// <param name="message">The message to write to the log</param>
        /// <param name="e">Exception causing error</param>
        public static void Fatal(string message, Exception e)
        {
            var m = string.Format("Fatal error: {0}", message);
            ErrorLog.Exception(m, e);
            Log(LoggingLevel.Fatal, string.Format("{0} raised, {1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
        }

        /// <summary>
        /// Write an exception error log. Also writes to system log
        /// </summary>
        /// <param name="e">Exception</param>
        public static void Exception(Exception e)
        {
            ErrorLog.Exception(e.GetType().Name, e);
            Log(LoggingLevel.Error, string.Format("{0} raised, {1}\r\n{2}", e.GetType().Name, e.Message, e.StackTrace));
        }

        /// <summary>
        /// Write an exception error log. Also writes to system log
        /// </summary>
        /// <param name="message">The message to write to the log</param>
        /// <param name="e">Exception</param>
        public static void Exception(string message, Exception e)
        {
            ErrorLog.Exception(message, e);
            Log(LoggingLevel.Error,
                string.Format("{0}, {1}, {2}\r\n{3}", message, e.GetType().Name, e.Message, e.StackTrace));
        }

        /// <summary>
        /// Write an exception error log. Also writes to system log
        /// </summary>
        /// <param name="e">The exception</param>
        /// <param name="message">The message</param>
        /// <param name="args">The message args</param>
        public static void Exception(Exception e, string message, params object[] args)
        {
            Exception(string.Format(message, args), e);
        }

        /// <summary>
        /// Set / Get the logging level
        /// </summary>
        public static LoggingLevel Level { set; get; }

        /// <summary>
        /// Set / Get the logging level to the system logs... by default this is Notice
        /// </summary>
        public static LoggingLevel SystemLogLevel
        {
            set { _systemLogLevel = value; }
            get { return _systemLogLevel; }
        }

        internal static List<LogEntry> PendingLogs
        {
            get
            {
                return InternalDictionary
                    .Values
                    .Where(e => e.CloudId.Length == 0)
                    .OrderBy(e => e.Id).ToList();
            }
        }
    }
}