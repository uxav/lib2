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
using Crestron.SimplSharpPro.DeviceSupport;
using UX.Lib2.Cloud.Logger;

namespace UX.Lib2.UI
{
    public class UITextEntry : UILabel
    {
        public UITextEntry(BasicTriList device, uint serialJoinNumber, uint enterKeyJoinNumber) : base(device, serialJoinNumber)
        {
            SerialOutputJoin = device.StringOutput[serialJoinNumber];
            EnterJoin = device.BooleanOutput[enterKeyJoinNumber];
            RegisterToSigChanges();
        }

        public UITextEntry(UIController uiController, uint serialJoinNumber, uint enterKeyJoinNumber) : base(uiController, serialJoinNumber)
        {
            SerialOutputJoin = uiController.Device.StringOutput[serialJoinNumber];
            EnterJoin = uiController.Device.BooleanOutput[enterKeyJoinNumber];
            RegisterToSigChanges();
        }

        public UITextEntry(UIViewController viewController, uint serialJoinNumber, uint enterKeyJoinNumber)
            : base(viewController, serialJoinNumber)
        {
            SerialOutputJoin = viewController.UIController.Device.StringOutput[serialJoinNumber];
            EnterJoin = viewController.UIController.Device.BooleanOutput[enterKeyJoinNumber];
            RegisterToSigChanges();
        }

        public UITextEntry(SmartObject smartObject, string inputSerialJoinName, string outputSerialJoinName, uint enterKeyJoinName)
            : base(smartObject, inputSerialJoinName)
        {
            SerialOutputJoin = smartObject.StringOutput[outputSerialJoinName];
            EnterJoin = smartObject.BooleanOutput[enterKeyJoinName];
            RegisterToSigChanges();
        }

        public event UITextFieldKeyboardChangeEventHandler KeyboardDidEnter;
        public event UITextFieldKeyboardChangeEventHandler KeyboardTextChanged;

        public StringOutputSig SerialOutputJoin { get; private set; }

        public BoolOutputSig EnterJoin { get; private set; }

        /// <summary>
        /// Set or get the item text
        /// </summary>
        public override string Text
        {
            get { return base.Text; }
            set
            {
                _text = value;
                SerialInputJoin.StringValue = _text;
            }
        }

        protected override void OnSigChange(GenericBase currentDevice, SigEventArgs args)
        {
            base.OnSigChange(currentDevice, args);
            if (args.Event == eSigEvent.StringChange && args.Sig == SerialOutputJoin)
            {
#if DEBUG
                //CloudLog.Debug("Text Entry {0} Text Change: {1}", SerialOutputJoin.Number, args.Sig.StringValue);
#endif
                _text = args.Sig.StringValue;
                OnKeyboardTextChanged(_text);
            }
            else if (args.Event == eSigEvent.BoolChange && args.Sig == EnterJoin && args.Sig.BoolValue)
            {
#if DEBUG
                CloudLog.Debug("Text Entry {0} Entered Text: {1}", SerialOutputJoin.Number, Text);
#endif
                OnKeyboardDidEnter(Text);
            }
        }

        protected virtual void OnKeyboardDidEnter(string text)
        {
            var handler = KeyboardDidEnter;
            if (handler == null) return;
            try
            {
                handler(this, text);
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }
        }

        protected virtual void OnKeyboardTextChanged(string text)
        {
            var handler = KeyboardTextChanged;
            if (handler == null) return;
            try
            {
                handler(this, text);
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }
        }
    }

    public delegate void UITextFieldKeyboardChangeEventHandler(UITextEntry textEntry, string text);
}