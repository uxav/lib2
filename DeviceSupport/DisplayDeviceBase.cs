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
using UX.Lib2.Cloud.Logger;

namespace UX.Lib2.DeviceSupport
{
    public abstract class DisplayDeviceBase : IPowerDevice, IFusionAsset
    {
        #region Fields

        private DevicePowerStatus _powerStatus;
        private readonly string _name;
        private ushort _displayUsage;
        private bool _deviceCommunicating;

        #endregion

        #region Constructors

        /// <summary>
        /// The default Constructor.
        /// </summary>
        protected DisplayDeviceBase(string name)
        {
            _name = name;
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events

        /// <summary>
        /// Power status has changed
        /// </summary>
        public event DevicePowerStatusEventHandler PowerStatusChange;

        /// <summary>
        /// DisplayUsage value has changed
        /// </summary>
        public event DisplayUsageChangeEventHandler DisplayUsageChange;

        public event DeviceCommunicatingChangeHandler DeviceCommunicatingChange;

        #endregion

        #region Delegates
        #endregion

        #region Properties

        public string Name
        {
            get { return _name; }
        }

        public abstract string ManufacturerName { get; }
        public abstract string ModelName { get; }

        public string DiagnosticsName
        {
            get { return Name + " (" + DeviceAddressString + ")"; }
        }

        public bool DeviceCommunicating
        {
            get { return _deviceCommunicating; }
            protected set
            {
                if (_deviceCommunicating == value) return;
                _deviceCommunicating = value;
                OnDeviceCommunicatingChange(this, _deviceCommunicating);
            }
        }

        public abstract string DeviceAddressString { get; }

        public abstract string SerialNumber { get; }

        public abstract string VersionInfo { get; }

        public virtual bool Power
        {
            get { return PowerStatus == DevicePowerStatus.PowerOn || PowerStatus == DevicePowerStatus.PowerWarming; }
            set
            {
                RequestedPower = value;
                ActionPowerRequest(RequestedPower);
            }
        }

        public bool RequestedPower { get; private set; }

        public DevicePowerStatus PowerStatus
        {
            get { return _powerStatus; }
            protected set
            {
                if (_powerStatus == value) return;
                var oldState = _powerStatus;
                _powerStatus = value;
                CloudLog.Info("{0} PowerStatus = {1}", this, value);
                OnPowerStatusChange(this, new DevicePowerStatusEventArgs(_powerStatus, oldState));
            }
        }

        /// <summary>
        /// The current input for the display
        /// </summary>
        public abstract DisplayDeviceInput CurrentInput { get; }

        /// <summary>
        /// Get a list of available supported inputs
        /// </summary>
        public abstract IEnumerable<DisplayDeviceInput> AvailableInputs { get; }

        /// <summary>
        /// True if the display uses DisplayUsage
        /// </summary>
        public abstract bool SupportsDisplayUsage { get; }

        /// <summary>
        /// Get the display usage value as a ushort, use for a analog guage.
        /// </summary>
        public ushort DisplayUsage
        {
            get { return _displayUsage; }
            protected set
            {
                if (_displayUsage == value) return;
                _displayUsage = value;
                OnDisplayUsageChange(this);
            }
        }

        public FusionAssetType AssetType
        {
            get
            {
                return FusionAssetType.Display;
            }
        }

        #endregion

        #region Methods

        protected virtual void OnPowerStatusChange(IPowerDevice device, DevicePowerStatusEventArgs args)
        {
            var handler = PowerStatusChange;
            if (handler == null) return;
            try
            {
                handler(device, args);
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }
        }

        protected virtual void OnDisplayUsageChange(DisplayDeviceBase display)
        {
            var handler = DisplayUsageChange;
            if (handler == null) return;
            try
            {
                handler(display);
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }
        }

        protected virtual void OnDeviceCommunicatingChange(IDevice device, bool communicating)
        {
            var handler = DeviceCommunicatingChange;
            if (handler == null) return;
            try
            {
                if (communicating)
                {
                    CloudLog.Notice("{0}.DeviceCommunicating = {1}", GetType().Name, true);
                }
                else
                {
                    CloudLog.Warn("{0}.DeviceCommunicating = {1}", GetType().Name, false);                    
                }
                handler(device, communicating);
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }
        }

        /// <summary>
        /// Call this when receiving power status feedback from device.
        /// PowerStatus = newPowerState;
        /// </summary>
        /// <param name="newPowerState"></param>
        protected abstract void SetPowerFeedback(DevicePowerStatus newPowerState);

        /// <summary>
        /// This is called when the power is set by the API. Make necessary arrangements to make sure the device follows the request
        /// </summary>
        /// <param name="powerRequest"></param>
        protected abstract void ActionPowerRequest(bool powerRequest);

        /// <summary>
        /// Set the input on the display
        /// </summary>
        /// <param name="input"></param>
        public abstract void SetInput(DisplayDeviceInput input);

        /// <summary>
        /// Initialize the display
        /// </summary>
        public abstract void Initialize();

        public override string ToString()
        {
            return string.Format("{0} \"{1}\"", GetType().Name, Name);
        }

        #endregion
    }

    /// <summary>
    /// Event handler for a display when the value changes
    /// </summary>
    /// <param name="display">The display which calls the event</param>
    public delegate void DisplayUsageChangeEventHandler(DisplayDeviceBase display);
}