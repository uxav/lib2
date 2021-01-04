using Crestron.SimplSharpPro;

namespace UX.Lib2.UI
{
    /// <summary>
    /// A UI item which has an analog mode join
    /// </summary>
    public interface IModeItem : IUIObject
    {
        #region Events
        #endregion

        #region Properties

        /// <summary>
        /// Set the mode value of the object
        /// </summary>
        ushort Mode { get; set; }

        /// <summary>
        /// The analog mode join to the object
        /// </summary>
        UShortInputSig ModeJoin { get; }

        #endregion

        #region Methods
        #endregion
    }
}