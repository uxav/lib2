using Crestron.SimplSharpPro;

namespace UX.Lib2.UI
{
    public interface IDigitalPress : IUIObject
    {
        #region Properties

        /// <summary>
        /// The digital join from the device or smartobject
        /// </summary>
        BoolOutputSig DigitalPressJoin { get; }

        /// <summary>
        /// True if button is currently in a pressed state
        /// </summary>
        bool IsPressed { get; }

        #endregion

        #region Methods

        #endregion
    }
}