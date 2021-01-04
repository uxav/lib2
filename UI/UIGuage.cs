using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using UX.Lib2.Models;

namespace UX.Lib2.UI
{
    public class UIGuage : UIObject, IAnalogFeedback, IVisibleItem
    {
        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public UIGuage(BasicTriList device, uint analogFeedbackNumber)
            : base(device)
        {
            AnalogFeedbackJoin = device.UShortInput[analogFeedbackNumber];
        }

        public UIGuage(UIController uiController, uint analogFeedbackNumber)
            : this(uiController.Device, analogFeedbackNumber)
        {
        }

        public UIGuage(UIViewController view, uint analogFeedbackNumber)
            : this(view.UIController.Device, analogFeedbackNumber)
        {
        }

        public UIGuage(SmartObject smartObject, string analogFeedbackJoinName)
            : base(smartObject)
        {
            AnalogFeedbackJoin = smartObject.UShortInput[analogFeedbackJoinName];
        }

        #endregion

        #region Events

        /// <summary>
        /// Subscribe to visibility change events
        /// </summary>
        public event VisibilityChangeEventHandler VisibilityChanged;

        #endregion

        #region Properties

        public virtual ushort AnalogValue {
            get { return AnalogFeedbackJoin.UShortValue; }
            set { AnalogFeedbackJoin.UShortValue = value; }
        }

        public UShortInputSig AnalogFeedbackJoin { get; private set; }

        /// <summary>
        /// True if the item is visible
        /// </summary>
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

        public bool IsRamping
        {
            get { return AnalogFeedbackJoin.IsRamping; }
        }

        #endregion

        #region Methods

        public void SetValue(ushort value)
        {
            AnalogValue = value;
        }

        public void SetValue(IAudioLevelControl control, ushort value)
        {
            AnalogValue = value;
        }

        protected override void OnSigChange(GenericBase owner, SigEventArgs args)
        {

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

        #endregion
    }
}