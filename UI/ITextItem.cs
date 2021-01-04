using System;

namespace UX.Lib2.UI
{
    /// <summary>
    /// A UI object which has text
    /// </summary>
    public interface ITextItem : ISerialItem
    {
        #region Events
        #endregion

        #region Properties

        /// <summary>
        /// Set or get the item text
        /// </summary>
        string Text { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Set the text
        /// </summary>
        /// <param name="text">The text to set the item with</param>
        void SetText(string text);

        /// <summary>
        /// Set the text, formatted with parameters
        /// </summary>
        /// <param name="text">The format string</param>
        /// <param name="args">The parameters</param>
        void SetText(string text, params object[] args);

        /// <summary>
        /// Set the text, formatted with parameters
        /// </summary>
        /// <param name="provider">Format provider</param>
        /// <param name="format">The format string</param>
        /// <param name="args">The parameters</param>
        void SetText(IFormatProvider provider, string format, params object[] args);

        #endregion
    }
}