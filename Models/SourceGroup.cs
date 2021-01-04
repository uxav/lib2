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

using System.Linq;

namespace UX.Lib2.Models
{
    public class SourceGroup
    {
        private readonly SourceCollection _collection;
        private readonly string _name;

        #region Fields
        #endregion

        #region Constructors

        internal SourceGroup(SourceCollection collection, string name)
        {
            _collection = collection;
            _name = name;
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events
        #endregion

        #region Delegates
        #endregion

        #region Properties

        public string Name
        {
            get { return _name; }
        }

        #endregion

        #region Methods

        public SourceCollection GetSources(RoomBase room)
        {
            return new SourceCollection(_collection.ForRoomOrGlobal(room).Where(s => s.Group == this));
        }

        public bool Contains(SourceBase source)
        {
            return _collection != null && _collection.Any(s => s == source && s.Group == this);
        }

        #endregion
    }
}