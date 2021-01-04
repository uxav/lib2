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

namespace UX.Lib2.Models
{
    public class RoomCollection : IEnumerable<RoomBase>
    {
        #region Fields

        private readonly Dictionary<uint, RoomBase> _rooms = new Dictionary<uint, RoomBase>();

        #endregion

        #region Constructors

        /// <summary>
        /// The default Constructor.
        /// </summary>
        internal RoomCollection()
        {
            
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events
        #endregion

        #region Delegates
        #endregion

        public RoomBase this[uint id]
        {
            get { return _rooms[id]; }
        }

        #region Properties
        #endregion

        #region Methods

        /// <summary>
        /// Returns true if the collection contains the ID key
        /// </summary>
        /// <param name="id">The ID for the Room</param>
        /// <returns>True if exists</returns>
        public bool ContainsKey(uint id)
        {
            return _rooms.ContainsKey(id);
        }

        internal void Add(RoomBase room)
        {
            _rooms.Add(room.Id, room);
        }

        public IEnumerator<RoomBase> GetEnumerator()
        {
            return _rooms.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}