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
using System.Globalization;
using Crestron.SimplSharpPro;
using UX.Lib2.Cloud.Logger;

namespace UX.Lib2.UI
{
    public class UIKeypad
    {
        #region Fields

        private readonly ButtonCollection _buttons;
        private UIKeypadButtonEvent _buttonEvent;
        private int _subscribeCount;

        #endregion

        #region Constructors

        /// <summary>
        /// The default Constructor.
        /// </summary>
        public UIKeypad(SmartObject smartObject)
        {
            CloudLog.Debug("{0}.ctor for SmartObject ID: {1}", GetType(), smartObject.ID);

            _buttons = new ButtonCollection {{0, new UIButton(smartObject, 10)}};
            for (uint cueIndex = 1; cueIndex <= 9; cueIndex++)
            {
                _buttons.Add(cueIndex, new UIButton(smartObject, cueIndex));
            }
            _buttons.Add(10, new UIButton(smartObject, 11));
            _buttons.Add(11, new UIButton(smartObject, 12));

            foreach (var button in _buttons)
            {
                button.HoldTime = TimeSpan.FromSeconds(1);
            }
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events

        public event UIKeypadButtonEvent ButtonEvent
        {
            add
            {
                if (_subscribeCount == 0)
                {
                    _buttons.ButtonEvent += OnButtonEvent;
                }
                _subscribeCount++;
                _buttonEvent += value;
            }
            remove
            {
                if (_subscribeCount == 0) return;
                _subscribeCount--;
                _buttonEvent -= value;
                if (_subscribeCount == 0)
                {
                    _buttons.ButtonEvent -= OnButtonEvent;
                }
            }
        }

        #endregion

        #region Delegates

        #endregion

        #region Properties

        #endregion

        #region Methods

        private void OnButtonEvent(IButton button, ButtonEventArgs args)
        {
            var type = (UIKeypadButtonType) args.CollectionKey;
            string stringValue;

            switch (type)
            {
                case UIKeypadButtonType.Star:
                    stringValue = "*";
                    break;
                case UIKeypadButtonType.Hash:
                    stringValue = "#";
                    break;
                default:
                    stringValue = args.CollectionKey.ToString(CultureInfo.InvariantCulture);
                    break;
            }

            if (_buttonEvent == null) return;

            CloudLog.Debug("{0} {1}: {2}", GetType().Name, args.EventType, (UIKeypadButtonType) args.CollectionKey);

            _buttonEvent(this, new UIKeypadButtonEventArgs()
            {
                EventType = args.EventType,
                KeypadButtonType = (UIKeypadButtonType) args.CollectionKey,
                StringValue = stringValue,
                Value = args.CollectionKey
            });
        }

        #endregion
    }

    public delegate void UIKeypadButtonEvent(UIKeypad keypad, UIKeypadButtonEventArgs args);

    public enum UIKeypadButtonType
    {
        Key0,
        Key1,
        Key2,
        Key3,
        Key4,
        Key5,
        Key6,
        Key7,
        Key8,
        Key9,
        Star,
        Hash
    }

    public class UIKeypadButtonEventArgs : EventArgs
    {
        public ButtonEventType EventType { get; internal set; }
        public UIKeypadButtonType KeypadButtonType { get; internal set; }
        public uint Value { get; internal set; }
        public string StringValue { get; internal set; }
    }
}