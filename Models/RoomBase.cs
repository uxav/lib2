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
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronThread;
using Crestron.SimplSharpPro.Fusion;
using UX.Lib2.Cloud.Logger;
using UX.Lib2.DeviceSupport;
using UX.Lib2.UI;

namespace UX.Lib2.Models
{
    public abstract class RoomBase
    {
        #region Fields

        private static uint _idCount;
        private readonly SystemBase _system;
        private string _name;
        private SourceBase _source;
        private readonly List<RoomBase> _connectedRooms = new List<RoomBase>();
        private string _roomContactNumber;
        private readonly Dictionary<uint, IFusionAsset> _fusionAssets = new Dictionary<uint, IFusionAsset>();

        private readonly Dictionary<FusionStaticAsset, FusionAssetCommsStatus> _assetCommsStatus =
            new Dictionary<FusionStaticAsset, FusionAssetCommsStatus>();
 
        private Thread _sourceChangeThread;
        private bool _sourceChangeBusy;
        private bool _programStopping;
        private CTimer _assetCommsCheckTimer;
        private string _nameFull;
        private bool _roomOccupied;

        #endregion

        #region Constructors

        /// <summary>
        /// The default Constructor.
        /// </summary>
        protected RoomBase(SystemBase system)
        {
            _idCount ++;
            Id = _idCount;
            _system = system;
            _system.Rooms.Add(this);
            CrestronEnvironment.ProgramStatusEventHandler += type =>
            {
                _programStopping = type == eProgramStatusEventType.Stopping;
            };
            _assetCommsCheckTimer = new CTimer(AssetCommsAlertTimerProcess, null, 60000, 1000);
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events

        public event RoomSourceChangeEventHandler SourceChange;

        public event RoomDetailsChangeEventHandler RoomDetailsChange;

        public event RoomOccupiedChangeEventHandler RoomOccupiedChange;

        #endregion

        #region Delegates
        #endregion

        #region Properties

        /// <summary>
        /// The System which owns the room
        /// </summary>
        public SystemBase System
        {
            get { return _system; }
        }

        /// <summary>
        /// The sources for this room including all global sources
        /// </summary>
        public SourceCollection Sources
        {
            get { return System.Sources.ForRoomOrGlobal(this); }
        }

        public virtual IEnumerable<SourceBase> PresentationSources
        {
            get
            {
                return
                    new SourceCollection(
                        System.Sources.ForRoomOrGlobal(this).PresentationSources);
            }
        }

        /// <summary>
        /// Set or get the current source for the system.
        /// </summary>
        public virtual SourceBase Source
        {
            get { return _source; }
            set
            {
                if(_source == value && _source != null) return;
                
                var previousSource = _source;

                _source = value;

                if (previousSource != null)
                {
                    previousSource.RoomCount--;
                }

                if (_source != null)
                {
                    _source.RoomCount++;
                }

                OnSourceChange(this, previousSource, _source);
            }
        }

        public bool SourceChangeBusy
        {
            get
            {
                return _sourceChangeThread != null &&
                       _sourceChangeThread.ThreadState == Thread.eThreadStates.ThreadRunning;
            }
        }

        public virtual DisplayCollection Displays
        {
            get { return new DisplayCollection(System.Displays.Where(d => d.Room == this)); }
        }

        /// <summary>
        /// The ID of the room which is autogenerated and unique.
        /// </summary>
        public uint Id { get; internal set; }

        /// <summary>
        /// Set or Get the Room Name
        /// </summary>
        public string Name
        {
            get { return String.IsNullOrEmpty(_name) ? string.Format("Room {0}", Id) : _name; }
            set
            {
                if(_name == value) return;
                
                _name = value;
                
                OnRoomDetailsChange(this);
            }
        }

        public string Location { get; protected set; }

        public string NameFull
        {
            get
            {
                if (string.IsNullOrEmpty(_nameFull) && string.IsNullOrEmpty(Location))
                {
                    return Name;
                }
                if (string.IsNullOrEmpty(_nameFull))
                {
                    return Location + " " + Name;
                }
                return _nameFull;
            }
            protected set { _nameFull = value; }
        }

        /// <summary>
        /// Set or get the room telephone / contact number
        /// </summary>
        public virtual string RoomContactNumber
        {
            get { return _roomContactNumber; }
            set
            {
                if(_roomContactNumber == value) return;

                _roomContactNumber = value;
                
                OnRoomDetailsChange(this);
            }
        }

        /// <summary>
        /// A user prompt is currently showing system wide
        /// </summary>
        public bool UserPromptShowing
        {
            get { return System.UserPrompts.Any(p => p.State == PromptState.Shown && p.Room == this); }
        }

        /// <summary>
        /// The type of divisible room
        /// </summary>
        public DivisibleRoomType DivisibleRoomType { get; protected set; }

        /// <summary>
        /// Connected slave rooms currently joined to this room
        /// </summary>
        public ReadOnlyCollection<RoomBase> ConnectedSlaveRooms
        {
            get
            {
                return new ReadOnlyCollection<RoomBase>(_connectedRooms);
            }
        }

        public ReadOnlyCollection<RoomBase> OtherRooms
        {
            get
            {
                return new ReadOnlyCollection<RoomBase>(System.Rooms
                    .Where(r => r != this)
                    .OrderBy(r => r.Id)
                    .ToList());
            }
        }

        public ReadOnlyCollection<RoomBase> DisconnectedSlaveRooms
        {
            get
            {
                return new ReadOnlyCollection<RoomBase>(OtherRooms
                    .Where(r => !_connectedRooms.Contains(r) && r.DivisibleRoomType == DivisibleRoomType.Slave)
                    .Where(r => r.Id > Id)
                    .ToList());
            }
        }

        public RoomBase ConnectedMasterRoom
        {
            get { return OtherRooms.FirstOrDefault(r => r.ConnectedSlaveRooms.Contains(this)); }
        }

        public UIControllerCollection UIControllers
        {
            get { return _system.UIControllers.ForRoom(this); }
        }

        public UIControllerCollection DefaultUIControllers
        {
            get { return _system.UIControllers.ForDefaultRoom(this); }
        }

        protected FusionRoom Fusion { get; private set; }

        public bool FusionEnabled
        {
            get { return FusionShouldRegister() > 0; }
        }

        protected virtual string PowerOffPromptHeaderText
        {
            get { return "Power Off Room?"; }
        }

        protected virtual string PowerOffPromptSubHeaderText
        {
            get { return "System will automatically power off if ignored"; }
        }

        protected virtual string PowerOffPromptConfirmButtonText
        {
            get { return "Power Off Now"; }
        }

        protected virtual bool FusionSystemPowerOnFb
        {
            get { return Source != null; }
        }

        public virtual bool RoomOccupied
        {
            get { return _roomOccupied; }
            protected set
            {
                if(_roomOccupied == value) return;

                _roomOccupied = value;

                OnRoomOccupiedChange(this, _roomOccupied);
            }
        }

        #endregion

        #region Methods

        private void OnSourceChange(RoomBase room, SourceBase previousSource, SourceBase newSource)
        {
            var handler = SourceChange;
            var args = new RoomSourceChangeEventArgs(previousSource, newSource);
            CloudLog.Info("{0} has changed sources from {1} to {2}", room,
                previousSource != null ? previousSource.ToString() : "Off",
                newSource != null ? newSource.ToString() : "Off");

            StartSourceChangeProcess(previousSource, newSource);

            try
            {
                if (handler != null) handler(room, args);
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }
        }

        protected virtual void StartSourceChangeProcess(SourceBase previousSource, SourceBase newSource)
        {
            _sourceChangeThread = new Thread(SourceLoadProcess, new[] {previousSource, newSource},
                Thread.eThreadStartOptions.CreateSuspended)
            {
                Name = "Room SourceLoadProcess()",
                Priority = Thread.eThreadPriority.HighPriority
            };
            _sourceChangeThread.Start();
        }

        protected virtual void OnRoomDetailsChange(RoomBase room)
        {
            var handler = RoomDetailsChange;
            if (handler != null) handler(room);
        }

        private object SourceLoadProcess(object userSpecific)
        {
            var sources = userSpecific as SourceBase[];

            if (sources == null)
            {
                CloudLog.Error("Error in SourceLoadProcess(object userSpecific)");
                return null;
            }

            _sourceChangeBusy = true;

            try
            {
                SourceLoadStarted(sources[1]);
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }

            try
            {
                SourceLoadProcess(sources[0], sources[1]);
            }
            catch (Exception e)
            {
                CloudLog.Exception(e, "Error loading source \"{0}\"", sources[1] != null ? sources[1].Name : "None");
            }

            try
            {
                if (Fusion != null)
                {
                    FusionShouldUpdateCoreParameters();
                }
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }

            _sourceChangeBusy = false;

            try
            {
                SourceLoadEnded();
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }

            return null;
        }

        internal void FusionRegisterInternal()
        {
            var ipId = FusionShouldRegister();
            
            try
            {
                if (ipId > 0)
                {
                    IpIdFactory.Block(ipId, IpIdFactory.DeviceType.Fusion);
                    Fusion = new FusionRoom(ipId, System.ControlSystem, Name, Guid.NewGuid().ToString());
                    Fusion.Description = "Fusion for " + Name;
                    Fusion.FusionStateChange += OnFusionStateChange;
                    Fusion.OnlineStatusChange += OnFusionOnlineStatusChange;
                    Fusion.FusionAssetStateChange += FusionOnFusionAssetStateChange;
                }
                else
                {
                    return;
                }
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }

            try
            {
                FusionShouldRegisterUserSigs();
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }

            try
            {
                FusionShouldRegisterAssets();
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }

            try
            {
                var regResult = Fusion.Register();
                if (regResult != eDeviceRegistrationUnRegistrationResponse.Success)
                {
                    CloudLog.Error("Error registering fusion in room {0} with IpId 0x{1:X2}, result = {2}", Id, ipId,
                        regResult);
                }
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }
        }

        protected abstract uint FusionShouldRegister();

        protected abstract void FusionShouldRegisterUserSigs();

        protected abstract void FusionShouldRegisterAssets();

        protected FusionStaticAsset FusionAddAsset(IFusionAsset asset)
        {
            try
            {
                uint key = 1;

                while (_fusionAssets.ContainsKey(key))
                {
                    key ++;
                }

                Fusion.AddAsset(eAssetType.StaticAsset, key, asset.Name, asset.AssetType.ToString().SplitCamelCase(),
                    Guid.NewGuid().ToString());

                _fusionAssets.Add(key, asset);

                var newAsset = Fusion.UserConfigurableAssetDetails[key].Asset as FusionStaticAsset;

                if (newAsset == null) return null;

                newAsset.ParamMake.Value = asset.ManufacturerName;
                newAsset.ParamModel.Value = asset.ModelName;
                newAsset.Connected.InputSig.BoolValue = asset.DeviceCommunicating;

                newAsset.AddSig(eSigType.String, 1, "Device Address", eSigIoMask.InputSigOnly);
                newAsset.AddSig(eSigType.String, 2, "Serial Number", eSigIoMask.InputSigOnly);
                newAsset.AddSig(eSigType.String, 3, "Version Info", eSigIoMask.InputSigOnly);

                asset.DeviceCommunicatingChange += FusionAssetOnDeviceCommunicatingChange;

                var deviceWithPower = asset as IPowerDevice;

                if (deviceWithPower != null)
                {
                    deviceWithPower.PowerStatusChange += FusionAssetOnPowerStatusChange;
                    newAsset.PowerOn.InputSig.BoolValue = deviceWithPower.Power;
                }
            }
            catch (Exception e)
            {
                CloudLog.Error("Error registering Fusion asset: {0} ({1}), {2}", asset.GetType().Name, asset.AssetType,
                    e.Message);
            }

            return null;
        }

        private void FusionAssetOnDeviceCommunicatingChange(IDevice device, bool communicating)
        {
            if (_programStopping) return;

            foreach (var asset in (from fusionAsset in _fusionAssets
                where device == fusionAsset.Value
                select Fusion.UserConfigurableAssetDetails[fusionAsset.Key].Asset).OfType<FusionStaticAsset>())
            {
                if (!communicating)
                {
                    CloudLog.Warn(
                        "Fusion monitoring status change for asset \"{0}\", Communicating = {1}, adding to queue and will wait 150 seconds before notifying Fusion",
                        device.Name, communicating);
                }
                else
                {
                    CloudLog.Info(
                           "Fusion monitoring status change for asset \"{0}\", Communicating = {1}, adding to queue and will wait 150 seconds before notifying Fusion",
                           device.Name, communicating);
                }

                _assetCommsStatus[asset] = new FusionAssetCommsStatus(communicating, DateTime.Now,
                    GetTimeDelayForDeviceTimeout(device));

                if (!(device is IPowerDevice))
                {
                    asset.PowerOn.InputSig.BoolValue = communicating;
                }

                if (!communicating)
                {
                    SendFusionErrorMessage(FusionErrorLevel.Error, "Asset: \"{0}\" is offline", device.Name);
                }

                else
                {
                    SendFusionErrorMessage(FusionErrorLevel.Ok, "Asset: \"{0}\" is online", device.Name);
                    asset.FusionGenericAssetSerialsAsset3.StringInput[50].StringValue = device.DeviceAddressString;
                    asset.FusionGenericAssetSerialsAsset3.StringInput[51].StringValue = device.SerialNumber;
                    asset.FusionGenericAssetSerialsAsset3.StringInput[52].StringValue = device.VersionInfo;
                }
            }

            CheckFusionAssetsForOnlineStatusAndReportErrors();
        }

        protected virtual TimeSpan GetTimeDelayForDeviceTimeout(IDevice device)
        {
            return TimeSpan.FromMinutes(1);
        }

        private void AssetCommsAlertTimerProcess(object userSpecific)
        {
            var itemsToRemove = new List<FusionStaticAsset>();

            foreach (var kvp in _assetCommsStatus)
            {
                var asset = kvp.Key;
                var info = kvp.Value;

                if (!info.ShouldRaiseAlert) continue;

                if (info.Communicating)
                {
                    CloudLog.Info("Notifying Fusion asset \"{0}\" is back online", asset.ParamAssetName);
                }
                else
                {
                    CloudLog.Warn("Notifying Fusion asset \"{0}\" is now offline", asset.ParamAssetName);
                }

                itemsToRemove.Add(asset);
                asset.Connected.InputSig.BoolValue = info.Communicating;
                asset.AssetError.InputSig.StringValue = !info.Communicating ? "3:Device is offline" : "0:Device is online";
            }

            foreach (var assetKey in itemsToRemove)
            {
                _assetCommsStatus.Remove(assetKey);
            }
        }

        internal void CheckFusionAssetsForOnlineStatusAndReportErrors()
        {
            if(_fusionAssets == null) return;

            switch (_fusionAssets.Count(c => !c.Value.DeviceCommunicating))
            {
                case 0:
                    SendFusionErrorMessage(FusionErrorLevel.Ok, "All devices are online");
                    break;
                case 1:
                    var offlineDevice = _fusionAssets.First(a => !a.Value.DeviceCommunicating).Value;
                    SendFusionErrorMessage(FusionErrorLevel.Error, "Asset: \"{0}\" is offline", offlineDevice.Name);
                    break;
                default:
                    SendFusionErrorMessage(FusionErrorLevel.FatalError,
                        "Multiple devices are offline. Please check http://{0}:8080/dashboard/diagnostics for more details.",
                        System.IpAddress);
                    break;
            }
        }

        private void FusionAssetOnPowerStatusChange(IPowerDevice device, DevicePowerStatusEventArgs args)
        {
            foreach (var asset in (from fusionAsset in _fusionAssets
                                   where device == fusionAsset.Value
                                   select Fusion.UserConfigurableAssetDetails[fusionAsset.Key].Asset).OfType<FusionStaticAsset>())
            {
                asset.PowerOn.InputSig.BoolValue = device.Power;
                return;
            }
        }

        protected virtual void OnFusionOnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            if(!args.DeviceOnLine) return;

            FusionShouldUpdateCoreParameters();

            foreach (var asset in _fusionAssets)
            {
                var staticAsset = Fusion.UserConfigurableAssetDetails[asset.Key].Asset as FusionStaticAsset;

                if(staticAsset == null) continue;
                
                var device = asset.Value;

                staticAsset.FusionGenericAssetSerialsAsset3.StringInput[50].StringValue = device.DeviceAddressString;
                staticAsset.FusionGenericAssetSerialsAsset3.StringInput[51].StringValue = device.SerialNumber;
                staticAsset.FusionGenericAssetSerialsAsset3.StringInput[52].StringValue = device.VersionInfo;
            }
        }

        protected virtual void FusionShouldUpdateCoreParameters()
        {
            if(Fusion == null) return;
            Fusion.SystemPowerOn.InputSig.BoolValue = FusionSystemPowerOnFb;
            Fusion.DisplayPowerOn.InputSig.BoolValue = Displays.Any(d => d.Device != null && d.Device.Power);
        }

        protected virtual void FusionRequestedPowerOff()
        {
            PowerOff(true, PowerOfFEventType.Fusion);
        }

        protected virtual void FusionRequestedPowerOn()
        {
            
        }

        protected virtual void FusionRequestedDisplaysOff()
        {
            foreach (var display in Displays.Where(d => d.Device != null))
            {
                display.Device.Power = false;
            }
        }

        protected virtual void FusionRequestedDisplaysOn()
        {
            foreach (var display in Displays.Where(d => d.Device != null))
            {
                display.Device.Power = true;
            }
        }

        protected virtual void OnRoomOccupiedChange(RoomBase room, bool occupied)
        {
            var handler = RoomOccupiedChange;
            if (handler == null) return;
            try
            {
                handler(room, occupied);
            }
            catch (Exception e)
            {
                CloudLog.Exception(e, "Error raising event");
            }
        }

        public abstract void Start();

        private void OnFusionStateChange(FusionBase device, FusionStateEventArgs args)
        {
            var fusion = device as FusionRoom;

            if(fusion == null) return;

            switch (args.EventId)
            {
                case FusionEventIds.SystemPowerOffReceivedEventId:
                    if (fusion.SystemPowerOff.OutputSig.BoolValue)
                    {
                        FusionRequestedPowerOff();
                    }
                    break;
                case FusionEventIds.SystemPowerOnReceivedEventId:
                    if (fusion.SystemPowerOn.OutputSig.BoolValue)
                    {
                        FusionRequestedPowerOn();
                    }
                    break;
                case FusionEventIds.DisplayPowerOffReceivedEventId:
                    if (fusion.DisplayPowerOff.OutputSig.BoolValue)
                    {
                        FusionRequestedDisplaysOff();
                    }
                    break;
                case FusionEventIds.DisplayPowerOnReceivedEventId:
                    if (fusion.DisplayPowerOn.OutputSig.BoolValue)
                    {
                        FusionRequestedDisplaysOn();
                    }
                    break;
            }
        }

        private void FusionOnFusionAssetStateChange(FusionBase device, FusionAssetStateEventArgs args)
        {
            if (!_fusionAssets.ContainsKey(args.UserConfigurableAssetDetailIndex)) return;

            var staticAsset =
                Fusion.UserConfigurableAssetDetails[args.UserConfigurableAssetDetailIndex].Asset as FusionStaticAsset;
           
            var asset = _fusionAssets[args.UserConfigurableAssetDetailIndex];
            var powerDevice = asset as IPowerDevice;

            if(powerDevice == null || staticAsset == null) return;

            switch (args.EventId)
            {
                case FusionAssetEventId.StaticAssetPowerOnReceivedEventId:
                    if (staticAsset.PowerOn.OutputSig.BoolValue)
                    {
                        powerDevice.Power = true;
                    }
                    break;
                case FusionAssetEventId.StaticAssetPowerOffReceivedEventId:
                    if (staticAsset.PowerOff.OutputSig.BoolValue)
                    {
                        powerDevice.Power = false;
                    }
                    break;
            }

        }

        protected virtual void FusionSetFreeBusyStatus(bool free, DateTime until)
        {
            if (Fusion == null) return;

            Fusion.FreeBusyStatusToRoom.InputSig.StringValue = free
                ? until.ToUniversalTime().ToString("s")
                : "-";
        }

        public abstract IEnumerable<IDevice> GetRoomDevices(); 

        protected abstract void SourceLoadStarted(SourceBase newSource);

        protected abstract void SourceLoadProcess(SourceBase previousSource, SourceBase newSource);

        protected abstract void SourceLoadEnded();

        internal void InternalPowerOff(PowerOfFEventType eventType)
        {
            PowerOff(eventType);

            foreach (var uiController in UIControllers)
            {
                uiController.InternalRoomPowerOff(eventType);
            }
        }

        protected abstract void PowerOff(PowerOfFEventType eventType);

        protected virtual void SendFusionErrorMessage(FusionErrorLevel level, string message, params object[] args)
        {
            if (Fusion == null) return;
            var messageDetails = string.Format(message, args);
            Fusion.ErrorMessage.InputSig.StringValue = string.Format("{0}:{1}", (int) level, messageDetails);
        }

        public virtual void PowerOff(bool askToConfirm, PowerOfFEventType eventType)
        {
            if (askToConfirm)
            {
                PromptUsers(prompt =>
                {
                    if (prompt.Response.Responded && prompt.Response.Action.ActionType == PromptActionType.Cancel)
                    {
                        //Ignore
                    }
                    else
                    {
                        InternalPowerOff((PowerOfFEventType) prompt.UserDefinedObject);
                    }
                }, PowerOffPromptHeaderText, PowerOffPromptSubHeaderText, 60, eventType,
                    new PromptAction
                    {
                        ActionName = "Cancel",
                        ActionType = PromptActionType.Cancel
                    }, new PromptAction
                    {
                        ActionName = PowerOffPromptConfirmButtonText,
                        ActionType = PromptActionType.Acknowledge
                    });
            }
            else
            {
                InternalPowerOff(eventType);
            }
        }

        protected void PowerOff(bool askToConfirm, PowerOfFEventType eventType, uint customSubPageJoinForPrompt)
        {
            if (askToConfirm)
            {
                PromptUsers(prompt =>
                {
                    if (prompt.Response.Responded && prompt.Response.Action.ActionType == PromptActionType.Cancel)
                    {
                        //Ignore
                    }
                    else
                    {
                        InternalPowerOff((PowerOfFEventType) prompt.UserDefinedObject);
                    }
                }, customSubPageJoinForPrompt, PowerOffPromptHeaderText, PowerOffPromptSubHeaderText, 60,
                    eventType,
                    new PromptAction
                    {
                        ActionName = "Cancel",
                        ActionType = PromptActionType.Cancel
                    }, new PromptAction
                    {
                        ActionName = PowerOffPromptConfirmButtonText,
                        ActionType = PromptActionType.Acknowledge
                    });
            }
            else
            {
                InternalPowerOff(eventType);
            }
        }

        /// <summary>
        /// Prompt all user interfaces for this room
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
            return System.PromptUsers(this, responseCallBack, customSubPageJoin, title, subTitle, timeOutInSeconds, userDefinedObject,
                promptActions);
        }

        public UserPrompt PromptUsers(PromptUsersResponse responseCallBack, string title, string subTitle,
            uint timeOutInSeconds, object userDefinedObject, params PromptAction[] promptActions)
        {
            return PromptUsers(responseCallBack, 0, title, subTitle, timeOutInSeconds, userDefinedObject,
                promptActions);
        }

        public void ConnectSlaveRoom(RoomBase room)
        {
            if (DivisibleRoomType != DivisibleRoomType.Master && room.DivisibleRoomType != DivisibleRoomType.Slave)
                throw new Exception("Cannot connect these room types");

            if (_connectedRooms.Contains(room)) return;

            _connectedRooms.Add(room);
#if DEBUG
            Debug.WriteWarn(string.Format("Room ID {0} connected to Room ID {1}", room.Id, Id));
#endif
            OnDivisibleStateChanged(room, DivisibleStateChangeType.Connected);
        }

        public void DisconnectSlaveRoom(RoomBase room)
        {
            if (_connectedRooms.Contains(room))
            {
                _connectedRooms.Remove(room);
#if DEBUG
                Debug.WriteWarn(string.Format("Room ID {0} disconnected from Room ID {1}", room.Id, Id));
#endif
                OnDivisibleStateChanged(room, DivisibleStateChangeType.Disconnected);
            }
        }

        protected abstract void OnDivisibleStateChanged(RoomBase room, DivisibleStateChangeType changeType);

        public virtual void Initialize()
        {
            
        }

        public override string ToString()
        {
            return string.Format("{0} Id: {1}, \"{2}\"", GetType().Name, Id, Name);
        }

        #endregion
    }

    public delegate void RoomDetailsChangeEventHandler(RoomBase room);

    public class RoomSourceChangeEventArgs : EventArgs
    {
        public RoomSourceChangeEventArgs(SourceBase previousSource, SourceBase newSource)
        {
            NewSource = newSource;
            PreviousSource = previousSource;
        }

        public SourceBase PreviousSource { get; private set; }
        public SourceBase NewSource { get; private set; }
    }

    public delegate void RoomSourceChangeEventHandler(RoomBase room, RoomSourceChangeEventArgs args);

    public delegate void RoomOccupiedChangeEventHandler(RoomBase room, bool occupied);

    public enum DivisibleRoomType
    {
        NotDivisible,
        Master,
        Slave
    }

    public enum DivisibleStateChangeType
    {
        Connected,
        Disconnected
    }

    public enum FusionErrorLevel
    {
        Ok,
        Notice,
        Warning,
        Error,
        FatalError
    }

    public enum PowerOfFEventType
    {
        UserRequest,
        Fusion,
        SourceTimedOut
    }

    internal class FusionAssetCommsStatus
    {
        internal FusionAssetCommsStatus(bool communicating, DateTime dateTime)
            : this(communicating, dateTime, TimeSpan.FromMinutes(1))
        {
            
        }

        internal FusionAssetCommsStatus(bool communicating, DateTime dateTime, TimeSpan delayUntilOfflineAlert)
        {
            Communicating = communicating;
            TimeLastChanged = dateTime;
            TimeToRaiseAlert = delayUntilOfflineAlert;
        }

        public bool Communicating { get; set; }
        public DateTime TimeLastChanged { get; private set; }
        public TimeSpan TimeToRaiseAlert { get; private set; }

        public bool ShouldRaiseAlert
        {
            get
            {
                var timeSinceLastChange = DateTime.Now - TimeLastChanged;
                if (Communicating && timeSinceLastChange >= TimeSpan.FromSeconds(20)) return true;
                return timeSinceLastChange >= TimeToRaiseAlert;
            }
        }
    }
}