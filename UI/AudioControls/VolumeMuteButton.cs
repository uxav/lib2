using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using UX.Lib2.Models;

namespace UX.Lib2.UI.AudioControls
{
    public class VolumeMuteButton : UIButton
    {
        #region Fields

        private IAudioLevelControl _control;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a basic button
        /// </summary>
        /// <param name="viewController">The View Controller for the button</param>
        /// <param name="pressJoinNumber">The join number for press and feedback</param>
        public VolumeMuteButton(UIViewController viewController, uint pressJoinNumber)
            : base(viewController, pressJoinNumber)
        {
        }

        /// <summary>
        /// Create a basic button with dynamic text
        /// </summary>
        /// <param name="viewController">The View Controller for the button</param>
        /// <param name="pressJoinNumber">The join number for press and feedback</param>
        /// <param name="serialJoinNumber">The join number for the text input</param>
        public VolumeMuteButton(UIViewController viewController, uint pressJoinNumber, uint serialJoinNumber)
            : base(viewController, pressJoinNumber, serialJoinNumber)
        {
        }

        /// <summary>
        /// Create a basic button
        /// </summary>
        /// <param name="uiController">The UI Controller for the button</param>
        /// <param name="pressJoinNumber">The join number for press and feedback</param>
        public VolumeMuteButton(UIController uiController, uint pressJoinNumber)
            : base(uiController, pressJoinNumber)
        {
        }

        /// <summary>
        /// Create a basic button with dynamic text
        /// </summary>
        /// <param name="uiController">The UI Controller for the button</param>
        /// <param name="pressJoinNumber">The join number for press and feedback</param>
        /// <param name="serialJoinNumber">The join number for the text input</param>
        public VolumeMuteButton(UIController uiController, uint pressJoinNumber, uint serialJoinNumber)
            : base(uiController, pressJoinNumber, serialJoinNumber)
        {
        }

        /// <summary>
        /// Create a basic button
        /// </summary>
        /// <param name="device">The device for the button</param>
        /// <param name="pressJoinNumber">The join number for press and feedback</param>
        public VolumeMuteButton(BasicTriList device, uint pressJoinNumber)
            : base(device, pressJoinNumber)
        {
        }

        /// <summary>
        /// Create a basic button with dynamic text
        /// </summary>
        /// <param name="device">The device for the button</param>
        /// <param name="pressJoinNumber">The join number for press and feedback</param>
        /// <param name="serialJoinNumber">The join number for the text input</param>
        public VolumeMuteButton(BasicTriList device, uint pressJoinNumber, uint serialJoinNumber)
            : base(device, pressJoinNumber, serialJoinNumber)
        {
        }

        /// <summary>
        /// Create a basic smart object button
        /// </summary>
        /// <param name="smartObject">The smartobject extender</param>
        /// <param name="pressJoinName">The join name of the smart object push</param>
        /// <param name="feedbackJoinName">The join name of the smart object feedback</param>
        public VolumeMuteButton(SmartObject smartObject, string pressJoinName, string feedbackJoinName)
            : base(smartObject, pressJoinName, feedbackJoinName)
        {
        }

        /// <summary>
        /// Create a basic smart object button with dynamic text
        /// </summary>
        /// <param name="smartObject">The smartobject extender</param>
        /// <param name="pressJoinName">The join name of the smart object push</param>
        /// <param name="feedbackJoinName">The join name of the smart object feedback</param>
        /// <param name="serialJoinName">The set text join name of the smart object</param>
        public VolumeMuteButton(SmartObject smartObject, string pressJoinName, string feedbackJoinName, string serialJoinName)
            : base(smartObject, pressJoinName, feedbackJoinName, serialJoinName)
        {
        }

        #endregion

        #region Properties

        public IAudioLevelControl MuteControl
        {
            get { return _control; }
            set
            {
                if (_control == value) return;

                if (_control != null)
                {
                    _control.MuteChange -= SetFeedback;
                    ButtonEvent -= OnMuteButtonEvent;
                }

                _control = value;

                if (_control == null) return;

                _control.MuteChange += SetFeedback;
                ButtonEvent += OnMuteButtonEvent;
                Feedback = _control.Muted;
            }
        }

        private void OnMuteButtonEvent(IButton button, ButtonEventArgs args)
        {
            if (args.EventType != ButtonEventType.Released) return;
            var muted = !_control.Muted;
            _control.Muted = muted;
            Feedback = muted;
        }

        #endregion

        #region Methods

        protected override void Dispose(bool disposing)
        {
            MuteControl = null;

            base.Dispose(disposing);
        }

        #endregion
    }
}