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

using System.Collections;
using System.Collections.Generic;
using Crestron.SimplSharpPro;

namespace UX.Lib2.UI
{
    public class UIPageCollection : IEnumerable<UIPageViewController>
    {

        #region Fields

        private readonly UIController _uiController;

        private static readonly Dictionary<uint, Dictionary<uint, UIPageViewController>> Pages =
            new Dictionary<uint, Dictionary<uint, UIPageViewController>>();

        private static readonly Dictionary<uint, List<UIPageViewController>> PreviousPagesDict =
            new Dictionary<uint, List<UIPageViewController>>(); 

        #endregion

        #region Constructors

        /// <summary>
        /// The default Constructor.
        /// </summary>
        internal UIPageCollection(UIController uiController)
        {
            _uiController = uiController;
            //_uiController.Device.OnlineStatusChange += DeviceOnOnlineStatusChange;
            Pages[_uiController.Device.ID] = new Dictionary<uint, UIPageViewController>();
            PreviousPagesDict[_uiController.Device.ID] = new List<UIPageViewController>();
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events

        /// <summary>
        /// Page visibility changes can be subscribed to here
        /// </summary>
        public event VisibilityChangeEventHandler VisibilityChanged;

        #endregion

        #region Delegates
        #endregion

        public UIPageViewController this[uint pageJoin]
        {
            get { return Pages[_uiController.Device.ID][pageJoin]; }
            private set { Pages[_uiController.Device.ID][pageJoin] = value; }
        }

        #region Properties

        #endregion

        #region Methods

        internal void Add(UIPageViewController page)
        {
            this[page.PageNumber] = page;
            page.VisibilityChanged += PageOnVisibilityChanged;
        }

        private void PageOnVisibilityChanged(IVisibleItem item, VisibilityChangeEventArgs args)
        {
            if (VisibilityChanged != null)
                VisibilityChanged(item, args);
        }

        internal List<UIPageViewController> PreviousPages
        {
            get { return PreviousPagesDict[_uiController.Device.ID]; }
        }

        public void ClearPreviousPageLogic()
        {
            PreviousPagesDict[_uiController.Device.ID].Clear();
        }

        public void ClearPreviousPageLogic(UIPageViewController pageToSetAsPrevious)
        {
            PreviousPagesDict[_uiController.Device.ID].Clear();
            PreviousPagesDict[_uiController.Device.ID].Add(pageToSetAsPrevious);
        }
        /*
        private void DeviceOnOnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            if (args.DeviceOnLine) return;
            var pages = Pages[currentDevice.ID];
            foreach (var page in pages)
            {
                page.Value.VisibleJoin.BoolValue = false;
            }
        }
        */
        #endregion

        public IEnumerator<UIPageViewController> GetEnumerator()
        {
            return Pages[_uiController.Device.ID].Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}