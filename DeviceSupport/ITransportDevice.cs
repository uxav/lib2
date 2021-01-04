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
    public interface ITransportDevice
    {
        bool Playing { get; }
        bool Stopped { get; }
        bool Paused { get; }
        void Play();
        void Stop();
        void Pause();
        void SkipForward();
        void SkipBack();
        void SendCommand(TransportDeviceCommand command);
        void SendCommandPress(TransportDeviceCommand command);
        void SendCommandRelease(TransportDeviceCommand command);
    }

    public enum TransportDeviceCommand
    {
        Play,
        Stop,
        Pause,
        Record,
        SkipForward,
        SkipBack,
        FastForward,
        Rewind,
        Eject,
        Keypad0,
        Keypad1,
        Keypad2,
        Keypad3,
        Keypad4,
        Keypad5,
        Keypad6,
        Keypad7,
        Keypad8,
        Keypad9,
        Dot,
        Clear,
        Home,
        Menu,
        Setup,
        Guide,
        Exit,
        Return,
        Up,
        Down,
        Left,
        Right,
        Select,
        Enter,
        Info,
        Favorites,
        ChannelUp,
        ChannelDown,
        Next,
        Previous,
        PageUp,
        PageDown,
        Red,
        Green,
        Yellow,
        Blue
    }
}