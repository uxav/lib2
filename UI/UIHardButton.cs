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
    public class UIHardButton : UIButton
    {
        #region Fields

        private readonly HardKeyBackLightMethod _backlightOnMethod;
        private readonly HardKeyBackLightMethod _backlightOffMethod;
        private bool _visible;

        #endregion

        #region Constructors

        public UIHardButton(UIController uiController, uint pressJoinNumber)
            : this(uiController, pressJoinNumber, null, null)
        {

        }

        public UIHardButton(UIController uiController, uint pressJoinNumber, HardKeyBackLightMethod backlightOnMethod,
            HardKeyBackLightMethod backlightOffMethod)
            : base(uiController, pressJoinNumber)
        {
            _backlightOnMethod = backlightOnMethod;
            _backlightOffMethod = backlightOffMethod;

            if (_backlightOffMethod == null || _backlightOnMethod == null)
            {
                return;
            }

            backlightOffMethod();

            uiController.Device.OnlineStatusChange += (device, args) =>
            {
                if (!args.DeviceOnLine) return;

                if (_visible)
                    _backlightOnMethod();
                else
                {
                    _backlightOffMethod();
                }
            };
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events
        #endregion

        #region Delegates
        #endregion

        #region Properties

        public override bool Visible
        {
            get
            {
                return _visible;
            }
            set
            {
                if(_visible == value) return;

                RequestedVisibleState = value;

                OnVisibilityChanged(this,
                    new VisibilityChangeEventArgs(value, value
                        ? VisibilityChangeEventType.WillShow
                        : VisibilityChangeEventType.WillHide));

                _visible = value;

                OnVisibilityChanged(this,
                    new VisibilityChangeEventArgs(value, value
                        ? VisibilityChangeEventType.DidShow
                        : VisibilityChangeEventType.DidHide));

                if (_backlightOnMethod == null || _backlightOffMethod == null) return;

                if (_visible)
                {
                    _backlightOnMethod();
                }
                else
                {
                    _backlightOffMethod();
                }
            }
        }

        #endregion

        #region Methods
        #endregion
    }

    public delegate void HardKeyBackLightMethod();
}