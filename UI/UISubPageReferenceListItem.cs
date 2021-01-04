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
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using UX.Lib2.Cloud.Logger;

namespace UX.Lib2.UI
{
    public abstract class UISubPageReferenceListItem : UIObject, IButton
    {
        #region Fields

        private readonly UISubPageReferenceList _list;
        private readonly uint _index;
        private readonly ReadOnlyDictionary<uint, BoolInputSig> _boolInputSigs;
        private readonly ReadOnlyDictionary<uint, BoolOutputSig> _boolOutputSigs;
        private readonly ReadOnlyDictionary<uint, StringInputSig> _stringInputSigs;
        private readonly ReadOnlyDictionary<uint, StringOutputSig> _stringOutputSigs;
        private readonly ReadOnlyDictionary<uint, UShortInputSig> _uShortInputSigs;
        private readonly ReadOnlyDictionary<uint, UShortOutputSig> _uShortOutputSigs;
        private readonly UIButton _button;
// ReSharper disable once InconsistentNaming
        private event ButtonEventHandler _buttonEvent;
        private bool _buttonSubscribed;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a base class for subpage reference list items
        /// </summary>
        /// <param name="list">The subpage list</param>
        /// <param name="index">The index of the item</param>
        protected UISubPageReferenceListItem(UISubPageReferenceList list, uint index)
            : base(list.SmartObject)
        {
            try
            {
                _list = list;
                _index = index;

                VisibleJoin = list.SmartObject.BooleanInput[string.Format("Item {0} Visible", index)];
                EnableJoin = list.SmartObject.BooleanInput[string.Format("Item {0} Enable", index)];

                if (list.DigitalJoinIncrement > 0)
                {
                    uint count = 0;
                    var inputSigs = new Dictionary<uint, BoolInputSig>();
                    var outputSigs = new Dictionary<uint, BoolOutputSig>();
                    for (var i = index*list.DigitalJoinIncrement - (list.DigitalJoinIncrement - 1);
                        i <= index*list.DigitalJoinIncrement;
                        i++)
                    {
                        count ++;
                        inputSigs[count] = list.SmartObject.BooleanInput[string.Format("fb{0}", i)];
                        outputSigs[count] = list.SmartObject.BooleanOutput[string.Format("press{0}", i)];
                    }
                    _boolInputSigs = new ReadOnlyDictionary<uint, BoolInputSig>(inputSigs);
                    _boolOutputSigs = new ReadOnlyDictionary<uint, BoolOutputSig>(outputSigs);
                }

                if (list.AnalogJoinIncrement > 0)
                {
                    uint count = 0;
                    var inputSigs = new Dictionary<uint, UShortInputSig>();
                    var outputSigs = new Dictionary<uint, UShortOutputSig>();
                    for (var i = index*list.AnalogJoinIncrement - (list.AnalogJoinIncrement - 1);
                        i <= index*list.AnalogJoinIncrement;
                        i++)
                    {
                        count++;
                        inputSigs[count] = list.SmartObject.UShortInput[string.Format("an_fb{0}", i)];
                        outputSigs[count] = list.SmartObject.UShortOutput[string.Format("an_act{0}", i)];
                    }
                    _uShortInputSigs = new ReadOnlyDictionary<uint, UShortInputSig>(inputSigs);
                    _uShortOutputSigs = new ReadOnlyDictionary<uint, UShortOutputSig>(outputSigs);
                }

                if (list.SerialJoinIncrement > 0)
                {
                    uint count = 0;
                    var inputSigs = new Dictionary<uint, StringInputSig>();
                    var outputSigs = new Dictionary<uint, StringOutputSig>();
                    for (var i = index*list.SerialJoinIncrement - (list.SerialJoinIncrement - 1);
                        i <= index*list.SerialJoinIncrement;
                        i++)
                    {
                        count++;
                        inputSigs[count] = list.SmartObject.StringInput[string.Format("text-o{0}", i)];
                        outputSigs[count] = list.SmartObject.StringOutput[string.Format("text-i{0}", i)];
                    }
                    _stringInputSigs = new ReadOnlyDictionary<uint, StringInputSig>(inputSigs);
                    _stringOutputSigs = new ReadOnlyDictionary<uint, StringOutputSig>(outputSigs);
                }

                _button = new UIButton(list.SmartObject, BoolOutputSigs[1].Name, BoolInputSigs[1].Name,
                    StringInputSigs[1].Name);
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

        protected virtual void OnVisibilityChanged(IVisibleItem item, VisibilityChangeEventArgs args)
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

        public UISubPageReferenceList List
        {
            get { return _list; }
        }

        public ReadOnlyDictionary<uint, BoolInputSig> BoolInputSigs
        {
            get { return _boolInputSigs; }
        }

        protected ReadOnlyDictionary<uint, BoolOutputSig> BoolOutputSigs
        {
            get { return _boolOutputSigs; }
        }

        public ReadOnlyDictionary<uint, StringInputSig> StringInputSigs
        {
            get { return _stringInputSigs; }
        }

        protected ReadOnlyDictionary<uint, StringOutputSig> StringOutputSigs
        {
            get { return _stringOutputSigs; }
        }

        public ReadOnlyDictionary<uint, UShortInputSig> UShortInputSigs
        {
            get { return _uShortInputSigs; }
        }

        protected ReadOnlyDictionary<uint, UShortOutputSig> UShortOutputSigs
        {
            get { return _uShortOutputSigs; }
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

        public virtual object LinkedObject { get; set; }

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

        public virtual bool Feedback
        {
            get { return DigitalFeedbackJoin.BoolValue; }
            set { SetFeedback(value); }
        }

        public StringInputSig SerialInputJoin
        {
            get { return _button.SerialInputJoin; }
        }

        public virtual string Text
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

        public bool IsLastItem
        {
            get { return _index == _list.ItemsAddedCount; }
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

        public virtual void SetFeedback(bool value)
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

        public void SetText(IFormatProvider provider, string format, params object[] args)
        {
            _button.SetText(provider, format, args);
        }

        protected void OnButtonEvent(IButton button, ButtonEventArgs args)
        {
            var handler = _buttonEvent;
            if (handler != null) handler(this, args);
        }

        #endregion

    }
}