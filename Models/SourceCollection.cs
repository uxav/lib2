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

using System.Collections.Generic;
using System.Linq;

namespace UX.Lib2.Models
{
    public class SourceCollection : IEnumerable<SourceBase>
    {
        #region Fields

        private readonly Dictionary<uint, SourceBase> _sources;
        private readonly Dictionary<string, SourceGroup> _groups; 

        #endregion

        #region Constructors

        /// <summary>
        /// The default Constructor.
        /// </summary>
        internal SourceCollection()
        {
            _sources = new Dictionary<uint, SourceBase>();
            _groups = new Dictionary<string, SourceGroup>();
        }

        /// <summary>
        /// Create a collection from another collection of sources
        /// </summary>
        /// <param name="fromSources"></param>
        internal SourceCollection(IEnumerable<SourceBase> fromSources)
        {
            _sources = new Dictionary<uint, SourceBase>();
            foreach (var source in fromSources)
            {
                _sources.Add(source.Id, source);
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

        /// <summary>
        /// Return a source by the ID
        /// </summary>
        /// <param name="id">The unique source ID</param>
        /// <returns></returns>
        public SourceBase this[uint id]
        {
            get { return _sources[id]; }
        }

        /// <summary>
        /// Returns the first source available of this type
        /// </summary>
        /// <param name="type">A SourceType value</param>
        /// <returns></returns>
        public SourceBase this[SourceType type]
        {
            get { return _sources.Values.FirstOrDefault(s => s.Type == type); }
        }

        /// <summary>
        /// Returns a SourceCollection of sources which are set for Main Source use
        /// </summary>
        public SourceCollection MainSources
        {
            get { return new SourceCollection(this.Where(s => s.IsAvailableForMainSourceUse)); }
        }

        /// <summary>
        /// Returns a SourceCollection of sources which are set for Content Sharing
        /// </summary>
        public SourceCollection ContentShareSources
        {
            get
            {
                return new SourceCollection(this.Where(s => s.IsAvailableForContentShareUse)
                    .OrderByDescending(s => s.IsWirelessPresentationDevice)
                    .ThenByDescending(s => s.Type == SourceType.PC));
            }
        }

        public SourceCollection PresentationSources
        {
            get
            {
                return
                    new SourceCollection(this.Where(s => s.IsPresentationSource)
                        .OrderByDescending(s => s.IsWirelessPresentationDevice)
                        .ThenByDescending(s => s.Type == SourceType.PC));
            }
        }

        public SourceCollection MediaSources
        {
            get { return new SourceCollection(this.Where(s => s.IsMediaSource)); }
        }

        public SourceCollection GlobalSources
        {
            get { return new SourceCollection(this.Where(s => s.AssignedRoom == null)); }
        }

        #endregion

        #region Methods

        internal void Add(SourceBase source)
        {
            _sources.Add(source.Id, source);
        }

        internal SourceGroup GetOrCreateGroup(string groupName)
        {
            if (!_groups.ContainsKey(groupName))
            {
                _groups[groupName] = new SourceGroup(this, groupName);
            }

            return _groups[groupName];
        }

        /// <summary>
        /// Get a collection of sources assigned to a specific room
        /// </summary>
        /// <param name="room">A Room</param>
        /// <returns>SourceCollection</returns>
        public SourceCollection ForRoom(RoomBase room)
        {
            var sources = this.Where(s => s.AssignedRoom == room);
            return new SourceCollection(sources);
        }

        /// <summary>
        /// Get a collection of sources assigned to a room or is a global source
        /// </summary>
        /// <returns>SourceCollection</returns>
        public SourceCollection ForRoomOrGlobal()
        {
            var sources = this.Where(s => !s.IsLocalToDisplayOnly);
            return new SourceCollection(sources);
        }

        /// <summary>
        /// Get a collection of sources assigned to a specific room or is a global source
        /// </summary>
        /// <param name="room">A room</param>
        /// <returns>SourceCollection</returns>
        public SourceCollection ForRoomOrGlobal(RoomBase room)
        {
            var sources = this.Where(s => (s.AssignedRoom == room || s.AssignedRoom == null) && !s.IsLocalToDisplayOnly);
            return new SourceCollection(sources);
        }
        
        /// <summary>
        /// Get collection of sources assigned to a display only
        /// </summary>
        /// <param name="display">The display</param>
        /// <returns>SourceCollection</returns>
        public SourceCollection ForDisplay(DisplayBase display)
        {
            var sources = this.Where(s => s.AssignedDisplay != null && s.AssignedDisplay == display);
            return new SourceCollection(sources);
        }

        /// <summary>
        /// Get a collection of sources of a specific type
        /// </summary>
        /// <param name="type">The type of source to get</param>
        /// <returns>SourceCollection</returns>
        public SourceCollection OfSourceType(SourceType type)
        {
            var sources = this.Where(s => s.Type == type);
            return new SourceCollection(sources);
        }
        
        /// <summary>
        /// Get sources of a specific availability type
        /// </summary>
        /// <param name="type">SourceAvailabilityType</param>
        /// <returns>SourceCollection</returns>
        public SourceCollection OfAvailabilityType(SourceAvailabilityType type)
        {
            var sources = this.Where(s => s.AvailabilityType == type);
            return new SourceCollection(sources);
        }

        public IEnumerator<SourceBase> GetEnumerator()
        {
            return _sources.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}