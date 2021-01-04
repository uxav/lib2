/* License
 * ------------------------------------------------------------------------------
 * Copyright (c) 2019 UX Digital Systems Ltd
 *
 * Permission is hereby granted, to any person obtaining a copy of this software
 * and associated documentation files (the "Software"), to deal in the Software
 * for the continued use and development of the system on which it was installed,
 * and to permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * Any persons obtaining the software have no rights to use, copy, modify, merge,
 * publish, distribute, sublicense, and/or sell copies of the Software without
 * written persmission from UX Digital Systems Ltd, if it is not for use on the
 * system on which it was originally installed.
 * ------------------------------------------------------------------------------
 * UX.Digital
 * ----------
 * http://ux.digital
 * support@ux.digital
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro;
using UX.Lib2.Cloud.Logger;

namespace UX.Lib2.UI
{
    public abstract class UISubPageReferenceList : IEnumerable<UISubPageReferenceListItem>
    {
        #region Fields

        private readonly UIController _uiController;
        private readonly SmartObject _smartObject;
        private readonly uint _digitalJoinIncrement;
        private readonly uint _analogJoinIncrement;
        private readonly uint _serialJoinIncrement;
        private readonly uint _maxNumberOfItems;
        private uint _selectedItemIndex;
        private ushort _count;

        private readonly Dictionary<uint, UISubPageReferenceListItem> _items =
            new Dictionary<uint, UISubPageReferenceListItem>();

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a base class for subpage referenece lists
        /// </summary>
        /// <param name="uiController">The UI Controller for the decice</param>
        /// <param name="smartObject">The smart object for the device</param>
        /// <param name="digitalJoinIncrement">The digital join increment, must be more than 0!</param>
        /// <param name="analogJoinIncrement">The analog join increment</param>
        /// <param name="serialJoinIncrement">The serial join increment, must be more than 0!</param>
        /// <param name="callBack">A 'CreateItemForIndexCallBack' delegate to create the items</param>
        protected UISubPageReferenceList(UIControllerWithSmartObjects uiController, SmartObject smartObject, uint digitalJoinIncrement, uint analogJoinIncrement,
            uint serialJoinIncrement, CreateItemForIndexCallBack callBack)
        {
            _uiController = uiController;
            _smartObject = smartObject;
            _digitalJoinIncrement = digitalJoinIncrement;
            _analogJoinIncrement = analogJoinIncrement;
            _serialJoinIncrement = serialJoinIncrement;

            _smartObject.SigChange += SmartObjectOnSigChange;

            if (_digitalJoinIncrement == 0 || _serialJoinIncrement == 0)
                throw new Exception("Join increments must be at least 1 for digital and serial joins");

            CloudLog.Debug("{0}.ctor for SmartObject ID: {1}", GetType(), smartObject.ID);
            try
            {
                uint count = 1;
                while (true)
                {
                    var name = string.Format("Item {0} Visible", count);
                    if (_smartObject.BooleanInput.Contains(name))
                    {
                        count ++;
                    }
                    else
                        break;
                }
                _maxNumberOfItems = count - 1;

                CloudLog.Debug("{0} for SmartObject ID: {1} contains {2} items", GetType(), smartObject.ID,
                    _maxNumberOfItems);

                for (uint i = 1; i <= _maxNumberOfItems; i++)
                {
                    _items[i] = callBack(this, i);
                }
            }
            catch (Exception e)
            {
                CloudLog.Exception(e, "Error in {0}.ctor, {1}", GetType().Name, e.Message);
            }
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events

        public event UISubPageReferenceListSelectedItemChangeEventHander SelectedItemChange;

        public event UISubPageReferenceListIsMovingChangedEventHandler IsMovingChange;

        #endregion

        #region Delegates

        public delegate UISubPageReferenceListItem CreateItemForIndexCallBack(UISubPageReferenceList list, uint index);

        #endregion

        public UISubPageReferenceListItem this[uint index]
        {
            get { return _items[index]; }
        }

        #region Properties

        public SmartObject SmartObject
        {
            get { return _smartObject; }
        }

        public virtual ushort NumberOfItems
        {
            get
            {
                return _smartObject.UShortInput["Set Number of Items"].UShortValue;
            }
        }

        public uint MaxNumberOfItems
        {
            get { return _maxNumberOfItems; }
        }

        public uint NumberOfEmptyItems
        {
            get { return MaxNumberOfItems - _count; }
        }

        public bool IsMoving
        {
            get { return _smartObject.BooleanOutput["Is Moving"].BoolValue; }
        }

        public uint DigitalJoinIncrement
        {
            get { return _digitalJoinIncrement; }
        }

        public uint AnalogJoinIncrement
        {
            get { return _analogJoinIncrement; }
        }

        public uint SerialJoinIncrement
        {
            get { return _serialJoinIncrement; }
        }

        public UIController UIController
        {
            get { return _uiController; }
        }

        public UISubPageReferenceListItem SelectedItem
        {
            get { return _items.ContainsKey(_selectedItemIndex) ? _items[_selectedItemIndex] : null; }
        }

        internal ushort ItemsAddedCount
        {
            get { return _count; }
        }

        #endregion

        #region Methods

        public virtual void ScrollToItem(ushort item)
        {
            _smartObject.UShortInput["Scroll To Item"].UShortValue = item;
        }

        protected virtual void SetNumberOfItems(ushort items)
        {
            _smartObject.UShortInput["Set Number of Items"].UShortValue = items;
        }

        public IEnumerator<UISubPageReferenceListItem> GetEnumerator()
        {
            return _items.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void ClearList(bool justByResettingTheCount)
        {
            _selectedItemIndex = 0;

            _count = 0;

            if (!justByResettingTheCount)
            {
                foreach (var item in this)
                {
                    item.LinkedObject = null;
                }

                SetNumberOfItems(_count);
            }

            OnSelectedItemChange(this);
        }

        public virtual void ClearList()
        {
            ClearList(false);
            OnSelectedItemChange(this);
        }

        public virtual uint AddItem(object linkedObject, bool holdOffSettingListSize)
        {
            if (_count == MaxNumberOfItems)
            {
                CloudLog.Error("Cannot add item to {0}, No more items available!, count = {1}, max = {2}",
                    GetType().Name, _count, MaxNumberOfItems);
                return 0;
            }

            _count++;

            var item = this[_count];

            item.Show();
            item.Enable();
            item.Feedback = false;
            item.LinkedObject = linkedObject;

            if (!holdOffSettingListSize)
                SetNumberOfItems(_count);

            return _count;
        }

        public virtual uint AddItem(object linkedObject)
        {
            return AddItem(linkedObject, false);
        }

        public virtual uint AddItem(string title, object linkedObject, bool holdOffSettingListSize)
        {
            var index = AddItem(linkedObject, holdOffSettingListSize);

            var item = this[index];
            item.Text = title;

            return index;
        }


        public virtual uint AddItem(string title, object linkedObject)
        {
            var index = AddItem(linkedObject);

            var item = this[index];
            item.Text = title;

            return index;
        }

        public bool ContainsLinkedObject(object linkedObject)
        {
            for (uint i = 1; i <= NumberOfItems; i ++)
            {
                if (_items[i].LinkedObject == linkedObject) return true;
            }

            return false;
        }

        public void SetListSizeToItemCount()
        {
            SetNumberOfItems(_count);
        }

        public void SetSelectedItem(object linkedObject)
        {
            if (linkedObject == null)
            {
                ClearSelectedItems();
                return;
            }

            try
            {
                SetSelectedItem(Nullable.GetUnderlyingType(linkedObject.GetType()) != null
                    ? this.FirstOrDefault(i => i.LinkedObject == linkedObject)
                    : this.FirstOrDefault(i => i.LinkedObject != null && i.LinkedObject.Equals(linkedObject)));
            }
            catch (Exception e)
            {
                CloudLog.Exception(e, "Cannot set selected item in SRL, linkedObject type is \"{0}\"", linkedObject.GetType());
            }
        }

        public void SetSelectedItem(UISubPageReferenceListItem item)
        {
            foreach (var listItem in this.Where(i => i != item))
            {
                listItem.SetFeedback(false);
            }

            if (item != null)
            {
                item.SetFeedback(true);
                _selectedItemIndex = item.Index;
            }
            else
            {
                _selectedItemIndex = 0;
            }

            OnSelectedItemChange(this);
        }

        public void ClearSelectedItems()
        {
            foreach (var listItem in this)
            {
                listItem.Feedback = false;
            }

            _selectedItemIndex = 0;
            OnSelectedItemChange(this);
        }

        protected virtual void OnSelectedItemChange(UISubPageReferenceList list)
        {
            var handler = SelectedItemChange;
            if (handler != null) handler(list);
        }

        protected virtual void OnIsMovingChange(UISubPageReferenceList list, bool ismoving)
        {
            var handler = IsMovingChange;
            if (handler != null) handler(list, ismoving);
        }

        private void SmartObjectOnSigChange(GenericBase currentDevice, SmartObjectEventArgs args)
        {
            if (args.Sig.Name == "Is Moving" && args.Sig.Type == eSigType.Bool)
            {
                OnIsMovingChange(this, args.Sig.BoolValue);
            }
        }

        #endregion
    }

    public delegate void UISubPageReferenceListSelectedItemChangeEventHander(UISubPageReferenceList list);

    public delegate void UISubPageReferenceListIsMovingChangedEventHandler(UISubPageReferenceList list, bool isMoving);
}