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

namespace UX.Lib2.UI
{
    internal sealed class UIActionSheetButtonListItem : UISubPageReferenceListItem
    {
        #region Fields

        private UIActionSheetButtonMode _buttonMode;

        #endregion

        #region Constructors

        public UIActionSheetButtonListItem(UISubPageReferenceList list, uint index)
            : base(list, index)
        {

        }

        #endregion

        #region Finalizers
        #endregion

        #region Events
        #endregion

        #region Delegates
        #endregion

        #region Properties

        public UIActionSheetButtonMode ButtonMode
        {
            get { return _buttonMode; }
            set
            {
                _buttonMode = value;
                UShortInputSigs[1].UShortValue = (ushort) value;
            }
        }

        #endregion

        #region Methods

        #endregion
    }
}