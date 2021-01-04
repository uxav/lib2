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
using Crestron.SimplSharpPro;
using UX.Lib2.Cloud.Logger;

namespace UX.Lib2.UI
{
    public sealed class UIDynamicButtonListItem : UIObject, IButton
    {
        #region Fields

        private readonly UIDynamicButtonList _list;
        private readonly uint _index;
        private readonly UIButton _button;
// ReSharper disable once InconsistentNaming
        private event ButtonEventHandler _buttonEvent;
        private bool _buttonSubscribed;
        private readonly UShortInputSig _iconAnalogSig;
        private readonly StringInputSig _iconSerialSig;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a base class for subpage reference list items
        /// </summary>
        /// <param name="list">The subpage list</param>
        /// <param name="index">The index of the item</param>
        internal UIDynamicButtonListItem(UIDynamicButtonList list, uint index)
            : base(list.SmartObject)
        {
            try
            {
                _list = list;
                _index = index;

                VisibleJoin = list.SmartObject.BooleanInput[string.Format("Item {0} Visible", index)];
                EnableJoin = list.SmartObject.BooleanInput[string.Format("Item {0} Enabled", index)];

                var boolInputSig = list.SmartObject.BooleanInput[string.Format("Item {0} Selected", index)];
                var boolOutputSig = list.SmartObject.BooleanOutput[string.Format("Item {0} Pressed", index)];
                _iconAnalogSig = list.SmartObject.UShortInput[string.Format("Set Item {0} Icon Analog", index)];
                _iconSerialSig = list.SmartObject.StringInput[string.Format("Set Item {0} Icon Serial", index)];
                var nameStringSig = list.SmartObject.StringInput[string.Format("Set Item {0} Text", index)];

                _button = new UIButton(list.SmartObject, boolOutputSig.Name, boolInputSig.Name,
                    nameStringSig.Name);
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events

        public event VisibilityChangeEventHandler VisibilityChanged;

        private void OnVisibilityChanged(IVisibleItem item, VisibilityChangeEventArgs args)
        {
            var handler = VisibilityChanged;
            if (handler != null) handler(item, args);
        }

        public event ButtonEventHandler ButtonEvent
        {
            add
            {
                _buttonEvent += value;
                if (_buttonSubscribed) return;
                _button.ButtonEvent += OnButtonEvent;
                _buttonSubscribed = true;
            }
            remove
            {
// ReSharper disable once DelegateSubtraction
                _buttonEvent -= value;
                if (!_buttonSubscribed) return;
                _button.ButtonEvent -= OnButtonEvent;
                _buttonSubscribed = false;
            }
        }

        #endregion

        #region Delegates
        #endregion

        #region Properties

        public UIController UIController
        {
            get { return _list.UIController; }
        }

        public UIDynamicButtonList List
        {
            get { return _list; }
        }

        public ushort IconNumber
        {
            get { return _iconAnalogSig.UShortValue; }
            set { _iconAnalogSig.UShortValue = value; }
        }

        public string IconName
        {
            get { return _iconSerialSig.StringValue; }
            set { _iconSerialSig.StringValue = value; }
        }

        public uint Index
        {
            get { return _index; }
        }

        public BoolInputSig EnableJoin { get; private set; }

        public bool Enabled
        {
            get { return EnableJoin == null || EnableJoin.BoolValue; }
            set
            {
                if(value) Enable();
                else Disable();
            }
        }

        public BoolInputSig VisibleJoin { get; set; }

        public bool Visible
        {
            get { return VisibleJoin == null || VisibleJoin.BoolValue; }
            set
            {
                if (VisibleJoin == null || VisibleJoin.BoolValue == value) return;

                RequestedVisibleState = value;

                OnVisibilityChanged(this,
                    new VisibilityChangeEventArgs(value, value
                        ? VisibilityChangeEventType.WillShow
                        : VisibilityChangeEventType.WillHide));

                VisibleJoin.BoolValue = value;

                OnVisibilityChanged(this,
                    new VisibilityChangeEventArgs(value, value
                        ? VisibilityChangeEventType.DidShow
                        : VisibilityChangeEventType.DidHide));
            }
        }

        public bool RequestedVisibleState { get; private set; }

        public object LinkedObject { get; set; }

        public BoolOutputSig DigitalPressJoin
        {
            get { return _button.DigitalPressJoin; }
        }

        public bool IsPressed
        {
            get { return _button.IsPressed; }
        }

        public TimeSpan HoldTime
        {
            get { return _button.HoldTime; }
            set { _button.HoldTime = value; }
        }

        public BoolInputSig DigitalFeedbackJoin
        {
            get { return _button.DigitalFeedbackJoin; }
        }

        public bool Feedback
        {
            get { return DigitalPressJoin.BoolValue; }
            set { DigitalFeedbackJoin.BoolValue = value; }
        }

        public StringInputSig SerialInputJoin
        {
            get { return _button.SerialInputJoin; }
        }

        public string Text
        {
            get { return _button.Text; }
            set { _button.Text = value; }
        }

        /// <summary>
        /// Get the underlying button used for the item button behaviour
        /// </summary>
        public UIButton Button
        {
            get { return _button; }
        }

        #endregion

        #region Methods

        protected override void OnSigChange(GenericBase owner, SigEventArgs args)
        {

        }

        public void Show()
        {
            Visible = true;
        }

        public void Hide()
        {
            Visible = false;
        }

        public void Enable()
        {
            if (EnableJoin != null)
                EnableJoin.BoolValue = true;
        }

        public void Disable()
        {
            if (EnableJoin != null)
                EnableJoin.BoolValue = false;
        }

        public void SetFeedback(bool value)
        {
            _button.SetFeedback(value);
        }

        public void SetText(string text)
        {
            _button.SetText(text);
        }

        public void SetText(string text, params object[] args)
        {
            _button.SetText(text, args);
        }

        public void SetIcon(ushort icon)
        {
            List.SmartObject.UShortInput[string.Format("Set Item {0} Icon Analog", _index)].UShortValue = icon;
        }

        public void SetText(IFormatProvider provider, string format, params object[] args)
        {
            _button.SetText(provider, format, args);
        }

        private void OnButtonEvent(IButton button, ButtonEventArgs args)
        {
            var handler = _buttonEvent;
            if (handler != null) handler(this, args);
        }

        #endregion

    }
}