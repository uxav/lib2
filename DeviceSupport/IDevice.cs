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

namespace UX.Lib2.DeviceSupport
{
    /// <summary>
    /// Base interface for a device
    /// </summary>
    public interface IDevice
    {
        /// <summary>
        /// The name of the device
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// The name of the manufacturer for the device
        /// </summary>
        string ManufacturerName { get; }

        /// <summary>
        /// A name used for diagnostics reporting. Can include connection details.
        /// </summary>
        string DiagnosticsName { get; }

        /// <summary>
        /// The model name of the device
        /// </summary>
        string ModelName { get; }

        /// <summary>
        /// Returns true if the device comms are ok!
        /// </summary>
        bool DeviceCommunicating { get; }
        
        /// <summary>
        /// Return an IP address or similar
        /// </summary>
        string DeviceAddressString { get; }

        /// <summary>
        /// Return device serial number
        /// </summary>
        string SerialNumber { get; }

        /// <summary>
        /// Version information string
        /// </summary>
        string VersionInfo { get; }

        /// <summary>
        /// Event called if the comms status changes on the device
        /// </summary>
        event DeviceCommunicatingChangeHandler DeviceCommunicatingChange;
    }

    public delegate void DeviceCommunicatingChangeHandler(IDevice device, bool communicating);
}