using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Crestron.SimplSharp.Reflection;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.UI;
using UX.Lib2.Cloud.Logger;
using UX.Lib2.DeviceSupport;
using UX.Lib2.Models;

namespace UX.Lib2.UI
{
    public abstract class UIController : IFusionAsset
    {
        #region Fields

        private static uint _idCount;
        private readonly SystemBase _system;
        private RoomBase _room;
        private int _deviceCount;
        private UIPageCollection _pages;
        private readonly ButtonCollection _hardButtons;
        private readonly bool _isEthernetDevice;
        private string _deviceIpAddress;
        private readonly List<RoomBase> _allowedRooms = new List<RoomBase>(); 

        #endregion

        #region Constructors

        /// <summary>
        /// Create a UIController instance
        /// </summary>
        /// <param name="system">The base system</param>
        /// <param name="device">The UI device used for the UIController</param>
        /// <param name="defaultRoom">The default room for the UI</param>
        protected UIController(SystemBase system, BasicTriList device, RoomBase defaultRoom)
        {
            _idCount ++;
            Id = _idCount;
            _system = system;
            _system.SystemStartupProgressChange += OnSystemStartupProgressChange;

            Device = device;
            DefaultRoom = defaultRoom;

            CloudLog.Debug("Creating {0} for device: {1} ({2}) with ID 0x{3:X2}", GetType().Name, device.GetType().Name, device.Name, device.ID);

            try
            {
                device.IpInformationChange += DeviceOnIpInformationChange;
                _isEthernetDevice = true;
            }
            catch
            {
                CloudLog.Debug("{0} is not Ethernet Device", device.ToString());
            }
            device.OnlineStatusChange += DeviceOnOnlineStatusChange;
            device.SigChange += OnSigChange;

            var tswFt5Button = device as TswFt5Button;

            if (tswFt5Button != null)
            {
                tswFt5Button.ExtenderEthernetReservedSigs.Use();
                tswFt5Button.ExtenderEthernetReservedSigs.DeviceExtenderSigChange +=
                    ExtenderEthernetReservedSigsOnDeviceExtenderSigChange;
            }

            var tswx52ButtonVoiceControl = device as Tswx52ButtonVoiceControl;
            if (tswx52ButtonVoiceControl != null)
            {
                tswx52ButtonVoiceControl.ExtenderSystemReservedSigs.Use();
                tswx52ButtonVoiceControl.ExtenderAutoUpdateReservedSigs.Use();
                tswx52ButtonVoiceControl.ExtenderAutoUpdateReservedSigs.DeviceExtenderSigChange += ExtenderAutoUpdateReservedSigsOnDeviceExtenderSigChange;
            }

            var tswX60BaseClass = device as TswX60BaseClass;
            if (tswX60BaseClass != null)
            {
                tswX60BaseClass.ExtenderHardButtonReservedSigs.Use();
                Debug.WriteInfo("Setting up hard buttons for TswX60BaseClass device");
                _hardButtons = new ButtonCollection();
                for (uint b = 1; b <= 5; b ++)
                {
                    var extender = tswX60BaseClass.ExtenderHardButtonReservedSigs;
                    var t = extender.GetType().GetCType();
                    var offMethod = t.GetMethod(string.Format("TurnButton{0}BackLightOff", b));
                    var onMethod = t.GetMethod(string.Format("TurnButton{0}BackLightOn", b));
                    var delType = typeof (HardKeyBackLightMethod).GetCType();
                    var offDel = (HardKeyBackLightMethod) CDelegate.CreateDelegate(delType, extender, offMethod);
                    var onDel = (HardKeyBackLightMethod) CDelegate.CreateDelegate(delType, extender, onMethod);
                    var button = new UIHardButton(this, b, onDel, offDel);
                    _hardButtons.Add(b, button);
                }
            } 
            else if (device is Tswx52ButtonVoiceControl)
            {
                Debug.WriteInfo("Setting up hard buttons for Tswx52ButtonVoiceControl device");
                _hardButtons = new ButtonCollection();
                for (uint b = 1; b <= 5; b++)
                {
                    var button = new UIHardButton(this, b);
                    _hardButtons.Add(b, button);
                }
            }

            _system.UIControllers.Add(this);
        }

        private void ExtenderAutoUpdateReservedSigsOnDeviceExtenderSigChange(DeviceExtender currentDeviceExtender, SigEventArgs args)
        {
            if (args.Sig == ((AutoUpdateReservedSigs) currentDeviceExtender).InProgressFeedback)
            {
                Debug.WriteInfo("Tsw Panel AutoUpdate", "In Progress = {0}",
                    ((AutoUpdateReservedSigs) currentDeviceExtender).InProgressFeedback.BoolValue);
            }
            else if (args.Sig == ((AutoUpdateReservedSigs) currentDeviceExtender).ManifestUrlFeedback)
            {
                Debug.WriteInfo("Tsw Panel AutoUpdate", "Manifest URL = {0}",
                    ((AutoUpdateReservedSigs)currentDeviceExtender).ManifestUrlFeedback.StringValue);
            }
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events

        public event UIControllerRoomChangeEventHandler RoomChange;

        public event RoomSourceChangeEventHandler SourceChange;

        public event UIControllerActivityEventHandler Activity;

        public event DeviceCommunicatingChangeHandler DeviceCommunicatingChange;

        #endregion

        #region Delegates
        #endregion

        #region Properties

        /// <summary>
        /// The unique ID of the UIController. This is not the same as a device ID / IPID.
        /// </summary>
        public uint Id { get; private set; }

        /// <summary>
        /// The UI Device or Gateway
        /// </summary>
        public virtual BasicTriList Device { get; private set; }

        public bool IsEthernetDevice
        {
            get { return _isEthernetDevice; }
        }

        /// <summary>
        /// Contains all UIPageViewControllers
        /// </summary>
        public UIPageCollection Pages
        {
            get { return _pages ?? (_pages = new UIPageCollection(this)); }
        }

        /// <summary>
        /// Get the current visible page
        /// </summary>
        public UIPageViewController CurrentPage
        {
            get { return _pages.FirstOrDefault(p => p.Visible); }
        }

        /// <summary>
        /// The System for the UIController
        /// </summary>
        public SystemBase System { get { return _system; } }

        /// <summary>
        /// The Room for the UIController
        /// </summary>
        public RoomBase Room
        {
            get { return _room; }
            set
            {
                if (_room == value) return;

                var previousRoom = _room;
                if (previousRoom != null)
                {
                    previousRoom.SourceChange -= OnRoomSourceChange;
                    previousRoom.RoomDetailsChange -= OnRoomDetailsChange;
                }

                _room = value;

                if (_room != null)
                {
                    _room.RoomDetailsChange += OnRoomDetailsChange;
                    _room.SourceChange += OnRoomSourceChange;
                }

                OnRoomChange(this, previousRoom, _room);
            }
        }

        /// <summary>
        /// The default room for the UI
        /// </summary>
        public RoomBase DefaultRoom { get; private set; }

        /// <summary>
        /// Set or get the current Source
        /// </summary>
        public SourceBase Source
        {
            get { return _room != null ? _room.Source : null; }
            set
            {
#if DEBUG
                Debug.WriteInfo("UI.set_Source", "UI {0} to source {1}", Id, value != null ? value.Id : 0);
#endif
                if (_room == null)
                {
                    CloudLog.Error("Cannot set UI Source, Room is null!");
                    return;
                }

                if (_room.Source == value && _room.Source != null)
                {
                    UIShouldShowSource(_room.Source);
                    return;
                }
                _room.Source = value;
            }
        }

        public bool SourceChangeBusy
        {
            get { return _room != null && _room.SourceChangeBusy; }
        }

        /// <summary>
        /// If device supports Hard Keys this will return a collection of buttons. Null if not supported.
        /// </summary>
        public ButtonCollection HardButtons
        {
            get { return _hardButtons; }
        }

        public string Name
        {
            get { return Device.ToString(); }
        }

        public string ManufacturerName
        {
            get { return "Crestron"; }
        }

        public string ModelName
        {
            get { return Device.Name; }
        }

        public string DiagnosticsName
        {
            get
            {
                if (string.IsNullOrEmpty(Device.Description))
                {
                    return Device.Name;
                }
                return Device.Name + " \"" + Device.Description + "\"";
            }
        }

        public bool DeviceCommunicating
        {
            get { return Device.IsOnline; }
        }

        public bool IsXpanel
        {
            get { return Device is XpanelForSmartGraphics || Device is Xpanel; }
        }

        public string DeviceAddressString
        {
            get { return _deviceIpAddress; }
        }

        public string VersionInfo
        {
            get { return "Unknown"; }
        }

        public string SerialNumber
        {
            get
            {
                var result = "Unknown";

                var tswFt5Button = Device as TswFt5Button;

                if (tswFt5Button != null)
                {
                    result = tswFt5Button.ExtenderEthernetReservedSigs.MacAddressFeedback.StringValue;
                }

                return result;
            }
        }

        public FusionAssetType AssetType
        {
            get
            {
                return FusionAssetType.TouchPanel;
            }
        }

        public bool CanChangeRooms
        {
            get { return _allowedRooms.Any(r => r != _room); }
        }

        public ReadOnlyCollection<RoomBase> AllowedRooms
        {
            get { return _allowedRooms.AsReadOnly(); }
        }

        public UIControllerCollection UIControllers
        {
            get { return _system.UIControllers; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Register the device in the UIController
        /// </summary>
        /// <returns></returns>
        public virtual eDeviceRegistrationUnRegistrationResponse Register()
        {
            if (Device.Registered) return eDeviceRegistrationUnRegistrationResponse.Success;
            var response = Device.Register();
            if (response != eDeviceRegistrationUnRegistrationResponse.Success)
                CloudLog.Error("Could not register {0} device with ID 0x{1:x2}, {2}", Device.GetType().Name, Device.ID,
                    response);
            return response;
        }

        private void OnSigChange(GenericBase currentDevice, SigEventArgs args)
        {
#if DEBUG
            Debug.WriteInfo(currentDevice.Name + " OnSigChange()", ", {0}, {1}", args.Event, args.Sig);
#endif
            if(args.Event == eSigEvent.BoolChange)
                OnActivity(this);
        }

        protected void OnActivity(UIController uiController)
        {
            var handler = Activity;
            if (handler != null) handler(uiController);
        }

        private void DeviceOnOnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            if (!args.DeviceOnLine)
            {
                CloudLog.Warn("{0} is offline!", currentDevice.ToString());               
            }

            try
            {
                if (DeviceCommunicatingChange != null)
                {
                    DeviceCommunicatingChange(this, args.DeviceOnLine);
                }
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }
        }
        
        protected virtual void OnDeviceConnect(int deviceCount)
        {
            CloudLog.Debug("{0} online deviceCount = {1}", Device, deviceCount);

            var oldCount = _deviceCount;

            _deviceCount = deviceCount;

            if (oldCount > 0) return;

            if(!System.Booted) return;

            if (Room != null && Room.Source != null)
            {
                UIShouldShowSource(Room.Source);
            }
            else
            {
                UIShouldShowHomePage(ShowHomePageEventType.OnDeviceConnect);
            }
        }

        private void DeviceOnIpInformationChange(GenericBase currentDevice, ConnectedIpEventArgs args)
        {
            if (!args.Connected) return;
            CloudLog.Notice("{0} has connected on IP {1}", currentDevice, args.DeviceIpAddress);
            if (currentDevice.ConnectedIpList.Count == 1)
            {
                _deviceIpAddress = currentDevice.ConnectedIpList.First().DeviceIpAddress;
            }
            OnDeviceConnect(currentDevice.ConnectedIpList.Count);
        }

        protected virtual void OnRoomChange(UIController uiController, RoomBase previousRoom, RoomBase newRoom)
        {
            var handler = RoomChange;
            var args = new UIControllerRoomChangeEventArgs(previousRoom, newRoom);
            CloudLog.Info("{0} has changed rooms from {1} to {2}", uiController,
                previousRoom != null ? previousRoom.ToString() : "None",
                newRoom != null ? newRoom.ToString() : "None");

            try
            {
                if (handler != null) handler(uiController, args);
            }
            catch (Exception e)
            {
                CloudLog.Exception(e, "Error calling RoomChange event in {0}", GetType().Name);
            }

            if(!System.Booted) return;

            try
            {
                if (newRoom != null && newRoom.Source != null)
                    UIShouldShowSource(newRoom.Source);
                else
                    UIShouldShowHomePage(ShowHomePageEventType.OnRoomChange);
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }
        }

        protected virtual void OnRoomDetailsChange(RoomBase room)
        {

        }

        protected virtual void OnRoomSourceChange(RoomBase room, RoomSourceChangeEventArgs args)
        {
            if (room != Room || !System.Booted) return;

            try
            {
                if (SourceChange != null)
                {
                    SourceChange(room, args);
                }
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }

            if(args.NewSource != null)
                UIShouldShowSource(args.NewSource);
            else
                UIShouldShowHomePage(ShowHomePageEventType.OnClearingSource);
        }

        protected abstract void UIShouldShowSource(SourceBase source);

        protected abstract void UIShouldShowHomePage(ShowHomePageEventType eventType);

        internal void InternalRoomPowerOff(PowerOfFEventType eventType)
        {
            try
            {
                RoomDidPowerOff(eventType);
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }
        }

        protected abstract void RoomDidPowerOff(PowerOfFEventType eventType);

        internal void ShowMainView()
        {
            if (!System.Booted) return;

            if (Room != null && Room.Source != null)
                UIShouldShowSource(Room.Source);
            else
                UIShouldShowHomePage(ShowHomePageEventType.OnStartup);
        }

        internal void ShowPrompt(UserPrompt prompt)
        {
            UIShouldShowPrompt(prompt);
        }

        protected abstract void UIShouldShowPrompt(UserPrompt prompt);

        /// <summary>
        /// Connect the UI back to the default room
        /// </summary>
        public void ConnectToDefaultRoom()
        {
            Room = DefaultRoom;
        }

        internal void InternalInitialize()
        {
#if DEBUG
            Debug.WriteInfo(GetType().Name + ".InternalInitialize()");
#endif
            Initialize();
        }

        protected abstract void Initialize();

        protected abstract void OnSystemStartupProgressChange(SystemBase system, SystemStartupProgressEventArgs args);

        public override string ToString()
        {
            return string.Format("{0} UI 0x{1:X2}", Device.Name, Device.ID);
        }

        internal void InternalSystemWillRestart(bool upgrading)
        {
            SystemWillRestart(upgrading);
        }

        protected virtual void SystemWillRestart(bool upgrading)
        {
            
        }

        public void Sleep()
        {
            var device = Device as Tswx52ButtonVoiceControl;
            if (device == null) return;

            device.ExtenderSystemReservedSigs.BacklightOff();
        }

        public void Wake()
        {
            var device = Device as Tswx52ButtonVoiceControl;
            if(device == null) return;
            
            device.ExtenderSystemReservedSigs.BacklightOn();
        }

        public void AddRoomToAllowedRooms(RoomBase room)
        {
            if (_allowedRooms.Contains(room))
            {
                throw new Exception("Room already added");
            }
            _allowedRooms.Add(room);
        }

        public void RemoveRoomFromAllowedRooms(RoomBase room)
        {
            if (!_allowedRooms.Contains(room))
            {
                throw new Exception("Room already not in allowed rooms");
            }
            _allowedRooms.Remove(room);
        }

        private void ExtenderEthernetReservedSigsOnDeviceExtenderSigChange(DeviceExtender currentDeviceExtender, SigEventArgs args)
        {
            if (args.Event != eSigEvent.StringChange) return;
            CloudLog.Debug("{0} ExtenderEthernetReservedSigs, Sig Change, Number: {1}, Name {2}, Value {3}",
                Device.Name, args.Sig.Number, args.Sig.Name, args.Sig.UShortValue);
        }

        #endregion
    }

    public class UIControllerRoomChangeEventArgs : EventArgs
    {
        internal UIControllerRoomChangeEventArgs(RoomBase previousRoom, RoomBase newRoom)
        {
            NewRoom = newRoom;
            PreviousRoom = previousRoom;
        }

        public RoomBase PreviousRoom { get; private set; }
        public RoomBase NewRoom { get; private set; }
    }

    public delegate void UIControllerRoomChangeEventHandler(
        UIController uiController, UIControllerRoomChangeEventArgs args);

    public delegate void UIControllerActivityEventHandler(UIController uiController);

    public enum ShowHomePageEventType
    {
        NotDefined,
        OnStartup,
        OnClearingSource,
        OnShutdown,
        OnDeviceConnect,
        OnRoomChange,
        OnLock,
        OnUnlock,
    }
}