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
using UX.Lib2.Cloud.Logger;
using UX.Lib2.DeviceSupport;
using UX.Lib2.DeviceSupport.Relays;

namespace UX.Lib2.Models
{
    public abstract class DisplayBase
    {
        #region Fields

        private static uint _idCount;
        private readonly DisplayDeviceBase _displayDevice;
        private SourceBase _source;
        private SourceBase _lastSource;
        private string _name;
        private bool _enabled = true;
        private IHoistController _hoistController;
        private IHoistController _screenController;

        #endregion

        #region Constructors

        protected DisplayBase(SystemBase system, DisplayDeviceBase displayDevice)
        {
            _idCount ++;
            Id = _idCount;
            _displayDevice = displayDevice;
            if (_displayDevice != null)
            {
                _displayDevice.PowerStatusChange += DisplayDeviceOnPowerStatusChange;
                _displayDevice.DeviceCommunicatingChange += DisplayDeviceOnDeviceCommunicatingChange;
            }
            System = system;
            System.Displays.Add(this);
        }

        protected DisplayBase(RoomBase room, DisplayDeviceBase displayDevice)
            : this(room.System, displayDevice)
        {
            AssignToRoom(room);
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events
        #endregion

        #region Delegates
        #endregion

        #region Properties

        public uint Id { get; private set; }

        public SystemBase System { get; set; }

        public SourceCollection Sources
        {
            get { return System.Sources.ForDisplay(this); }
        }

        public virtual RoomBase Room { get; private set; }

        public string Name
        {
            get
            {
                if (!string.IsNullOrEmpty(_name))
                    return _name;
                if (Device != null && !string.IsNullOrEmpty(Device.Name))
                    return Device.Name;
                return ToString();
            }
            set { _name = value; }
        }

        public DisplayDeviceBase Device
        {
            get { return _displayDevice; }
        }

        public SourceBase Source
        {
            set
            {
                _source = value;

                if (_source != null)
                {
                    _lastSource = value;
                }

                if (!Enabled) return;

                try
                {
                    OnSourceChange(_source);
                }
                catch (Exception e)
                {
                    CloudLog.Exception(e);
                }
            }
            get { return _source; }
        }

        public SourceBase LastSource
        {
            get { return _lastSource; }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if(_enabled == value) return;
                
                _enabled = value;

                if (_enabled)
                {
                    OnSourceChange(_source);
                }
                else
                {
                    OnSourceChange(null);
                    if (Device != null)
                    {
                        Device.Power = false;
                    }
                }
            }
        }

        public IHoistController ScreenController
        {
            get { return _screenController; }
        }

        public IHoistController HoistController
        {
            get { return _hoistController; }
        }

        #endregion

        #region Methods

        protected virtual void OnSourceChange(SourceBase source)
        {
            CloudLog.Debug("{0} set to Source: {1}", this, source != null ? source.ToString() : "None");

            if (source == null || _displayDevice == null) return;
            if (!_displayDevice.Power)
            {
                CloudLog.Debug("{0} Power set to On!", this);
                _displayDevice.Power = true;
            }
            try
            {
                if (Room != null && Room.GetDisplayInputOverrideForSource(this, source) != DisplayDeviceInput.Unknown)
                {
                    RouteSourceDisplayDeviceInput(Room.GetDisplayInputOverrideForSource(this, source));
                }
                else if (source.DisplayDeviceInput != DisplayDeviceInput.Unknown)
                {
                    RouteSourceDisplayDeviceInput(source.DisplayDeviceInput);
                }
            }
            catch (Exception e)
            {
                CloudLog.Exception(e, "Error setting input for display");
            }
        }

        protected virtual void RouteSourceDisplayDeviceInput(DisplayDeviceInput input)
        {
            try
            {
                _displayDevice.SetInput(input);
            }
            catch (Exception e)
            {
                CloudLog.Exception(e, "Error setting display device input");
            }
        }

        public void AssignToRoom(RoomBase room)
        {
            Room = room;
        }

        internal virtual void Initialize()
        {
            if (_displayDevice != null)
            {
                Debug.WriteInfo("Initializing Display", "{0} - {1}", _displayDevice.GetType().Name,
                    _displayDevice.DeviceAddressString);
                _displayDevice.Initialize();
            }
        }

        protected virtual void DisplayDeviceOnPowerStatusChange(IPowerDevice device, DevicePowerStatusEventArgs args)
        {
            switch (args.NewPowerStatus)
            {
                case DevicePowerStatus.PowerWarming:
                case DevicePowerStatus.PowerOn:
                    if(args.PreviousPowerStatus == DevicePowerStatus.PowerWarming) return;
                    if (HoistController != null)
                    {
                        CloudLog.Info("{0} power now {1}, Lowering Hoist", Name, args.NewPowerStatus);
                        HoistController.Down();
                    }
                    if (ScreenController != null)
                    {
                        CloudLog.Info("{0} power now {1}, Lowering Screen", Name, args.NewPowerStatus);
                        ScreenController.Down();
                    }
                    break;
                case DevicePowerStatus.PowerOff:
                    if (ScreenController != null && args.PreviousPowerStatus == DevicePowerStatus.PowerOn)
                    {
                        CloudLog.Info("{0} power now {1}, Raising Screen", Name, args.NewPowerStatus);
                        ScreenController.Up();
                    } 
                    if (HoistController != null)
                    {
                        CloudLog.Info("{0} power now {1}, Raising Hoist", Name, args.NewPowerStatus);
                        HoistController.Up();
                    }
                    break;
                case DevicePowerStatus.PowerCooling:
                    if (ScreenController != null)
                    {
                        CloudLog.Info("{0} power now {1}, Raising Screen", Name, args.NewPowerStatus);
                        ScreenController.Up();
                    }
                    break;
            }
        }

        protected virtual void DisplayDeviceOnDeviceCommunicatingChange(IDevice device, bool communicating)
        {

        }

        public void SetHoistController(IHoistController controller)
        {
            _hoistController = controller;
        }

        public void SetScreenController(IHoistController controller)
        {
            _screenController = controller;
        }

        #endregion
    }
}