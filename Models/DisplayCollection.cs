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
using System.Linq;

namespace UX.Lib2.Models
{
    public class DisplayCollection : IEnumerable<DisplayBase>
    {
        #region Fields

        private readonly Dictionary<uint, DisplayBase> _displays; 

        #endregion

        #region Constructors

        internal DisplayCollection()
        {
            _displays = new Dictionary<uint, DisplayBase>();
        }

        internal DisplayCollection(IEnumerable<DisplayBase> fromDisplays)
        {
            _displays = new Dictionary<uint, DisplayBase>();
            foreach (var display in fromDisplays)
            {
                _displays.Add(display.Id, display);
            }
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events
        #endregion

        #region Delegates
        #endregion

        public DisplayBase this[uint id]
        {
            get { return _displays[id]; }
        }

        #region Properties
        #endregion

        #region Methods

        internal void Add(DisplayBase display)
        {
            _displays[display.Id] = display;
        }

        /// <summary>
        /// Get a collection of displays assigned to a specific room
        /// </summary>
        /// <param name="room">A Room</param>
        /// <returns>DisplayCollection</returns>
        public DisplayCollection ForRoom(RoomBase room)
        {
            var displays = this.Where(d => d.Room == room);
            return new DisplayCollection(displays);
        }

        /// <summary>
        /// Get a collection of displays assigned to a room or are global
        /// </summary>
        /// <param name="room">A room</param>
        /// <returns>DisplayCollection</returns>
        public DisplayCollection ForRoomOrGlobal(RoomBase room)
        {
            var displays = this.Where(d => d.Room == room || d.Room == null);
            return new DisplayCollection(displays);
        }

        public IEnumerator<DisplayBase> GetEnumerator()
        {
            return _displays.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}