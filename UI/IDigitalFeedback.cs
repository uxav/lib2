using Crestron.SimplSharpPro;

namespace UX.Lib2.UI
{
    public interface IDigitalFeedback : IUIObject
    {
        #region Events
        #endregion

        #region Properties

        /// <summary>
        /// Set or get the current digital feedback state
        /// </summary>
        bool Feedback { get; set; }

        /// <summary>
        /// The digital join to the device or smartobject
        /// </summary>
        BoolInputSig DigitalFeedbackJoin { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Set the feedback state
        /// </summary>
        /// <param name="value">The high / low state of the feedback join</param>
        void SetFeedback(bool value);

        #endregion
    }
}