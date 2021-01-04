using System;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace UX.Lib2.UI
{
    public class UILabel : UIObject, ITextItem, IVisibleItem, IEnableItem
    {
        #region Fields

        protected string _text = string.Empty;
        protected readonly IVisibleItem _owner;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a basic dynamic UI Text Label
        /// </summary>
        /// <param name="device">The panel device or gateway</param>
        /// <param name="serialJoinNumber"></param>
        public UILabel(BasicTriList device, uint serialJoinNumber)
            : base(device)
        {
            SerialInputJoin = device.StringInput[serialJoinNumber];
        }

        /// <summary>
        /// Create a basic dynamic UI Text Label
        /// </summary>
        /// <param name="uiController">The UI Controller</param>
        /// <param name="serialJoinNumber"></param>
        public UILabel(UIController uiController, uint serialJoinNumber)
            : this(uiController.Device, serialJoinNumber)
        {
        }

        /// <summary>
        /// Create a basic dynamic UI Text Label. This sets automatically again on owner visibility.
        /// </summary>
        /// <param name="viewController">The view controller containting the item</param>
        /// <param name="serialJoinNumber"></param>
        public UILabel(UIViewController viewController, uint serialJoinNumber)
            : this(viewController.UIController.Device, serialJoinNumber)
        {
            _owner = viewController;
            _owner.VisibilityChanged += OwnerOnVisibilityChanged;
        }

        public UILabel(SmartObject smartObject, string serialJoinName)
            : base(smartObject)
        {
            SerialInputJoin = smartObject.StringInput[serialJoinName];
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events
        
        public event VisibilityChangeEventHandler VisibilityChanged;

        #endregion

        #region Delegates
        #endregion

        #region Properties

        /// <summary>
        /// The serial join to the device or smartobject
        /// </summary>
        public StringInputSig SerialInputJoin { get; private set; }

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

        public BoolInputSig VisibleJoin { get; set; }

        public BoolInputSig EnableJoin { get; set; }

        public bool Enabled
        {
            get { return EnableJoin == null || EnableJoin.BoolValue; }
            set
            {
                if (EnableJoin != null)
                    EnableJoin.BoolValue = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Set or get the item text
        /// </summary>
        public virtual string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                if (_owner == null || _owner.Visible)
                    SerialInputJoin.StringValue = _text;
            }
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

        public void Append(char c)
        {
            Text = Text + c;
        }

        public void Append(string s)
        {
            Text = Text + s;
        }

        private void OwnerOnVisibilityChanged(IVisibleItem item, VisibilityChangeEventArgs args)
        {
            if (args.EventType != VisibilityChangeEventType.WillShow) return;
            SerialInputJoin.StringValue = _text;
        }

        protected override void OnSigChange(GenericBase owner, SigEventArgs args)
        {
            
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                
            }

            if (_owner != null)
                _owner.VisibilityChanged -= OwnerOnVisibilityChanged;

            base.Dispose(disposing);
        }

        protected virtual void OnVisibilityChanged(IVisibleItem item, VisibilityChangeEventArgs args)
        {
            var handler = VisibilityChanged;
            if (handler != null) handler(item, args);
        }

        public void Show()
        {
            Visible = true;
        }

        public void Hide()
        {
            Visible = false;
        }

        public void Clear()
        {
            Text = string.Empty;
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

        #endregion
    }
}