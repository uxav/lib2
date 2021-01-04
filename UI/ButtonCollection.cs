using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UX.Lib2.UI
{
    public sealed class ButtonCollection : IEnumerable<IButton>
    {
        #region Fields

        internal readonly Dictionary<uint, IButton> Buttons = new Dictionary<uint, IButton>();
        private ButtonEventHandler _buttonEvent;
        private int _subscribeCount;

        #endregion

        #region Constructors

        /// <summary>
        /// The default Constructor.
        /// </summary>
        public ButtonCollection()
        {
            
        }

        /// <summary>
        /// Create a button collection from a dictionary of indexed buttons
        /// </summary>
        /// <param name="buttons"></param>
        public ButtonCollection(Dictionary<uint, IButton> buttons)
        {
            Buttons = buttons;
        }

        public ButtonCollection(UITabBar tabBar)
        {
            Buttons = new Dictionary<uint, IButton>();
            var count = 0U;
            foreach (var button in tabBar)
            {
                count++;
                Buttons.Add(count, button);
            }
        }
        
        /// <summary>
        /// Create a button collection from a subpage reference list object
        /// </summary>
        /// <param name="subPageReferenceListItems"></param>
        public ButtonCollection(IEnumerable<UISubPageReferenceListItem> subPageReferenceListItems)
        {
            Buttons = new Dictionary<uint, IButton>();
            foreach (var item in subPageReferenceListItems)
            {
                Buttons.Add(item.Index, item);
            }
        }

        /// <summary>
        /// Create a button collection from a dynamic button list
        /// </summary>
        /// <param name="dynamicButtonListItems"></param>
        public ButtonCollection(IEnumerable<UIDynamicButtonListItem> dynamicButtonListItems)
        {
            Buttons = new Dictionary<uint, IButton>();
            foreach (var item in dynamicButtonListItems)
            {
                Buttons.Add(item.Index, item);
            }
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events

        public event ButtonEventHandler ButtonEvent
        {
            add
            {
                if (_subscribeCount == 0)
                {
                    foreach (var button in Buttons.Values)
                    {
                        button.ButtonEvent += OnButtonEvent;
                    }
                }
                _subscribeCount ++;
                _buttonEvent += value;
            }
            remove
            {
                if(_subscribeCount == 0) return;
                _subscribeCount --;
                _buttonEvent -= value;
                if (_subscribeCount == 0)
                {
                    foreach (var button in Buttons.Values)
                    {
                        button.ButtonEvent -= OnButtonEvent;
                    }
                }
            }
        }

        #endregion

        #region Delegates
        #endregion

        #region Properties

        /// <summary>
        /// Get the count of the buttons in the collection
        /// </summary>
        public int Count
        {
            get { return Buttons.Count; }
        }

        #endregion

        /// <summary>
        /// Get a button by the digital press join number
        /// </summary>
        /// <param name="key">The object used for the Key</param>
        /// <returns>The button for the join number</returns>
        public IButton this[uint key]
        {
            get { return Buttons[key]; }
            set
            {
                if(value == null)
                    throw new ArgumentNullException("Cannot set item in collection to null");
                if (Buttons.ContainsValue(value))
                    throw new InvalidOperationException("Collection already contains button object");
                if (Buttons.ContainsKey(key) && _subscribeCount > 0)
                    Buttons[key].ButtonEvent -= OnButtonEvent;
                Buttons[key] = value;
                if (_subscribeCount > 0)
                    Buttons[key].ButtonEvent += OnButtonEvent;
            }
        }

        #region Methods

        /// <summary>
        /// See if collection contains a button with the key value
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(uint key)
        {
            return Buttons.ContainsKey(key);
        }

        /// <summary>
        /// Add a button to the collection
        /// </summary>
        /// <param name="key">The key used for the button</param>
        /// <param name="button">A button</param>
        public void Add(uint key, IButton button)
        {
            if(Buttons.ContainsKey(key))
                throw new ArgumentException("Key already exists");
            if(button == null)
                throw new ArgumentNullException("Button value cannot be null");
            this[key] = button;
        }

        /// <summary>
        /// Remove a button from the collection
        /// </summary>
        /// <param name="key">Remove a button by key value</param>
        public void Remove(uint key)
        {
            if(!Buttons.ContainsKey(key))
                throw new ArgumentOutOfRangeException("Key does not exist in collection");
            if (_subscribeCount > 0)
                Buttons[key].ButtonEvent -= OnButtonEvent;
            Buttons.Remove(key);
        }

        private void OnButtonEvent(IButton button, ButtonEventArgs args)
        {
            var handler = _buttonEvent;
            var newArgs = new ButtonEventArgs(args.EventType, args.HoldTime, this,
                Buttons.First(kvp => kvp.Value.DigitalPressJoin == button.DigitalPressJoin).Key);
            if (handler != null) handler(button, newArgs);
        }

        public void SetInterlockedFeedback(uint key)
        {
            foreach (var dictItem in Buttons.Where(item => item.Key != key))
            {
                dictItem.Value.Feedback = false;
            }
            if (Buttons.ContainsKey(key))
            {
                Buttons[key].Feedback = true;
            }
        }

        public void ClearInterlockedFeedback()
        {
            foreach (var button in Buttons.Values)
            {
                button.Feedback = false;
            }
        }

        public IEnumerator<IButton> GetEnumerator()
        {
            return Buttons.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}