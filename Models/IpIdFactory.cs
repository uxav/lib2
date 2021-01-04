using System;
using System.Collections.Generic;
using Crestron.SimplSharp;

namespace UX.Lib2.Models
{
    public static class IpIdFactory
    {
        private static readonly Dictionary<uint, DeviceType> Dict = new Dictionary<uint, DeviceType>(); 

        /// <summary>
        /// The type of device for the IP ID range
        /// </summary>
        public enum DeviceType
        {
            TouchPanel,
            Airmedia,
            Dm,
            Nvx,
            Fusion,
            Other
        }

        public static ReadOnlyDictionary<uint, DeviceType> CreatedValues
        {
            get { return new ReadOnlyDictionary<uint, DeviceType>(Dict); }
        } 

        /// <summary>
        /// Returns the starting value for the IP ID for a device type
        /// See this link for the ranges: https://docs.google.com/spreadsheets/d/10RSe9k98vvcjUy_012ijayA8IvkcdiZ8z5Qi2t2pfsg/edit?usp=sharing
        /// </summary>
        /// <param name="type">The type of device</param>
        /// <returns>Starting value for the IP ID range</returns>
        public static uint GetStartIndexForDeviceType(DeviceType type)
        {
            switch (type)
            {
                case DeviceType.TouchPanel:
                    return 0x04;
                case DeviceType.Airmedia:
                    return 0x60;
                case DeviceType.Dm:
                    return 0x80;
                case DeviceType.Nvx:
                    return 0xC0;
                case DeviceType.Fusion:
                    return 0xF0;
            }

            return 0x21;
        }

        private static DeviceType GetDeviceTypeForIpId(uint ipId)
        {
            if (ipId >= 0xF0)
            {
                return DeviceType.Fusion;
            }

            if (ipId >= 0xC0)
            {
                return DeviceType.Nvx;
            }

            if (ipId >= 0x80)
            {
                return DeviceType.Dm;
            }

            if (ipId >= 0x60)
            {
                return DeviceType.Airmedia;
            }

            if (ipId >= 0x04 && ipId < 0x60)
            {
                return DeviceType.TouchPanel;
            }

            return DeviceType.Other;
        }

        /// <summary>
        /// Create and get an unused IP ID for a device type
        /// </summary>
        /// <param name="type">The type of device</param>
        /// <returns>An unused IP ID number</returns>
        public static uint Create(DeviceType type)
        {
            var id = GetStartIndexForDeviceType(type);

            while (Dict.ContainsKey(id))
            {
                id++;
            }

            if (type != GetDeviceTypeForIpId(id))
            {
                id = GetStartIndexForDeviceType(DeviceType.Other);

                while (Dict.ContainsKey(id))
                {
                    id ++;
                }
            }

            Dict.Add(id, type);

            return id;
        }

        /// <summary>
        /// Create and get an unused IP ID for a device type
        /// </summary>
        /// <param name="type">The type of device</param>
        /// <param name="preferredId">Use a preferred ID if available</param>
        /// <returns>An unused IP ID number</returns>
        public static uint Create(DeviceType type, uint preferredId)
        {
            if (Dict.ContainsKey(preferredId)) return Create(type);
            Dict.Add(preferredId, type);
            return preferredId;
        }

        /// <summary>
        /// Use to block out a specific IP ID so it isn't created next time around
        /// </summary>
        /// <param name="id">IP ID to block out</param>
        /// <param name="type">The type of device</param>
        public static uint Block(uint id, DeviceType type)
        {
            if (Dict.ContainsKey(id)) throw new InvalidOperationException("Key already exists");
            Dict[id] = type;
            return id;
        }

        /// <summary>
        /// Clear the values and reset!
        /// </summary>
        public static void Clear()
        {
            Dict.Clear();
        }
    }
}