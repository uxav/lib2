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
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;

namespace UX.Lib2.DeviceSupport
{
    public interface ISwitcher : IFusionAsset
    {
        void RouteVideo(uint input, uint output);
        void RouteAudio(uint input, uint output);
        uint GetVideoInput(uint output);
        uint GetAudioInput(uint output);
        bool InputIsActive(uint input);
        event SwitcherInputStatusChangedEventHandler InputStatusChanged;
        void Init();
        bool SupportsDMEndPoints { get; }
        EndpointReceiverBase GetEndpointForOutput(uint output);
        HdmiInputWithCEC GetHdmiCecInput(uint input);
    }

    public delegate void SwitcherInputStatusChangedEventHandler(ISwitcher switcher, SwitcherInputStatusChangeEventArgs args);

    public class SwitcherInputStatusChangeEventArgs : EventArgs
    {
        public SwitcherInputStatusChangeEventArgs(ISwitcher switcher, uint number)
        {
            Switcher = switcher;
            Number = number;
        }

        ISwitcher Switcher { get; set; }
        public uint Number { get; protected set; }
        public bool HasVideo
        {
            get
            {
                return Switcher.InputIsActive(Number);
            }
        }
    }

    public enum SwitcherInputOutputType
    {
        AV,
        VideoOnly,
        AudioOnly
    }
}