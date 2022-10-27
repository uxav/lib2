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
using UX.Lib2.Models;

namespace UX.Lib2.UI
{
    public abstract class UIActionSheet
    {
        #region Fields

        private readonly UIControllerWithSmartObjects _uiController;
        private readonly UIViewController _view;
        private readonly UISubPageReferenceList _buttonList;
        private ButtonCollection _buttons = new ButtonCollection();
        private UserPrompt _prompt;
        private UIActionSheetResponse _callback;
        private int _timeOutTime;
        private int _secondsCount;
        private CTimer _timer;
        private readonly UILabel _titleLabel;
        private readonly UILabel _subtitleLabel;
        private Dictionary<uint, string> _buttonKeyNames = new Dictionary<uint, string>();
        private object _objRef;

        #endregion

        #region Constructors

        /// <summary>
        /// The default Constructor.
        /// </summary>
        protected UIActionSheet(UIControllerWithSmartObjects uiController, UIViewController view,
            SmartObject subPageReferenceListOfButtons, UILabel titleLabel, UILabel subtitleLabel)
        {
            _uiController = uiController;
            _view = view;
            _titleLabel = titleLabel;
            _subtitleLabel = subtitleLabel;
            _buttonList = new UIActionSheetButtonList(uiController, subPageReferenceListOfButtons);
            _view.VisibilityChanged += ViewOnVisibilityChanged;
            CancelButtonMode = UIActionSheetButtonMode.Red;
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events
        #endregion

        #region Delegates
        #endregion

        #region Properties

        public UIActionSheetButtonMode AcknowledgeButtonMode { get; set; }
        public UIActionSheetButtonMode CancelButtonMode { get; set; }

        public UIViewController View
        {
            get { return _view; }
        }

        public TimeSpan TimeRemaining
        {
            get
            {
                return _prompt != null
                    ? TimeSpan.FromSeconds(_prompt.SecondsRemaining)
                    : TimeSpan.FromSeconds(_timeOutTime - _secondsCount);
            }
        }

        #endregion

        #region Methods

        public void SetTimeOutInSeconds(int seconds)
        {
            _timeOutTime = seconds;
        }

        public void Show(UserPrompt prompt)
        {
            _prompt = prompt;
            _timeOutTime = 0;
            _callback = null;
            _objRef = null;

            if (_titleLabel != null)
                _titleLabel.Text = prompt.Title;

            if (_subtitleLabel != null)
                _subtitleLabel.Text = prompt.SubTitle;

            _buttonList.ClearList();
            foreach (var action in prompt.Actions)
            {
                var i = _buttonList.AddItem(action.ActionName, action);
                switch (action.ActionType)
                {
                    case PromptActionType.Acknowledge:
                        ((UIActionSheetButtonListItem) _buttonList[i]).ButtonMode = AcknowledgeButtonMode;
                        break;
                    case PromptActionType.Cancel:
                        ((UIActionSheetButtonListItem)_buttonList[i]).ButtonMode = CancelButtonMode;
                        break;
                    case PromptActionType.Reject:
                        ((UIActionSheetButtonListItem)_buttonList[i]).ButtonMode = UIActionSheetButtonMode.Red;
                        break;
                    case PromptActionType.Answer:
                        ((UIActionSheetButtonListItem)_buttonList[i]).ButtonMode = UIActionSheetButtonMode.Green;
                        break;
                }
            }
            
            _buttons = new ButtonCollection();
            foreach (var button in _buttonList)
            {
                _buttons.Add(button.Index, button);
            }

            _prompt.StateChanged += (userPrompt, state) =>
            {
                if(state != PromptState.Shown)
                    _view.Hide();
            };

            _view.Show();
        }

        public void Show(UIActionSheetResponse responseCallBack, string title, string subtitle, Dictionary<string, string> buttonKeyTitleValues)
        {
            if (_view.Visible) return;

            _prompt = null;
            _objRef = null;
            _callback = responseCallBack;
            if (_titleLabel != null)
                _titleLabel.Text = title;
            if (_subtitleLabel != null)
                _subtitleLabel.Text = subtitle;
            _buttonList.ClearList();
            _buttonKeyNames.Clear();
            foreach (var buttonTitle in buttonKeyTitleValues)
            {
                var index = _buttonList.AddItem(buttonTitle.Value, UIActionSheetResponseType.Acknowledged);
                _buttonKeyNames[index] = buttonTitle.Key;
            }

            _buttons = new ButtonCollection();
            foreach (var button in _buttonList)
            {
                _buttons.Add(button.Index, button);
            }

            _view.Show();
        }

        public void Show(UIActionSheetResponse responseCallBack, string title, string subtitle, string[] buttonTitles)
        {
            var dict = new Dictionary<string, string>();
            foreach (var buttonTitle in buttonTitles)
            {
                dict[buttonTitle] = buttonTitle;
            }
            Show(responseCallBack, title, subtitle, dict);
        }

        public void Show(UIActionSheetResponse responseCallBack, string title, string subtitle, string acknowledgeButtonTitle)
        {
            if(_view.Visible) return;

            _prompt = null;
            _objRef = null;
            _callback = responseCallBack;
            if (_titleLabel != null)
                _titleLabel.Text = title;
            if (_subtitleLabel != null)
                _subtitleLabel.Text = subtitle;
            _buttonList.ClearList();
            _buttonKeyNames.Clear();
            var index = _buttonList.AddItem(acknowledgeButtonTitle, UIActionSheetResponseType.Acknowledged);
            _buttonKeyNames[index] = "Acknowledge";
            _buttons = new ButtonCollection();
            foreach (var button in _buttonList)
            {
                _buttons.Add(button.Index, button);
            }

            _view.Show();
        }

        public void Show(UIActionSheetResponse responseCallBack, string title, string subtitle,
            string acknowledgeButtonTitle, string cancelButtonTitle)
        {
            if (_view.Visible) return;

            _prompt = null;
            _objRef = null;
            _callback = responseCallBack;
            if (_titleLabel != null)
                _titleLabel.Text = title;
            if (_subtitleLabel != null)
                _subtitleLabel.Text = subtitle;
            _buttonList.ClearList();
            _buttonKeyNames.Clear();
            var i = _buttonList.AddItem(cancelButtonTitle, UIActionSheetResponseType.Cancelled);
            ((UIActionSheetButtonListItem) _buttonList[i]).ButtonMode = CancelButtonMode;
            _buttonKeyNames[i] = "Cancel";
            i = _buttonList.AddItem(acknowledgeButtonTitle, UIActionSheetResponseType.Acknowledged);
            ((UIActionSheetButtonListItem) _buttonList[i]).ButtonMode = AcknowledgeButtonMode;
            _buttonKeyNames[i] = "Acknowledge";

            _buttons = new ButtonCollection();
            foreach (var button in _buttonList)
            {
                _buttons.Add(button.Index, button);
            }

            _view.Show();
        }

        public void Show(UIActionSheetResponse responseCallBack, string title, string subtitle, string cancelButtonTitle,
            params string[] otherButtons)
        {
            if (_view.Visible) return;

            _prompt = null;
            _objRef = null;
            _callback = responseCallBack;
            if (_titleLabel != null)
                _titleLabel.Text = title;
            if (_subtitleLabel != null)
                _subtitleLabel.Text = subtitle;
            _buttonList.ClearList();
            _buttonKeyNames.Clear();
            uint i;
            foreach (var otherButton in otherButtons)
            {
                i = _buttonList.AddItem(otherButton, null);
                ((UIActionSheetButtonListItem) _buttonList[i]).ButtonMode = AcknowledgeButtonMode;
                _buttonKeyNames[i] = otherButton;
            }
            i = _buttonList.AddItem(cancelButtonTitle, UIActionSheetResponseType.Cancelled);
            ((UIActionSheetButtonListItem) _buttonList[i]).ButtonMode = CancelButtonMode;
            _buttonKeyNames[i] = "Cancel";

            _buttons = new ButtonCollection();
            foreach (var button in _buttonList)
            {
                _buttons.Add(button.Index, button);
            }

            _view.Show();
        }

        public void Show(UIActionSheetResponse responseCallBack, string title, string subtitle, 
            string acknowledgeButtonTitle, string cancelButtonTitle, object objRef)
        {
            Show(responseCallBack, title, subtitle, acknowledgeButtonTitle, cancelButtonTitle);
            _objRef = objRef;
        }


        public void Cancel()
        {
            if(!_view.Visible) return;

            _view.Hide();

            if(_callback == null) return;

            try
            {
                _callback(UIActionSheetResponseType.Cancelled, new UIActionSheetResponseArgs());
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }
        }

        protected virtual void ViewOnVisibilityChanged(IVisibleItem item, VisibilityChangeEventArgs args)
        {
#if DEBUG
            Debug.WriteInfo(GetType().Name, args.EventType.ToString());
#endif
            switch (args.EventType)
            {
                case VisibilityChangeEventType.WillShow:
                    _secondsCount = 0;
                    _timer = new CTimer(CountStep, null, 1000, 1000);
                    _buttons.ButtonEvent += ButtonsOnButtonEvent;
                    break;
                case VisibilityChangeEventType.WillHide:
                    _buttons.ButtonEvent -= ButtonsOnButtonEvent;
                    _timer.Stop();
                    _timer.Dispose();
                    break;
            }
        }

        private void ButtonsOnButtonEvent(IButton button, ButtonEventArgs args)
        {
            if (args.EventType != ButtonEventType.Released) return;

            var index = args.CollectionKey;
#if DEBUG
            Debug.WriteInfo("ActionSheet Button Selected", "{0} \"{1}\"",
                index, _buttonList[index].Text);
#endif
            _view.Hide();

            if (_prompt != null)
            {
                _prompt.Respond(_buttonList[index].LinkedObject as PromptAction);

                return;
            }

            var type = UIActionSheetResponseType.Acknowledged;

            if (_buttonList[index].LinkedObject != null)
            {
                type = (UIActionSheetResponseType) _buttonList[index].LinkedObject;
            }

            if (_callback == null) return;

            try
            {
                _callback(type, new UIActionSheetResponseArgs()
                {
                    ButtonIndex = index,
                    ButtonTitle = _buttonList[index].Text,
                    ButtonKeyName = _buttonKeyNames[index],
                    Ref = _objRef
                });
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }
        }

        private void CountStep(object userSpecific)
        {
            _secondsCount ++;

            if(_timeOutTime == 0) return;

            if (_secondsCount < _timeOutTime) return;

            _view.Hide();

            if (_callback == null) return;

            try
            {
                _callback(UIActionSheetResponseType.TimedOut, new UIActionSheetResponseArgs());
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }
        }

        #endregion
    }

    public class UIActionSheetResponseArgs : EventArgs
    {
        public UIActionSheetResponseArgs()
        {
            ButtonTitle = string.Empty;
            ButtonKeyName = string.Empty;
        }
        public uint ButtonIndex { get; internal set; }
        public string ButtonTitle { get; internal set; }
        public string ButtonKeyName { get; internal set; }
        public object Ref { get; internal set; }
    }

    public delegate void UIActionSheetResponse(UIActionSheetResponseType responseType, UIActionSheetResponseArgs args);

    public enum UIActionSheetResponseType
    {
        Cancelled,
        TimedOut,
        Acknowledged
    }

    public enum UIActionSheetButtonMode
    {
        Normal,
        Green,
        Red,
        Yellow
    }
}