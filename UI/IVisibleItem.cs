using Crestron.SimplSharpPro;

namespace UX.Lib2.UI
{
    public interface IVisibleItem : IUIObject
    {
        #region Events
        
        /// <summary>
        /// Triggered when the visibility changes on the item
        /// </summary>
        event VisibilityChangeEventHandler VisibilityChanged;

        #endregion

        #region Properties

        /// <summary>
        /// True if currently visible
        /// </summary>
        bool Visible { get; }

        /// <summary>
        /// The state set for Visible.
        /// This will be set before the feedback on Visible and may be used
        /// to reference if something is currently transitioning.
        /// </summary>
        bool RequestedVisibleState { get; }

        /// <summary>
        /// The digital join for the visible feedback
        /// </summary>
        BoolInputSig VisibleJoin { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Make the object visible
        /// </summary>
        void Show();

        /// <summary>
        /// Hide the object
        /// </summary>
        void Hide();

        #endregion
    }

    /// <summary>
    /// Event handler delegate for when the visibility changes on an IVisibleItem
    /// </summary>
    /// <param name="item">The item triggering the event</param>
    /// <param name="args">More information on the change</param>
    public delegate void VisibilityChangeEventHandler(IVisibleItem item, VisibilityChangeEventArgs args);

    /// <summary>
    /// Aruments for a visibility change event
    /// </summary>
    public class VisibilityChangeEventArgs
    {
        internal VisibilityChangeEventArgs(bool willBeVisible, VisibilityChangeEventType eventType)
        {
            EventType = eventType;
            WillBeVisible = willBeVisible;
        }

        /// <summary>
        /// The type of visible change
        /// </summary>
        public VisibilityChangeEventType EventType { get; private set; }

        public bool WillBeVisible { get; private set; }
    }

    /// <summary>
    /// The type of event which can trigger
    /// </summary>
    public enum VisibilityChangeEventType
    {
        /// <summary>
        /// The item will show
        /// </summary>
        WillShow,
        /// <summary>
        /// The item did show
        /// </summary>
        DidShow,
        /// <summary>
        /// The item will hide
        /// </summary>
        WillHide,
        /// <summary>
        /// The item did hide
        /// </summary>
        DidHide
    }
}