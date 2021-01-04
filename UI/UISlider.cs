using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace UX.Lib2.UI
{
    public class UISlider : UIGuage, IAnalogTouch, IDigitalPress
    {
        #region Fields

        private AnalogTouchValueChangeHandler _analogValueChanged;
        private uint _subscribeCount;
        private AnalogTouchReleasedHandler _analogDoneChanging;

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public UISlider(BasicTriList device, uint analogTouchNumber, bool useMatchingDigitalPressJoin)
            : base(device, analogTouchNumber)
        {
            AnalogTouchJoin = device.UShortOutput[analogTouchNumber];
            if (useMatchingDigitalPressJoin)
                DigitalPressJoin = device.BooleanOutput[analogTouchNumber];
        }

        public UISlider(UIController uiController, uint analogTouchNumber, bool useMatchingDigitalPressJoin)
            : this(uiController.Device, analogTouchNumber, useMatchingDigitalPressJoin)
        {
        }

        public UISlider(UIViewController view, uint analogTouchNumber, bool useMatchingDigitalPressJoin)
            : this(view.UIController.Device, analogTouchNumber, useMatchingDigitalPressJoin)
        {
        }

        public UISlider(SmartObject smartObject, string analogFeedbackJoinName, string analogTouchJoinName)
            : base(smartObject, analogFeedbackJoinName)
        {
            AnalogTouchJoin = smartObject.UShortOutput[analogTouchJoinName];
        }

        public UISlider(SmartObject smartObject, string analogFeedbackJoinName, string analogTouchJoinName, string digitalTouchJoinName)
            : base(smartObject, analogFeedbackJoinName)
        {
            AnalogTouchJoin = smartObject.UShortOutput[analogTouchJoinName];
            DigitalPressJoin = smartObject.BooleanOutput[digitalTouchJoinName];
        }

        #endregion

        #region Events

        public event AnalogTouchValueChangeHandler AnalogValueChanged
        {
            add
            {
                if(_subscribeCount == 0)
                    RegisterToSigChanges();
                _subscribeCount ++;
                _analogValueChanged += value;
            }
            remove
            {
                if (_subscribeCount > 0)
                {
                    _subscribeCount --;
                    _analogValueChanged -= value;
                }
                if(_subscribeCount == 0)
                    UnregisterToSigChanges();
            }
        }

        public event AnalogTouchReleasedHandler AnalogValueDoneChanging
        {
            add
            {
                if(_subscribeCount == 0)
                    RegisterToSigChanges();
                _subscribeCount ++;
                _analogDoneChanging += value;
            }
            remove
            {
                if (_subscribeCount > 0)
                {
                    _subscribeCount --;
                    _analogDoneChanging -= value;
                }
                if(_subscribeCount == 0)
                    UnregisterToSigChanges();
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Set the feedback value and get the touch value
        /// </summary>
        public override ushort AnalogValue 
        {
            get { return AnalogTouchJoin.UShortValue; }
            set { base.AnalogValue = value; }
        }

        /// <summary>
        /// The analog touch join from and to the device or smartobject
        /// </summary>
        public UShortOutputSig AnalogTouchJoin { get; private set; }

        /// <summary>
        /// The digital join from the device or smartobject
        /// </summary>
        public BoolOutputSig DigitalPressJoin { get; private set; }

        /// <summary>
        /// True if button is currently in a pressed state
        /// </summary>
        public bool IsPressed
        {
            get
            {
                return DigitalPressJoin != null && DigitalPressJoin.BoolValue;
            }
        }

        #endregion

        #region Methods

        protected virtual void OnAnalogValueChanged(IAnalogTouch item, ushort value)
        {
            var handler = _analogValueChanged;
            if (handler != null) handler(item, value);
        }

        protected virtual void OnAnalogValueDoneChanging(IAnalogTouch item, ushort value)
        {
            var handler = _analogDoneChanging;
            if (handler != null) handler(item, value);
        }

        protected override void OnSigChange(GenericBase owner, SigEventArgs args)
        {
            base.OnSigChange(owner, args);

            if (args.Event == eSigEvent.BoolChange && args.Sig == DigitalPressJoin && !args.Sig.BoolValue)
            {
                OnAnalogValueDoneChanging(this, AnalogTouchJoin.UShortValue);
                return;
            }

            if(args.Event != eSigEvent.UShortChange || args.Sig != AnalogTouchJoin) return;
            
            if(DigitalPressJoin != null && !DigitalPressJoin.BoolValue) return;
            
            OnAnalogValueChanged(this, args.Sig.UShortValue);
        }

        #endregion
    }
}