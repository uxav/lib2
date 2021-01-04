using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using UX.Lib2.Cloud.Logger;

namespace UX.Lib2.UI
{
    /// <summary>
    /// A basic button for a UI
    /// </summary>
    public class UIButton : UIObject, IButton
    {
        #region Fields

        private readonly Stopwatch _pressTimer = new Stopwatch();
        private CTimer _pressCheckTimeTimer;
        private TimeSpan _holdTime = TimeSpan.FromSeconds(2);
        private int _subscribeCount;
        private ButtonEventHandler _buttonEvent;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a basic button
        /// </summary>
        /// <param name="viewController">The View Controller for the button</param>
        /// <param name="pressJoinNumber">The join number for press and feedback</param>
        public UIButton(UIViewController viewController, uint pressJoinNumber)
            : this(viewController.UIController.Device, pressJoinNumber)
        {
        }

        /// <summary>
        /// Create a basic button with dynamic text
        /// </summary>
        /// <param name="viewController">The View Controller for the button</param>
        /// <param name="pressJoinNumber">The join number for press and feedback</param>
        /// <param name="serialJoinNumber">The join number for the text input</param>
        public UIButton(UIViewController viewController, uint pressJoinNumber, uint serialJoinNumber)
            : this(viewController.UIController.Device, pressJoinNumber, serialJoinNumber)
        {
        }

        /// <summary>
        /// Create a basic button
        /// </summary>
        /// <param name="uiController">The UI Controller for the button</param>
        /// <param name="pressJoinNumber">The join number for press and feedback</param>
        public UIButton(UIController uiController, uint pressJoinNumber)
            : this(uiController.Device, pressJoinNumber)
        {
        }

        /// <summary>
        /// Create a basic button with dynamic text
        /// </summary>
        /// <param name="uiController">The UI Controller for the button</param>
        /// <param name="pressJoinNumber">The join number for press and feedback</param>
        /// <param name="serialJoinNumber">The join number for the text input</param>
        public UIButton(UIController uiController, uint pressJoinNumber, uint serialJoinNumber)
            : this(uiController.Device, pressJoinNumber, serialJoinNumber)
        {
        }

        /// <summary>
        /// Create a basic button
        /// </summary>
        /// <param name="device">The device for the button</param>
        /// <param name="pressJoinNumber">The join number for press and feedback</param>
        public UIButton(BasicTriList device, uint pressJoinNumber)
            : base(device)
        {
            DigitalPressJoin = device.BooleanOutput[pressJoinNumber];
            DigitalFeedbackJoin = device.BooleanInput[pressJoinNumber];
        }

        /// <summary>
        /// Create a basic button with dynamic text
        /// </summary>
        /// <param name="device">The device for the button</param>
        /// <param name="pressJoinNumber">The join number for press and feedback</param>
        /// <param name="serialJoinNumber">The join number for the text input</param>
        public UIButton(BasicTriList device, uint pressJoinNumber, uint serialJoinNumber)
            : this(device, pressJoinNumber)
        {
            SerialInputJoin = device.StringInput[serialJoinNumber];
        }

        /// <summary>
        /// Create a basic smart object button
        /// </summary>
        /// <param name="smartObject">The smartobject extender</param>
        /// <param name="pressJoinNumber">The join number of the smart object push</param>
        public UIButton(SmartObject smartObject, uint pressJoinNumber)
            : base(smartObject)
        {
            DigitalPressJoin = smartObject.BooleanOutput[pressJoinNumber];
        }

        /// <summary>
        /// Create a basic smart object button
        /// </summary>
        /// <param name="smartObject">The smartobject extender</param>
        /// <param name="pressJoinName">The join name of the smart object push</param>
        /// <param name="feedbackJoinName">The join name of the smart object feedback</param>
        public UIButton(SmartObject smartObject, string pressJoinName, string feedbackJoinName)
            : base(smartObject)
        {
            DigitalPressJoin = smartObject.BooleanOutput[pressJoinName];
            DigitalFeedbackJoin = smartObject.BooleanInput[feedbackJoinName];
        }

        /// <summary>
        /// Create a basic smart object button with dynamic text
        /// </summary>
        /// <param name="smartObject">The smartobject extender</param>
        /// <param name="pressJoinName">The join name of the smart object push</param>
        /// <param name="feedbackJoinName">The join name of the smart object feedback</param>
        /// <param name="serialJoinName">The set text join name of the smart object</param>
        public UIButton(SmartObject smartObject, string pressJoinName, string feedbackJoinName, string serialJoinName)
            : this(smartObject, pressJoinName, feedbackJoinName)
        {
            SerialInputJoin = smartObject.StringInput[serialJoinName];
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events

        /// <summary>
        /// Subscribe to button events
        /// </summary>
        public event ButtonEventHandler ButtonEvent
        {
            add
            {
                if(_subscribeCount == 0)
                    RegisterToSigChanges();
                _subscribeCount ++;
                _buttonEvent += value;
            }
            remove
            {
                if(_subscribeCount == 0) return;
                _subscribeCount --;
                _buttonEvent -= value;
                if (_subscribeCount == 0)
                {
                    UnregisterToSigChanges();
                }
            }
        }

        /// <summary>
        /// Subscribe to visibility change events
        /// </summary>
        public event VisibilityChangeEventHandler VisibilityChanged;

        #endregion

        #region Delegates
        #endregion

        #region Properties

        /// <summary>
        /// True if button is currently in a pressed state
        /// </summary>
        public bool IsPressed
        {
            get { return DigitalPressJoin.BoolValue; }
        }

        /// <summary>
        /// Set or get the current digital feedback state
        /// </summary>
        public bool Feedback
        {
            get { return DigitalFeedbackJoin.BoolValue; }
            set { DigitalFeedbackJoin.BoolValue = value; }
        }

        /// <summary>
        /// The digital join to the device or smartobject
        /// </summary>
        public BoolInputSig DigitalFeedbackJoin { get; private set; }

        /// <summary>
        /// The digital join from the device or smartobject
        /// </summary>
        public BoolOutputSig DigitalPressJoin { get; private set; }

        /// <summary>
        /// Set or get the time for the button to trigger a hold event
        /// </summary>
        public TimeSpan HoldTime
        {
            get { return _holdTime; }
            set { _holdTime = value; }
        }

        /// <summary>
        /// Set or get the item text
        /// </summary>
        public string Text
        {
            get
            {
                return SerialInputJoin == null ? string.Empty : SerialInputJoin.StringValue;
            }
            set
            {
                if (SerialInputJoin != null)
                    SerialInputJoin.StringValue = value;
            }
        }

        /// <summary>
        /// The serial join to the device or smartobject
        /// </summary>
        public StringInputSig SerialInputJoin { get; private set; }

        /// <summary>
        /// True if the item is visible
        /// </summary>
        public virtual bool Visible
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

        public bool RequestedVisibleState { get; protected set; }

        /// <summary>
        /// The digital visibility join to the device or smartobject for
        /// </summary>
        public BoolInputSig VisibleJoin { get; set; }

        /// <summary>
        /// True if enabled
        /// </summary>
        public bool Enabled
        {
            get { return EnableJoin == null || EnableJoin.BoolValue; }
            set
            {
                if (EnableJoin != null)
                    EnableJoin.BoolValue = value;
            }
        }

        /// <summary>
        /// The UI item enable digital join
        /// </summary>
        public BoolInputSig EnableJoin { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Set the feedback state
        /// </summary>
        /// <param name="value">The high / low state of the button</param>
        public void SetFeedback(bool value)
        {
            Feedback = value;
        }

        /// <summary>
        /// Set the text
        /// </summary>
        /// <param name="text">The text to set the item with</param>
        public void SetText(string text)
        {
            Text = text;
        }

        /// <summary>
        /// Set the text, formatted with parameters
        /// </summary>
        /// <param name="text">The format string</param>
        /// <param name="args">The parameters</param>
        public void SetText(string text, params object[] args)
        {
            Text = string.Format(text, args);
        }

        /// <summary>
        /// Set the text, formatted with parameters
        /// </summary>
        /// <param name="provider">Format provider</param>
        /// <param name="format">The format string</param>
        /// <param name="args">The parameters</param>
        public void SetText(IFormatProvider provider, string format, params object[] args)
        {
            Text = string.Format(provider, format, args);
        }

        /// <summary>
        /// Make the item visible
        /// </summary>
        public void Show()
        {
            Visible = true;
        }

        /// <summary>
        /// Make the item hidden
        /// </summary>
        public void Hide()
        {
            Visible = false;
        }

        /// <summary>
        /// Enable the UI item
        /// </summary>
        public void Enable()
        {
            if (EnableJoin != null)
                EnableJoin.BoolValue = true;
        }

        /// <summary>
        /// Disable the UI item
        /// </summary>
        public void Disable()
        {
            if (EnableJoin != null)
                EnableJoin.BoolValue = false;
        }

        protected override void OnSigChange(GenericBase owner, SigEventArgs args)
        {
            if (args.Sig != DigitalPressJoin || args.Event != eSigEvent.BoolChange) return;

            if (args.Sig.BoolValue)
            {
                _pressTimer.Start();
                _pressCheckTimeTimer = new CTimer(specific =>
                {
                    if (_subscribeCount == 0)
                    {
                        _pressCheckTimeTimer.Stop();
                        _pressTimer.Stop();
                        _pressTimer.Reset();
                        return;
                    }
                    if (_pressTimer.Elapsed < HoldTime) return;
                    _pressCheckTimeTimer.Stop();
                    OnButtonEvent(this, new ButtonEventArgs(ButtonEventType.Held, _pressTimer.Elapsed));
                }, null, 100, 100);
                OnButtonEvent(this, new ButtonEventArgs(ButtonEventType.Pressed, _pressTimer.Elapsed));
            }
            else
            {
                _pressCheckTimeTimer.Dispose();
                _pressTimer.Stop();
                if(_pressTimer.Elapsed < HoldTime)
                    OnButtonEvent(this, new ButtonEventArgs(ButtonEventType.Tapped, _pressTimer.Elapsed));
                OnButtonEvent(this, new ButtonEventArgs(ButtonEventType.Released, _pressTimer.Elapsed));
                _pressTimer.Reset();
            }
        }

        protected virtual void OnButtonEvent(IButton button, ButtonEventArgs args)
        {
#if DEBUG
            Debug.WriteInfo("{0}, {1} {2}", this, args.EventType, args.HoldTime);
#endif
            var handler = _buttonEvent;
            try
            {
                if (handler != null) handler(button, args);
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }
        }

        protected virtual void OnVisibilityChanged(IVisibleItem item, VisibilityChangeEventArgs args)
        {
            var handler = VisibilityChanged;
            if (handler != null) handler(item, args);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                
            }

            if(_pressCheckTimeTimer != null && !_pressCheckTimeTimer.Disposed)
                _pressCheckTimeTimer.Dispose();
            
            base.Dispose(disposing);
        }

        public override string ToString()
        {
            return string.Format("Button {0} for {1}", DigitalPressJoin.Number, Owner);
        }

        #endregion
    }
}