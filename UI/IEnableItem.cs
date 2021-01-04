using Crestron.SimplSharpPro;

namespace UX.Lib2.UI
{
    /// <summary>
    /// A UI item which can be enabled or disabled by a digital enable join
    /// </summary>
    public interface IEnableItem : IUIObject
    {
        #region Properties

        /// <summary>
        /// The UI item enable digital join
        /// </summary>
        BoolInputSig EnableJoin { get; }

        /// <summary>
        /// True if enabled
        /// </summary>
        bool Enabled { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Enable the UI item
        /// </summary>
        void Enable();

        /// <summary>
        /// Disable the UI item
        /// </summary>
        void Disable();

        #endregion
    }
}