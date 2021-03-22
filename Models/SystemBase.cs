using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Crestron.SimplSharp;
using Crestron.SimplSharp.AutoUpdate;
using Crestron.SimplSharp.CrestronAuthentication;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.Reflection;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.Fusion;
using Crestron.SimplSharpPro.UI;
using Newtonsoft.Json.Linq;
using UX.Lib2.Cloud.Logger;
using UX.Lib2.Config;
using UX.Lib2.DeviceSupport;
using UX.Lib2.UI;
using UX.Lib2.WebScripting2;
using Thread = Crestron.SimplSharpPro.CrestronThread.Thread;

namespace UX.Lib2.Models
{
    public abstract class SystemBase
    {
        #region Fields

        private readonly CrestronControlSystem _controlSystem;
        private readonly Assembly _appAssembly;
        private Thread _systemThread;
        private bool _programStopping;
        private readonly List<UserPrompt> _userPrompts = new List<UserPrompt>();
        private readonly CCriticalSection _userPromptsLock = new CCriticalSection();
        private UserPrompt _auPrompt;
        private Thread _systemStartupThread;
        private readonly CrestronQueue<InitializeProcess> _initializeQueue = new CrestronQueue<InitializeProcess>(500);
        private WebApp.WebApp _webApp;
        private CTimer _rebootTimer;
        private readonly CEvent _systemWait = new CEvent();
        private readonly CEvent _startupWait = new CEvent();
        private Thread _testScriptThread;
        private Thread _clockInitThread;
        private CTimer _clockTimer;
        private CTimer _systemTimer;
        private DateTime _lastSystemTimerCallback = DateTime.Now;
        private bool _initialized;
        private bool _timersBrokenFlag;

        #endregion

        #region Constructors

        /// <summary>
        /// Create an instance of a system for use on a CrestronControlSystem
        /// </summary>
        protected SystemBase(CrestronControlSystem controlSystem, Assembly appAssembly)
        {
            BootStatus = "Waiting for System.ctor()";

            _controlSystem = controlSystem;
            _appAssembly = appAssembly;
            CrestronEnvironment.ProgramStatusEventHandler +=
                type =>
                {
                    _programStopping = type == eProgramStatusEventType.Stopping;
                    try
                    {
                        _systemWait.Set();
                    }
                    catch (Exception e)
                    {
                        CloudLog.Error("Error calling _systemWait.Set() on program stop", e.Message);
                    }
                    try
                    {
                        _startupWait.Set();
                    }
                    catch (Exception e)
                    {
                        CloudLog.Error("Error calling _startupWait.Set() on program stop", e.Message);
                    }
                };
            Displays = new DisplayCollection();
            Sources = new SourceCollection();
            Rooms = new RoomCollection();
            UIControllers = new UIControllerCollection(this);

            Debug.WriteInfo("Checking for new app version");

            AppIsNewVersion = CheckIfNewVersion(appAssembly);

            if (AppIsNewVersion)
            {
                Debug.WriteWarn("New Version", "version = {0}, running upgrade scripts...",
                    appAssembly.GetName().Version.ToString());
// ReSharper disable once DoNotCallOverridableMethodsInConstructor
                AppShouldRunUpgradeScripts();
            }

            CrestronConsole.AddNewConsoleCommand(parameters => FusionRVI.GenerateFileForAllFusionDevices(),
                "RviGenerate", "Create RVI file for Fusion", ConsoleAccessLevelEnum.AccessOperator);
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events

        public event SystemStartupProgressEventHandler SystemStartupProgressChange;

        public event SystemClockTimeChangeEventHandler TimeChanged;

        #endregion

        #region Delegates
        #endregion

        #region Properties

        public string Hostname
        {
            get
            {
                return
                    CrestronEthernetHelper.GetEthernetParameter(
                        CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_HOSTNAME,
                        CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(
                            EthernetAdapterType.EthernetLANAdapter));
            }
        }

        public string IpAddress
        {
            get
            {
                return
                    CrestronEthernetHelper.GetEthernetParameter(
                        CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS,
                        CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(
                            EthernetAdapterType.EthernetLANAdapter));
            }
        }

        public string MacAddress
        {
            get
            {
                return
                    CrestronEthernetHelper.GetEthernetParameter(
                        CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_MAC_ADDRESS,
                        CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(
                            EthernetAdapterType.EthernetLANAdapter));
            }
        }

        public abstract string Name { get; }

        /// <summary>
        /// The CrestronControlSystem for this system
        /// </summary>
        public CrestronControlSystem ControlSystem
        {
            get { return _controlSystem; }
        }

        /// <summary>
        /// UIControllers for this system
        /// </summary>
        public UIControllerCollection UIControllers { get; private set; }

        /// <summary>
        /// Rooms for this system
        /// </summary>
        public RoomCollection Rooms { get; private set; }

        /// <summary>
        /// Displays in this system
        /// </summary>
        public DisplayCollection Displays { get; private set; }

        /// <summary>
        /// Sources in this system
        /// </summary>
        public SourceCollection Sources { get; private set; }

        public virtual IEnumerable<IStatusMessageItem> StatusMessages
        {
            get
            {
                var items = new List<IStatusMessageItem>();

                if (!Authentication.Enabled)
                {
                    items.Add(new StatusMessage(StatusMessageWarningLevel.Warning,
                        "System authentication is not enabled. Consider enabling system authentication on this control processor."));
                }

                if (SecondsSinceLastSystemTimerCallback > 10)
                {
                    items.Add(new StatusMessage(StatusMessageWarningLevel.Error,
                        "System timer failure. The system timer runs every second, but last called back {0} seconds ago. Consider rebooting the processor!",
                        SecondsSinceLastSystemTimerCallback));
                }

                else if (SecondsSinceLastSystemTimerCallback > 3)
                {
                    items.Add(new StatusMessage(StatusMessageWarningLevel.Warning,
                        "System timer slow to checkin. Last callback {0} seconds ago!",
                        SecondsSinceLastSystemTimerCallback));
                }

                else
                {
                    items.Add(new StatusMessage(StatusMessageWarningLevel.Ok,
                        "System timer checked in {0} seconds ago.",
                        SecondsSinceLastSystemTimerCallback));
                }

                return items;
            }
        }

        public int StatusMessagesErrorCount
        {
            get { return StatusMessages.Count(s => s.MessageLevel == StatusMessageWarningLevel.Error); }
        }

        public int StatusMessagesWarningCount
        {
            get { return StatusMessages.Count(s => s.MessageLevel == StatusMessageWarningLevel.Warning); }
        }

        /// <summary>
        /// A user prompt is currently showing system wide
        /// </summary>
        public bool UserPromptShowing
        {
            get { return _userPrompts.Any(p => p.State == PromptState.Shown && p.Room == null); }
        }

        internal ReadOnlyCollection<UserPrompt> UserPrompts
        {
            get { return _userPrompts.AsReadOnly(); }
        }

        public bool Booted { get; private set; }
        public bool WillRestart { get; private set; }

        public int BootProgressPercent { get; protected set; }

        public string BootStatus { get; protected set; }

        public WebApp.WebApp WebApp
        {
            get { return _webApp; }
        }

        public abstract AConfig Config { get; }

        public Assembly AppAssembly
        {
            get { return _appAssembly; }
        }

        public string VersionName
        {
            get { return AppAssembly.GetName().Name; }
        }

        public Version Version
        {
            get { return AppAssembly.GetName().Version; }
        }

        public bool AppIsNewVersion { get; private set; }

        public abstract string SystemTypeInfo { get; }

        public static string AppStoragePath
        {
            get
            {
                var dir =
                    new DirectoryInfo(string.Format("\\NVRAM\\app_{0}",
                        InitialParametersClass.ApplicationNumber.ToString("D2")));
                if (!dir.Exists)
                {
                    dir.Create();
                }
                return dir.FullName;
            }
        }

        public bool TestScriptRunning
        {
            get
            {
                return _testScriptThread != null && _testScriptThread.ThreadState == Thread.eThreadStates.ThreadRunning;
            }
        }

        public int SecondsSinceLastSystemTimerCallback
        {
            get
            {
                var span = DateTime.Now - _lastSystemTimerCallback;
                return (int) span.TotalSeconds;
            }
        }

        #endregion

        #region Methods

        public virtual void Initialize()
        {
            if(_initialized) return;

            _initialized = true;

            _systemTimer = new CTimer(SystemTimerCallback, null, 1000, 1000);

            InternalClockInitialize();

            var response = string.Empty;
            CrestronConsole.SendControlSystemCommand("webport", ref response);
            var match = Regex.Match(response, @"Webserver port *= *(\d+)");
            if (match.Success)
            {
                var port = int.Parse(match.Groups[1].Value);
                Debug.WriteWarn("Built-web server is using port {0}", port);
            }

            foreach (var process in GetSystemItemsToInitialize())
            {
                _initializeQueue.Enqueue(process);
            }

            foreach (var source in Sources.Where(s => s.Device != null))
            {
                var device = source.Device as IInitializeComplete;
                if (device != null)
                {
                    _initializeQueue.Enqueue(new InitializeProcess(source.Device.Initialize,
                        string.Format("Initializing Source Device: {0}", device.GetType().Name),
                        TimeSpan.Zero, device.CheckInitializedOk));
                }
                else
                {
                    _initializeQueue.Enqueue(new InitializeProcess(source.Device.Initialize,
                        string.Format("Initializing Source Device: {0}", source.Device.GetType().Name)));
                }
            }

            foreach (var room in Rooms)
            {
                _initializeQueue.Enqueue(new InitializeProcess(room.Initialize,
                    string.Format("Initializing Room: \"{0}\"", room.Name)));
            }

            var displays = Displays.Where(d => d.Device != null).ToArray();

            var count = 0;
            foreach (var display in displays)
            {
                count++;
                _initializeQueue.Enqueue(new InitializeProcess(display.Initialize,
                    string.Format("Initializing Display {0}", count)));
            }

            foreach (var room in Rooms.Where(r => r.FusionEnabled))
            {
                _initializeQueue.Enqueue(new InitializeProcess(room.FusionRegisterInternal,
                    string.Format("Registering Fusion for Room: \"{0}\"", room.Name), TimeSpan.FromSeconds(5)));
            }

            /*if (Rooms.Any(r => r.FusionEnabled))
            {
                _initializeQueue.Enqueue(new InitializeProcess(FusionRVI.GenerateFileForAllFusionDevices,
                    "Generating Fusion RVI file for discovery"));
            }*/

            if (UIControllers.Any())
            {
                _initializeQueue.Enqueue(new InitializeProcess(UIControllers.ConnectToDefaultRooms,
                    "Setting up UI Controllers"));
            }

            if (UIControllers.Any())
            {
                _initializeQueue.Enqueue(new InitializeProcess(UIControllers.Initialize,
                    "Initializing UI Controllers"));
            }

            if (_systemThread != null) return;

            _systemThread = new Thread(SystemThreadProcess, null)
            {
                Name = "UX.Lib2.SystemBase SystemThreadProcess()",
                Priority = Thread.eThreadPriority.LowestPriority
            };

            _systemStartupThread = new Thread(SystemStartupThreadProcess, null)
            {
                Name = "System Startup Thread Process",
                Priority = Thread.eThreadPriority.MediumPriority
            };
        }

        private void SystemTimerCallback(object userSpecific)
        {
            _lastSystemTimerCallback = DateTime.Now;
        }

        private void InternalClockInitialize()
        {
            _clockInitThread = new Thread(specific =>
            {
                CloudLog.Debug("InternalClockInitialize(), Waiting for seconds to be 0", GetType().Name);
                while (DateTime.Now.Second != 0)
                {
                    Thread.Sleep(50);
                    CrestronEnvironment.AllowOtherAppsToRun();
                }
                CloudLog.Notice("InternalClockInitialize(), Seconds should now be zero, time is now {1}, creating CTimer to track time",
                    GetType().Name,
                    DateTime.Now.ToString("T"));
                _clockTimer = new CTimer(s => OnTimeChange(), null, 60000, 60000);
                OnTimeChange();
                CloudLog.Info("InternalClockInitialize(), OnTimeChange() will be called every time time is 0 seconds", GetType().Name);
                return null;
            }, null, Thread.eThreadStartOptions.CreateSuspended)
            {
                Name = "Clock Init Thread",
                Priority = Thread.eThreadPriority.HighPriority
            };
            _clockInitThread.Start();
        }

        private void OnTimeChange()
        {
            var handler = TimeChanged;

            if (handler == null) return;
            try
            {
                handler(this, DateTime.Now);
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }
        }

        protected abstract IEnumerable<InitializeProcess> GetSystemItemsToInitialize();

        protected virtual void OnSystemStartupProgressChange(string message, ushort progressLevel, bool isComplete)
        {
            BootStatus = message;
#if DEBUG
            Debug.WriteInfo(message);
#endif
            BootProgressPercent = (int) Tools.ScaleRange(progressLevel, ushort.MinValue, ushort.MaxValue, 20, 100);
            var args = new SystemStartupProgressEventArgs(message, progressLevel, isComplete);
            var handler = SystemStartupProgressChange;
            try
            {
                if (handler != null) handler(this, args);
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }
        }

        public virtual void Reboot()
        {
            BootStatus = "System will now reboot - Please Wait";
            foreach (var uiController in UIControllers)
            {
                uiController.InternalSystemWillRestart(false);
            }
            WillRestart = true;
            _rebootTimer = new CTimer(specific =>
            {
                var response = string.Empty;
                CrestronConsole.SendControlSystemCommand("reboot", ref response);
            }, 2000);
        }

        public virtual void Restart()
        {
            BootStatus = "Application will now restart - Please Wait";
            foreach (var uiController in UIControllers)
            {
                uiController.InternalSystemWillRestart(false);
            }
            WillRestart = true;
            _rebootTimer = new CTimer(specific =>
            {
                var response = string.Empty;
                CrestronConsole.SendControlSystemCommand(
                    string.Format("progres -p{0}", InitialParametersClass.ApplicationNumber), ref response);
            }, 2000);
        }

        public virtual void LoadUpdate()
        {
            BootStatus = "Application will now update - Please Wait";
            foreach (var uiController in UIControllers)
            {
                uiController.InternalSystemWillRestart(true);
            }
            WillRestart = true;
            _rebootTimer = new CTimer(specific =>
            {
                var response = string.Empty;
                CrestronConsole.SendControlSystemCommand(
                    string.Format("progload -p{0}", InitialParametersClass.ApplicationNumber), ref response);
            }, 2000);
        }

        public virtual void FactoryReset()
        {
            CloudLog.Warn("FactoryReset() Called!!");

            var directory = new DirectoryInfo(AppStoragePath);

            foreach (var file in directory.GetFiles())
            {
                CloudLog.Warn("Deleting file {0}", file.FullName);
                file.Delete();
            }

            BootStatus = "Processor will now reboot - Please Wait";
            foreach (var uiController in UIControllers)
            {
                uiController.InternalSystemWillRestart(false);
            }
            WillRestart = true;
            _rebootTimer = new CTimer(specific =>
            {
                var response = string.Empty;
                CrestronConsole.SendControlSystemCommand("reboot", ref response);
            }, 2000);
        }

        protected abstract void AppShouldRunUpgradeScripts();

        private object SystemStartupThreadProcess(object userSpecific)
        {
            var startingCount = _initializeQueue.Count + 1;

            var timeout = 30;
            while (UIControllers.Any(ui => !ui.Device.IsOnline && !(ui.Device is XpanelForSmartGraphics)))
            {
                if (!_programStopping || timeout == 0) break;
                OnSystemStartupProgressChange("Waiting for devices to connect",
                    (ushort) Tools.ScaleRange(1, 0, startingCount, ushort.MinValue,
                        ushort.MaxValue), false);
                _startupWait.Wait(1000);
                timeout --;
            }

            if (_programStopping) return null;

            while (!_initializeQueue.IsEmpty && !_programStopping)
            {
                var process = _initializeQueue.Dequeue();

                var progress = startingCount - _initializeQueue.Count;
                OnSystemStartupProgressChange(process.ProcessDescription,
                    (ushort)
                        Tools.ScaleRange(progress, 0, startingCount, ushort.MinValue,
                            ushort.MaxValue), false);
                _startupWait.Wait((int) process.TimeToWaitBeforeRunningProcess.TotalMilliseconds);
                try
                {
                    process.InitializeDelegate();
                }
                catch (Exception e)
                {
                    CloudLog.Exception(e, "Error calling InitializeDelegate: \"{0}\"", process.ProcessDescription);
                }

                if (process.CheckCompleteCallback != null)
                {
                    var count = 0;
                    while (!process.CheckCompleteCallback() && count < 300)
                    {
                        count ++;
                        _startupWait.Wait(1000);
                    }
                }
                else
                {
                    _startupWait.Wait(500);
                }
            }

            if (_programStopping) return null;

            Booted = true;
            OnSystemStartupProgressChange("Booted OK", ushort.MaxValue, true);
            foreach (var uiController in UIControllers)
            {
                uiController.ShowMainView();
            }

            foreach (var room in Rooms)
            {
                room.CheckFusionAssetsForOnlineStatusAndReportErrors();
            }

            return null;
        }

        private void ProcessOnIsComplete()
        {
            throw new NotImplementedException();
        }

        public abstract void SetConfig(JToken fromData);

        public abstract void LoadConfigTemplate(string id);

        protected virtual void StartWebApp()
        {
            StartWebApp(80);
        }

        protected void StartWebApp(int portNumber)
        {
            CloudLog.Notice("Starting web app on port {0}", portNumber);
            _webApp = new WebApp.WebApp(this, portNumber);
            OnWebAppStarted();
        }

        protected virtual void OnWebAppStarted()
        {
            _webApp.AddRoute("/setup", typeof(SetupRedirectHandler));
        }

        private object SystemThreadProcess(object userSpecific)
        {
            try
            {
                while (true)
                {
                    if (_userPrompts.Count == 0)
                    {
                        _systemWait.Wait(2000);
                    }

                    if (_programStopping) return null;

                    if (_userPrompts.Count == 0)
                    {
                        goto CheckTimers;
                    }

                    try
                    {
                        var prompts = _userPrompts.ToArray();

                        // We have queued prompts that aren't showing as current system prompts
                        if (prompts.Any(p => p.State == PromptState.Queued) &&
                            !prompts.Any(p => p.State == PromptState.Shown && p.Room == null))
                        {
                            //Debug.WriteInfo("Prompts queued! System {0}, Room {1}",
                            //    _userPrompts.Count(p => p.State == PromptState.Queued && p.Room == null),
                            //    _userPrompts.Count(p => p.State == PromptState.Queued && p.Room != null));

                            // Look for any system prompts (not specific to a room)
                            var prompt = prompts.FirstOrDefault(p => p.State == PromptState.Queued && p.Room == null);

                            if (prompt != null)
                            {
                                var roomPrompts = prompts.Where(p => p.State == PromptState.Shown && p.Room != null);

                                // Close any active room prompts and reset to queue as system will override

                                var pArray = roomPrompts as UserPrompt[] ?? roomPrompts.ToArray();
                                foreach (var roomPrompt in pArray)
                                {
                                    roomPrompt.State = PromptState.Queued;
                                }

                                if (pArray.Any())
                                    Thread.Sleep(200);

                                prompt.State = PromptState.Shown;

                                //Debug.WriteInfo("Showing system prompt on all UIs!");

                                foreach (var uiController in UIControllers)
                                {
                                    uiController.ShowPrompt(prompt);
                                }
                            }

                            else
                            {
                                // No system prompts to show so we can show any room prompts

                                foreach (var room in Rooms)
                                {
                                    var room1 = room;

                                    // Already showing a prompt in this room ... skip the room
                                    if (_userPrompts.Any(p => p.State == PromptState.Shown && p.Room == room1))
                                        continue;

                                    prompt = prompts.FirstOrDefault(
                                        p => p.State == PromptState.Queued && p.Room == room1);

                                    if (prompt != null && prompt.State != PromptState.Cancelled)
                                    {
                                        prompt.State = PromptState.Shown;

                                        foreach (var uiController in UIControllers.ForRoom(room1))
                                        {
                                            uiController.ShowPrompt(prompt);
                                        }
                                    }
                                }
                            }
                        }

                        //if (prompts.Length > 0)
                        //    Debug.WriteInfo("Removing {0} expired prompts", prompts.Length);

                        foreach (var userPrompt in
                            prompts.Where(p => p.State != PromptState.Queued && p.State != PromptState.Shown))
                        {
                            _userPromptsLock.Enter();
                            _userPrompts.Remove(userPrompt);
                            _userPromptsLock.Leave();
                        }
                    }
                    catch (Exception e)
                    {
                        CloudLog.Exception(e);
                        Thread.Sleep(1000);
                    }

                CheckTimers:

                    if(!Booted) continue;

                    if (SecondsSinceLastSystemTimerCallback > 10 && !_timersBrokenFlag)
                    {
                        _timersBrokenFlag = true;
                        CloudLog.Error(
                            "System Timer last checked in {0} seconds ago. Will reboot processor in 10 minutes if not fixed!!",
                            SecondsSinceLastSystemTimerCallback);
                    }
                    else if (SecondsSinceLastSystemTimerCallback > 600 && _timersBrokenFlag)
                    {
                        CloudLog.Error("Automatic reboot now!");
                        var response = string.Empty;
                        CrestronConsole.SendControlSystemCommand("REBOOT", ref response);
                    }
                    else if (_timersBrokenFlag && SecondsSinceLastSystemTimerCallback < 10)
                    {
                        _timersBrokenFlag = false;
                        CloudLog.Warn("System Timer checked in {0} seconds ago after a delay", SecondsSinceLastSystemTimerCallback);
                    }

                    CrestronEnvironment.AllowOtherAppsToRun();
                }
            }
            catch (Exception e)
            {
                CloudLog.Exception(e, "Unhandled Exception in SystemThreadProcess()!");
                return null;
            }
        }

        public abstract IEnumerable<IDevice> GetSystemDevices();

        public void TestScriptStart(IDictionary<string, string> args)
        {
            if (_testScriptThread != null && _testScriptThread.ThreadState == Thread.eThreadStates.ThreadRunning)
            {
                throw new Exception("Script already running");
            }

            _testScriptThread = new Thread(TestScriptThreadProcess, args)
            {
                Priority = Thread.eThreadPriority.LowestPriority,
                Name = "System Testing Script Thread"
            };
        }

        public void TestScriptAbort()
        {
            if (_testScriptThread == null || _testScriptThread.ThreadState != Thread.eThreadStates.ThreadRunning)
            {
                throw new Exception("Script not running");
            }

            _testScriptThread.Abort();
        }

        private object TestScriptThreadProcess(object o)
        {
            try
            {
                var args = o as IDictionary<string, string>;
                TestScriptProcess(args);
            }
            catch (Exception e)
            {
                if (e is ThreadAbortException)
                {
                    CloudLog.Warn("TestScriptThreadProcess() script aborted");
                }
                else
                {
                    CloudLog.Exception(e, "Error running Test Script");
                    
                }
            }

            TestScriptEnded();

            return null;
        }

        public abstract void TestScriptProcess(IDictionary<string, string> args);

        public abstract void TestScriptEnded();

        /// <summary>
        /// Prompt all user interfaces for an action (system wide)
        /// </summary>
        /// <param name="responseCallBack">The callback delegate for when someone responds</param>
        /// <param name="customSubPageJoin">Set to 0 for default</param>
        /// <param name="title">The title of the prompt</param>
        /// <param name="subTitle">The subtitle of the response</param>
        /// <param name="timeOutInSeconds">Timeout in seconds for the prompt. 0 is no timeout</param>
        /// <param name="userDefinedObject">User defined object to pass</param>
        /// <param name="promptActions">Array of actions to display (eg. buttons on actionsheet)</param>
        /// <returns>UserPrompt instance</returns>
        public UserPrompt PromptUsers(PromptUsersResponse responseCallBack, uint customSubPageJoin, string title, string subTitle,
            uint timeOutInSeconds, object userDefinedObject, params PromptAction[] promptActions)
        {
            var prompt = new UserPrompt
            {
                Actions = promptActions.ToList(),
                CallBack = responseCallBack,
                Title = title,
                SubTitle = subTitle,
                TimeOutInSeconds = timeOutInSeconds,
                UserDefinedObject = userDefinedObject,
                CustomSubPageJoin = customSubPageJoin
            };

            AddPrompt(prompt);
            
            return prompt;
        }

        public UserPrompt PromptUsers(PromptUsersResponse responseCallBack, string title, string subTitle,
            uint timeOutInSeconds, object userDefinedObject, params PromptAction[] promptActions)
        {
            return PromptUsers(responseCallBack, 0, title, subTitle, timeOutInSeconds, userDefinedObject, promptActions);
        }

        internal UserPrompt PromptUsers(RoomBase room, PromptUsersResponse responseCallBack, uint customSubPageJoin, string title, string subTitle,
            uint timeOutInSeconds, object userDefinedObject, params PromptAction[] promptActions)
        {
            var prompt = new UserPrompt
            {
                Room = room,
                Actions = promptActions.ToList(),
                CallBack = responseCallBack,
                Title = title,
                SubTitle = subTitle,
                TimeOutInSeconds = timeOutInSeconds,
                UserDefinedObject = userDefinedObject,
                CustomSubPageJoin = customSubPageJoin
            };

            AddPrompt(prompt);

            return prompt;
        }

        private void AddPrompt(UserPrompt prompt)
        {
            _userPromptsLock.Enter();
            _userPrompts.Add(prompt);
            _userPromptsLock.Leave();
            _systemWait.Set();
        }

        protected virtual void AutoUpdateOnAutoUpdateChange(AutoUpdateEventArgs args)
        {
            try
            {
                switch (args.EventId)
                {
                    case AutoUpdateEventIds.UpdateIsAvailable:
                        Debug.WriteSuccess("AutoUpdate Available", "Showing postpont prompt on all UIs");
                        _auPrompt = PromptUsers(prompt =>
                        {
                            if (prompt.Response.Responded)
                            {
                                AutoUpdate.PerformUpdateInResponseToEvent(prompt.Response.Action.ActionType ==
                                                                          PromptActionType.Acknowledge);
                            }
                        }, "System Update", "The system is about to commence an auto system update", 30, null,
                            new PromptAction
                            {
                                ActionName = "Update Now",
                                ActionType = PromptActionType.Acknowledge
                            },
                            new PromptAction
                            {
                                ActionName = "Postpone",
                                ActionType = PromptActionType.Cancel
                            });
                        break;
                    case AutoUpdateEventIds.UpdateConfirmed:
                        Debug.WriteSuccess("AutoUpdate allowed by user");
                        _auPrompt = PromptUsers(prompt => { }, "System Update In Progress", "The system may restart",
                            500, null);
                        break;
                    case AutoUpdateEventIds.UpdateConfirmedViaTimeout:
                        Debug.WriteWarn("AutoUpdate prompt timed out .... continuing with update");
                        if (_auPrompt != null)
                            _auPrompt.Cancel();
                        break;
                    case AutoUpdateEventIds.UpdateDenied:
                        Debug.WriteWarn("AutoUpdate denied by user");
                        break;
                    case AutoUpdateEventIds.UpdateStartedWithNoConfirmation:
                        Debug.WriteWarn("AutoUpdate Started With No Confirmation!");
                        _auPrompt = PromptUsers(prompt => { }, "System Update In Progress", "The system may restart",
                            500, null);
                        break;
                    case AutoUpdateEventIds.ErrorMessage:
                        if (AutoUpdate.LastErrorReceived.Length > 0)
                            CloudLog.Error("AutoUpdate error: {0}", AutoUpdate.LastErrorReceived);
                        break;
                    case AutoUpdateEventIds.UpdateFinished:
                        Debug.WriteSuccess("AutoUpdate Complete");
                        if (_auPrompt != null && _auPrompt.State == PromptState.Shown)
                            _auPrompt.Cancel();
                        break;
                    default:
                        Debug.WriteNormal("AutoUpdate State", "{0}  ({1})", AutoUpdate.AutoUpdateState, args.EventId);
                        break;
                }
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }
        }

        private bool CheckIfNewVersion(Assembly appAssembly)
        {
            try
            {
                var runningVersion = appAssembly.GetName().Version;

                CloudLog.Info("Checking version of {0} to see if \"{1}\" is new", appAssembly.GetName().Name,
                    runningVersion);

                var filePath = string.Format("{0}\\{1}_version.info", AppStoragePath, appAssembly.GetName().Name);

                if (!File.Exists(filePath))
                {
                    using (var newFile = File.OpenWrite(filePath))
                    {
                        newFile.Write(runningVersion.ToString(), Encoding.UTF8);
                    }
                    CloudLog.Notice("Version file created at \"{0}\", app must be updated or new", filePath);
                    return true;
                }

                bool appIsNewVersion;

                using (var file = new StreamReader(filePath, Encoding.UTF8))
                {
                    var contents = file.ReadToEnd().Trim();
                    var version = new Version(contents);
                    appIsNewVersion = runningVersion.CompareTo(version) != 0;
                    if (appIsNewVersion)
                    {
                        CloudLog.Warn("APP UPDATED TO {0}", runningVersion.ToString());
                    }
                    else
                    {
                        CloudLog.Notice("App version remains as {0}", runningVersion.ToString());                        
                    }
                }

                if (appIsNewVersion)
                {
                    File.Delete(filePath);

                    using (var newFile = File.OpenWrite(filePath))
                    {
                        newFile.Write(runningVersion.ToString(), Encoding.UTF8);
                    }
                    CloudLog.Notice("Version file deleted and created at \"{0}\", with new version number", filePath);
                }

                return appIsNewVersion;
            }
            catch (Exception e)
            {
                CloudLog.Error("Error checking if app is new version, returning true, {0}", e.Message);
                return true;
            }
        }

        #endregion
    }

    public class SystemStartupProgressEventArgs : EventArgs
    {
        private readonly string _message;
        private readonly ushort _progressLevel;
        private readonly bool _isComplete;

        internal SystemStartupProgressEventArgs(string message, ushort progressLevel, bool isComplete)
        {
            _message = message;
            _progressLevel = progressLevel;
            _isComplete = isComplete;
        }

        public string Message
        {
            get { return _message; }
        }

        public ushort ProgressLevel
        {
            get { return _progressLevel; }
        }

        public bool IsComplete
        {
            get { return _isComplete; }
        }
    }

    public delegate void SystemStartupProgressEventHandler(SystemBase system, SystemStartupProgressEventArgs args);

    public delegate void SystemClockTimeChangeEventHandler(SystemBase system, DateTime time);
}