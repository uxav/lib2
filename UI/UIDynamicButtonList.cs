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
    public sealed class UIDynamicButtonList : IEnumerable<UIDynamicButtonListItem>
    {
        #region Fields

        private readonly UIController _uiController;
        private readonly SmartObject _smartObject;
        private readonly uint _maxNumberOfItems;
        private uint _selectedItemIndex;
        private ushort _count;

        private readonly Dictionary<uint, UIDynamicButtonListItem> _items =
            new Dictionary<uint, UIDynamicButtonListItem>();

        #endregion

        #region Constructors

        public UIDynamicButtonList(UIControllerWithSmartObjects uiController, SmartObject smartObject)
        {
            _uiController = uiController;
            _smartObject = smartObject;

            _smartObject.SigChange += SmartObjectOnSigChange;

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
                    _items[i] = new UIDynamicButtonListItem(this, i);
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

        public event UIDynamicButtonListSelectedItemChangeEventHander SelectedItemChange;

        public event UIDynamicButtonListIsMovingChangedEventHandler IsMovingChange;

        #endregion

        #region Delegates

        public delegate UISubPageReferenceListItem CreateItemForIndexCallBack(UISubPageReferenceList list, uint index);

        #endregion

        public UIDynamicButtonListItem this[uint index]
        {
            get { return _items[index]; }
        }

        #region Properties

        public SmartObject SmartObject
        {
            get { return _smartObject; }
        }

        public ushort NumberOfItems
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

        public UIController UIController
        {
            get { return _uiController; }
        }

        public UIDynamicButtonListItem SelectedItem
        {
            get { return _items.ContainsKey(_selectedItemIndex) ? _items[_selectedItemIndex] : null; }
        }

        #endregion

        #region Methods

        public void ScrollToItem(ushort item)
        {
            _smartObject.UShortInput["Scroll To Item"].UShortValue = item;
        }

        private void SetNumberOfItems(ushort items)
        {
            _smartObject.UShortInput["Set Number of Items"].UShortValue = items;
        }

        public IEnumerator<UIDynamicButtonListItem> GetEnumerator()
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

        public void ClearList()
        {
            ClearList(false);
        }

        public uint AddItem(object linkedObject, bool holdOffSettingListSize)
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

        public uint AddItem(object linkedObject)
        {
            return AddItem(linkedObject, false);
        }

        public uint AddItem(string title, object linkedObject, bool holdOffSettingListSize)
        {
            var index = AddItem(linkedObject, holdOffSettingListSize);

            var item = this[index];
            item.Text = title;

            return index;
        }


        public uint AddItem(string title, object linkedObject)
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

        public void SetSelectedItem(UIDynamicButtonListItem item)
        {
            foreach (var listItem in this.Where(i => i != item))
            {
                listItem.Feedback = false;
            }

            if (item != null)
            {
                item.Feedback = true;
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

        private void OnSelectedItemChange(UIDynamicButtonList list)
        {
            var handler = SelectedItemChange;
            if (handler != null) handler(list);
        }

        private void OnIsMovingChange(UIDynamicButtonList list, bool ismoving)
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

    public delegate void UIDynamicButtonListSelectedItemChangeEventHander(UIDynamicButtonList list);

    public delegate void UIDynamicButtonListIsMovingChangedEventHandler(UIDynamicButtonList list, bool isMoving);
}