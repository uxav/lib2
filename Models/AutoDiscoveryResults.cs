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

using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;

namespace UX.Lib2.Models
{
    public class AutoDiscoveryResults : IEnumerable<AutoDiscoveredDevice>
    {
        #region Fields

        private readonly Dictionary<string, AutoDiscoveredDevice> _devices = new Dictionary<string, AutoDiscoveredDevice>(); 

        #endregion

        #region Constructors

        internal AutoDiscoveryResults(IEnumerable<EthernetAutodiscovery.AutoDiscoveredDeviceElement> elements)
        {
            foreach (var element in elements)
            {
                var info = Regex.Match(element.DeviceIdString, @"^([\w-]+) \[(\S+)[\s\S]*#(\w+)\](?: @E-(\w+))?$");
                
                if(!info.Success) continue;

                var result = new AutoDiscoveredDevice
                {
                    Model = info.Groups[1].Value,
                    Version = info.Groups[2].Value,
                    SerialNumber = info.Groups[3].Value,
                    MacAddress = info.Groups[4].Value,
                    IpId = element.IPId,
                    IpAddress = element.IPAddress,
                    HostName = element.HostName
                };

                _devices.Add(result.IpAddress, result);

                if (result.HostName.Length > LongestHostName)
                    LongestHostName = result.HostName.Length;
                if (result.Model.Length > LongestModelName)
                    LongestModelName = result.Model.Length;
            }
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events
        #endregion

        #region Delegates
        #endregion

        #region Properties

        public AutoDiscoveredDevice this[string ipAddress]
        {
            get { return _devices[ipAddress]; }
        }

        public int Count
        {
            get { return _devices.Count; }
        }

        public int LongestModelName { get; private set; }
        public int LongestHostName { get; private set; }

        #endregion

        #region Methods

        public bool ContainsDeviceWithIpAddress(string ipAddress)
        {
            return _devices.ContainsKey(ipAddress);
        }

        #endregion

        public IEnumerator<AutoDiscoveredDevice> GetEnumerator()
        {
            return _devices.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}