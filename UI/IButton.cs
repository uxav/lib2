using System;

namespace UX.Lib2.UI
{
    public interface IButton : IDigitalPress, IDigitalFeedback, ITextItem, IVisibleItem, IEnableItem
    {
        #region Events

        /// <summary>
        /// Subscribe to push events for the button
        /// </summary>
        event ButtonEventHandler ButtonEvent;

        #endregion

        #region Properties

        /// <summary>
        /// Set or get the time for the button to trigger a hold event
        /// </summary>
        TimeSpan HoldTime { get; set; }

        #endregion

        #region Methods

        #endregion
    }

    /// <summary>
    /// A handler delegate for a button event
    /// </summary>
    /// <param name="button">The button which triggered the event</param>
    /// <param name="args">Information about the event</param>
    public delegate void ButtonEventHandler(IButton button, ButtonEventArgs args);

    /// <summary>
    /// Args for a button event
    /// </summary>
    public class ButtonEventArgs : EventArgs
    {
        internal ButtonEventArgs(ButtonEventType eventType, TimeSpan timeSincePress)
        {
            EventType = eventType;
            HoldTime = timeSincePress;
        }

        internal ButtonEventArgs(ButtonEventType eventType, TimeSpan timeSincePress, ButtonCollection collection, uint collectionKey)
        {
            EventType = eventType;
            HoldTime = timeSincePress;
            CalledFromCollection = true;
            Collection = collection;
            CollectionKey = collectionKey;
        }

        /// <summary>
        /// The button event type occuring for this event
        /// </summary>
        public ButtonEventType EventType { get; private set; }

        /// <summary>
        /// The time the button has been held since a press occured
        /// </summary>
        public TimeSpan HoldTime { get; private set; }

        /// <summary>
        /// True if the event was called from a button collection
        /// </summary>
        public bool CalledFromCollection { get; private set; }

        /// <summary>
        /// Returns a collection if the event is called from a collection
        /// </summary>
        public ButtonCollection Collection { get; private set; }

        /// <summary>
        /// The key value of the button in the collection
        /// </summary>
        public uint CollectionKey { get; private set; }
    }

    /// <summary>
    /// Describes the type of button event occuring
    /// </summary>
    public enum ButtonEventType
    {
        /// <summary>
        /// The button was pressed
        /// </summary>
        Pressed,
        /// <summary>
        /// The button was pressed and imediately released before a hold
        /// </summary>
        Tapped,
        /// <summary>
        /// The button was held
        /// </summary>
        Held,
        /// <summary>
        /// The button was released
        /// </summary>
        Released
    }
}