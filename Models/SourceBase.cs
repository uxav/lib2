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

namespace UX.Lib2.Models
{
    public abstract class SourceBase
    {
        #region Fields

        private static uint _idCount;
        private RoomBase _assignedRoom;
        private readonly SystemBase _system;
        private readonly SourceType _type;
        private ISourceDevice _device;
        private int _roomCount;
        private string _name;
        private SourceGroup _group;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a source which will be global and used in all rooms
        /// </summary>
        /// <param name="system">System the source belongs to</param>
        /// <param name="type">The type of source</param>
        /// <param name="device">The device driver for the source</param>
        protected SourceBase(SystemBase system, SourceType type, ISourceDevice device)
        {
            _idCount ++;
            Id = _idCount;
            _system = system;
            _type = type;
            _device = device;
            system.Sources.Add(this);
        }

        /// <summary>
        /// Create a source which is local to a room and assigned for use only in the room
        /// </summary>
        /// <param name="room">The room which the source is assigned to</param>
        /// <param name="type">The type of source</param>
        /// <param name="device">The device driver for the source</param>
        protected SourceBase(RoomBase room, SourceType type, ISourceDevice device)
            : this(room.System, type, device)
        {
            _assignedRoom = room;
        }

        /// <summary>
        /// Create a source which is local to a display only
        /// </summary>
        /// <param name="display">The display used</param>
        /// <param name="displayDeviceInput">Input used to connect to the display</param>
        /// <param name="type">The type of source</param>
        /// <param name="device">The device driver for the source</param>
        protected SourceBase(DisplayBase display, DisplayDeviceInput displayDeviceInput, SourceType type, ISourceDevice device)
            : this(display.System, type, device)
        {
            AssignedDisplay = display;
            DisplayDeviceInput = displayDeviceInput;
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events
        #endregion

        #region Delegates
        #endregion

        #region Properties

        /// <summary>
        /// The unique Id of the source. This is automatically generated based on index of the sources construction.
        /// </summary>
        public uint Id { get; private set; }

        /// <summary>
        /// Get or set the name of the source
        /// </summary>
        public string Name
        {
            get { return String.IsNullOrEmpty(_name) ? string.Format("{0} Source, {1}", Type, Id) : _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Get or set the name of the source icon
        /// </summary>
        public string IconName { get; set; }

        public SourceGroup Group
        {
            get { return _group; }
        }

        /// <summary>
        /// True if the source is assigned to a room
        /// </summary>
        public bool IsAssignedToRoom
        {
            get { return _assignedRoom != null; }
        }

        /// <summary>
        /// The assigned room for this source. Null if not assigned
        /// </summary>
        public RoomBase AssignedRoom
        {
            get { return _assignedRoom; }
            set { _assignedRoom = value; }
        }

        /// <summary>
        /// The type of source
        /// </summary>
        public SourceType Type
        {
            get { return _type; }
        }

        /// <summary>
        /// The system this source belongs to
        /// </summary>
        public SystemBase System
        {
            get { return _system; }
        }

        /// <summary>
        /// The count of rooms which this source is currently being used by.
        /// Does not count if the source is a display local source
        /// </summary>
        public int RoomCount
        {
            get { return _roomCount; }
            internal set
            {
                CloudLog.Debug("{0} RoomCount = {1}", this, value);
                var oldVal = _roomCount;
                _roomCount = value;
                if (_roomCount > oldVal && _device != null)
                {
                    try
                    {
                        _device.UpdateOnSourceRequest();
                    }
                    catch (Exception e)
                    {
                        CloudLog.Exception(e);
                    }
                }
                if (_roomCount == 0 && _device != null)
                {
                    try
                    {
                        _device.StopPlaying();
                    }
                    catch (Exception e)
                    {
                        CloudLog.Exception(e);
                    }
                    try
                    {
                        SourceEnded();
                    }
                    catch (Exception e)
                    {
                        CloudLog.Exception(e);
                    }
                }
                if (_roomCount == 1 && _device != null)
                {
                    try
                    {
                        SourceStarted();
                    }
                    catch (Exception e)
                    {
                        CloudLog.Exception(e);
                    }
                    try
                    {
                        _device.StartPlaying();
                    }
                    catch (Exception e)
                    {
                        CloudLog.Exception(e);
                    }
                }
            }
        }

        /// <summary>
        /// True if the source is local only to a display.. check AssignedDisplay if true
        /// </summary>
        public bool IsLocalToDisplayOnly
        {
            get { return AssignedDisplay != null; }
        }

        /// <summary>
        /// The assigned display if local only to a display
        /// </summary>
        public DisplayBase AssignedDisplay { get; private set; }

        /// <summary>
        /// The input used as the local display input
        /// </summary>
        public DisplayDeviceInput DisplayDeviceInput { get; protected set; }

        /// <summary>
        /// True if the source is in use by a room
        /// </summary>
        public bool InUse
        {
            get { return RoomCount > 0; }
        }

        /// <summary>
        /// The device driver for the source
        /// </summary>
        public ISourceDevice Device
        {
            get { return _device; }
        }

        public virtual bool IsPresentationSource
        {
            get
            {
                switch (Type)
                {
                    case SourceType.AirMedia:
                    case SourceType.ClickShare:
                    case SourceType.SolsticePod:
                    case SourceType.Pano:
                    case SourceType.GenericWirelessPresentationDevice:
                    case SourceType.Laptop:
                    case SourceType.PC:
                        return true;
                    default:
                        return false;
                }
            }
        }

        public virtual bool IsMediaSource
        {
            get
            {
                switch (Type)
                {
                    case SourceType.AppleTV:
                    case SourceType.AuxInput:
                    case SourceType.BluRay:
                    case SourceType.DVD:
                    case SourceType.FreeSat:
                    case SourceType.FreeView:
                    case SourceType.SkyHD:
                    case SourceType.iPod:
                    case SourceType.IPTV:
                    case SourceType.LiveStream:
                    case SourceType.MovieServer:
                    case SourceType.MusicServer:
                    case SourceType.SignagePlayer:
                    case SourceType.Satellite:
                    case SourceType.SkyQ:
                        return true;
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// The availabililty type of the source.
        /// Use for setting if source is available on a main menu or for content sharing through
        /// a codec or similar
        /// </summary>
        public SourceAvailabilityType AvailabilityType { get; protected set; }

        public bool IsAvailableForMainSourceUse
        {
            get
            {
                return AvailabilityType == SourceAvailabilityType.MainSourceAndContentShare ||
                       AvailabilityType == SourceAvailabilityType.MainSource;
            }
        }

        public bool IsAvailableForContentShareUse
        {
            get
            {
                return AvailabilityType == SourceAvailabilityType.MainSourceAndContentShare ||
                       AvailabilityType == SourceAvailabilityType.ContentShareOnly;
            }
        }

        public bool IsWirelessPresentationDevice
        {
            get
            {
                switch (Type)
                {
                        case SourceType.AirMedia:
                        case SourceType.SolsticePod:
                        case SourceType.ClickShare:
                        case SourceType.GenericWirelessPresentationDevice:
                        return true;
                }
                return false;
            }
        }

        #endregion

        #region Methods

        public void AddToGroup(string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name should not be null or empty");
            }
            _group = _system.Sources.GetOrCreateGroup(name);
        }

        public void RemoveFromGroup()
        {
            _group = null;
        }

        public virtual void SetDevice(ISourceDevice device)
        {
            _device = device;
        }

        protected virtual void SourceStarted()
        {
            
        }

        protected virtual void SourceEnded()
        {
            
        }

        /// <summary>
        /// Generate SVG code for the source Icon
        /// </summary>
        /// <returns></returns>
        public virtual string GenerateIconData()
        {
            return GenerateDefaultIconData(Type);
        }

        public static string GenerateDefaultIconData(SourceType type)
        {
            switch (type)
            {
                case SourceType.VideoConference:
                    return
                        "<svg xmlns=\"http://www.w3.org/2000/svg\" x=\"0px\" y=\"0px\" width=\"50\" height=\"50\" viewBox=\"0 0 50 50\" style=\"\">" +
                        "<g id=\"surface1\">" +
                        "<path style=\"\" d=\"M 3 8 C 2.449219 8 2 8.449219 2 9 L 2 39 C 2 39.554688 2.449219 40 3 40 L 47 40 C 47.554688 40 48 39.554688 48 39 L 48 9 C 48 8.449219 47.554688 8 47 8 Z M 25 13 C 27.761719 13 30 15.238281 30 18 C 30 20.761719 27.761719 23 25 23 C 22.238281 23 20 20.761719 20 18 C 20 15.238281 22.238281 13 25 13 Z M 22 25 L 28 25 C 30.761719 25 33 27.238281 33 30 L 17 30 C 17 27.238281 19.238281 25 22 25 Z M 10 32 L 16 32 L 16 36 L 10 36 Z M 18 32 L 24 32 L 24 36 L 18 36 Z M 26 32 L 32 32 L 32 36 L 26 36 Z M 34 32 L 40 32 L 40 36 L 34 36 Z M 15 42 C 14.640625 41.996094 14.304688 42.183594 14.121094 42.496094 C 13.941406 42.808594 13.941406 43.191406 14.121094 43.503906 C 14.304688 43.816406 14.640625 44.003906 15 44 L 35 44 C 35.359375 44.003906 35.695313 43.816406 35.878906 43.503906 C 36.058594 43.191406 36.058594 42.808594 35.878906 42.496094 C 35.695313 42.183594 35.359375 41.996094 35 42 Z \">" +
                        "</path></g></svg>";
                default:
                    return "";
            }
        }

        public override string ToString()
        {
            return string.Format("{0} Source Id: {1}, \"{2}\"", Type, Id, Name);
        }

        #endregion
    }

    public enum SourceType
    {
        Unknown,
        VideoConference,
        PC,
        Laptop,
        DVD,
        BluRay,
        TV,
        IPTV,
        Satellite,
        Tuner,
        AM,
        FM,
        DAB,
        InternetRadio,
        iPod,
        AirPlay,
        MovieServer,
        MusicServer,
        InternetService,
        AppleTV,
        Chromecast,
        AndroidTV,
        XBox,
        PlayStation,
        NintendoWii,
        AirMedia,
        ClickShare,
        CCTV,
        AuxInput,
        LiveStream,
        SignagePlayer,
        GenericWirelessPresentationDevice,
        Sky,
        SkyHD,
        SkyQ,
        FreeView,
        FreeSat,
        YouView,
        YouTube,
        FireBox,
        Skype,
        Hangouts,
        Sonos,
        Pano,
        SolsticePod,
        PolycomTrio
    }

    public enum SourceAvailabilityType
    {
        MainSource,
        MainSourceAndContentShare,
        ContentShareOnly
    }
}