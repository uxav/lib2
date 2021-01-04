using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace UX.Lib2.UI
{
    public class UIButtonWithModes : UIButton, IModeItem
    {
        #region Constructors

        /// <summary>
        /// Create a multi-mode button
        /// </summary>
        /// <param name="device">The device for the button</param>
        /// <param name="pressJoinNumber">The join number for press and feedback</param>
        /// <param name="modeJoinNumber">The join number for the analog mode join</param>
        public UIButtonWithModes(BasicTriList device, uint pressJoinNumber, uint modeJoinNumber)
            : base(device, pressJoinNumber)
        {
            ModeJoin = device.UShortInput[modeJoinNumber];
        }

        /// <summary>
        /// Create a multi-mode button with dynamic text
        /// </summary>
        /// <param name="device">The device for the button</param>
        /// <param name="pressJoinNumber">The join number for press and feedback</param>
        /// <param name="serialJoinNumber">The join number for the text input</param>
        /// <param name="modeJoinNumber">The join number for the analog mode join</param>
        public UIButtonWithModes(BasicTriList device, uint pressJoinNumber, uint serialJoinNumber, uint modeJoinNumber)
            : base(device, pressJoinNumber, serialJoinNumber)
        {
            ModeJoin = device.UShortInput[modeJoinNumber];
        }

        /// <summary>
        /// Create a multi-mode button using a smartobject
        /// </summary>
        /// <param name="smartObject"></param>
        /// <param name="pressJoinName"></param>
        /// <param name="feedbackJoinName"></param>
        /// <param name="modeJoinName"></param>
        public UIButtonWithModes(SmartObject smartObject, string pressJoinName, string feedbackJoinName, string modeJoinName)
            : base(smartObject, pressJoinName, feedbackJoinName)
        {
            ModeJoin = smartObject.UShortInput[modeJoinName];
        }

        #endregion

        #region Properties

        /// <summary>
        /// Set the mode value of the object
        /// </summary>
        public ushort Mode
        {
            get { return ModeJoin.UShortValue; }
            set { ModeJoin.UShortValue = value; }
        }

        /// <summary>
        /// The analog mode join to the object
        /// </summary>
        public UShortInputSig ModeJoin { get; private set; }

        #endregion
    }
}