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
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronThread;
using ICSharpCode.SharpZipLib.Tar;
using UX.Lib2.Annotations;

namespace UX.Lib2.DeviceSupport
{
    /// <summary>
    /// Extension methods used by certain Crestron devices like ComPorts etc
    /// </summary>
    public static class Extenstions
    {
        #region ComPort Extensions
        /// <summary>
        /// Send bytes to the serial port to prevent issues with encoding of non ascii strings
        /// </summary>
        /// <param name="port">ComPort to extend</param>
        /// <param name="bytes">Byte array to send</param>
        /// <param name="count">Count of bytes from array</param>
        public static void Send(this IComPortDevice port, byte[] bytes, int count)
        {
            var str = string.Empty;
            for (var i = 0; i < count; i++)
            {
                str = str + (char)bytes[i];
            }
            port.Send(str);
        }

        /// <summary>
        /// Send a byte to the serial port to prevent issues with encoding of non ascii strings
        /// </summary>
        /// <param name="port">ComPort to extend</param>
        /// <param name="b">Byte to send</param>
        public static void Send(this IComPortDevice port, byte b)
        {
            var str = string.Empty + (char) b;
            port.Send(str);
        }
        #endregion

        #region IROutputPort Extensions
        /// <summary>
        /// Send bytes to IR serial port to prevent issues with encoding of non ascii strings
        /// </summary>
        /// <param name="port">IROutputPort to extend</param>
        /// <param name="bytes">Byte array to send</param>
        /// <param name="count">Count ot bytes from array</param>
        public static void SendSerialData(this IROutputPort port, byte[] bytes, int count)
        {
            var str = string.Empty;
            for (var i = 0; i < count; i++)
            {
                str = str + (char)bytes[i];
            }
            port.SendSerialData(str);
        }
        #endregion

        #region Sig Extensions
        /// <summary>
        /// Ramp an analog value up at a rate determined by the total time from down to up
        /// </summary>
        /// <param name="sig">Analog sig to ramp up</param>
        /// <param name="changeCallback">Callback delegate to use to update the value when it changes</param>
        /// <param name="totalSeconds">Total time it should take to ramp up from 0 to 100%</param>
        public static void RampUp(this UShortInputSig sig, UShortInputSigRampingCallback changeCallback, double totalSeconds)
        {
            var span = ushort.MaxValue - sig.UShortValue;
            var ms = totalSeconds*1000;
            var relativeTime = span/ms;
            sig.CreateRamp(ushort.MaxValue, (uint) (relativeTime/10));
            var t = new Thread(specific =>
            {
                var s = (UShortInputSig) specific;
                while (s.IsRamping)
                {
                    Thread.Sleep(10);
                    changeCallback(s);
                }
                changeCallback(s);
                return null;
            }, changeCallback)
            {
                Name = string.Format("{0} RampUp Process", sig),
            };
        }

        /// <summary>
        /// Ramp an analog value down at a rate determined by the total time from up to down
        /// </summary>
        /// <param name="sig">Analog sig to ramp down</param>
        /// <param name="changeCallback">Callback delegate to use to update the value when it changes</param>
        /// <param name="totalSeconds">Total time it should take to ramp down from 100 to 0%</param>
        public static void RampDown(this UShortInputSig sig, UShortInputSigRampingCallback changeCallback, double totalSeconds)
        {
            var ms = totalSeconds*1000;
            var relativeTime = sig.UShortValue/ms;
            sig.CreateRamp(ushort.MinValue, (uint) (relativeTime/10));
            var t = new Thread(specific =>
            {
                var s = (UShortInputSig)specific;
                while (s.IsRamping)
                {
                    Thread.Sleep(10);
                    changeCallback(s);
                }
                changeCallback(s);
                return null;
            }, changeCallback)
            {
                Name = string.Format("{0} RampDown Process", sig),
            };
        }
        #endregion
    }

    public delegate void UShortInputSigRampingCallback(UShortInputSig sig);
}