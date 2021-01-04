using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using UX.Lib2.Cloud.Logger;

namespace UX.Lib2.UI
{
    public abstract class UIPageViewController : UIViewController
    {
        #region Fields

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for a Page based view controller
        /// </summary>
        protected UIPageViewController(UIController uiController, uint pageNumber)
            : base(uiController, uiController.Device.BooleanInput[pageNumber])
        {
            if (uiController.Pages.Any(p => p.PageNumber == pageNumber))
                throw new Exception(
                    string.Format("Cannot add page controller with digital join {0}, page already exists", pageNumber));

            uiController.Pages.Add(this);
            uiController.Device.SigChange += DeviceOnSigChange;
            uiController.Device.OnlineStatusChange += DeviceOnOnlineStatusChange;
        }

        #endregion

        #region Finalizers

        #endregion

        #region Events

        #endregion

        #region Delegates

        #endregion

        #region Properties

        /// <summary>
        /// All other pages for this UI not including this one
        /// </summary>
        public IEnumerable<UIPageViewController> OtherPages
        {
            get { return UIController.Pages.Where(p => p != this); }
        }

        /// <summary>
        /// The number of the page join
        /// </summary>
        public uint PageNumber
        {
            get { return VisibleJoin.Number; }
        }

        #region Overrides of UIViewController

        /// <summary>
        /// True if currently visible
        /// </summary>
        public override bool Visible
        {
            get
            {
                return VisibleJoin.BoolValue;
            }
            protected set
            {
                if (VisibleJoin.BoolValue == value) return;

                RequestedVisibleState = value;

                if(value)
                {
                    if (UIController.Pages.PreviousPages.Contains(this))
                    {
                        UIController.Pages.PreviousPages.Remove(this);
                    }
                    
                    foreach (var page in OtherPages.Where(page => page.Visible))
                    {
                        page.Visible = false;
                        UIController.Pages.PreviousPages.Add(page);
                    }

#if DEBUG
                    Debug.WriteInfo("Previous Pages", "Count = {0}", UIController.Pages.PreviousPages.Count());
#endif
                }

                OnVisibilityChanged(this,
                new VisibilityChangeEventArgs(value, value
                    ? VisibilityChangeEventType.WillShow
                    : VisibilityChangeEventType.WillHide));

                VisibleJoin.BoolValue = value;

                CloudLog.Debug("Page Join {0} set to {1}", VisibleJoin.Number, VisibleJoin.BoolValue);

                OnVisibilityChanged(this,
                    new VisibilityChangeEventArgs(value, value
                        ? VisibilityChangeEventType.DidShow
                        : VisibilityChangeEventType.DidHide));
            }
        }

        public UIPageViewController PreviousPage
        {
            get { return UIController.Pages.PreviousPages.LastOrDefault(); }
        }

        public bool CanGoBack
        {
            get { return PreviousPage != null; }
        }

        #endregion

        #endregion

        #region Methods

        private void DeviceOnSigChange(BasicTriList currentDevice, SigEventArgs args)
        {
            if (args.Event != eSigEvent.BoolChange || args.Sig.Number != VisibleJoin.Number || !args.Sig.BoolValue)
                return;

            CloudLog.Debug("Page Feedback {0} = {1}", args.Sig.Number, args.Sig.BoolValue);
        }

        private void DeviceOnOnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            if (!args.DeviceOnLine || !VisibleJoin.BoolValue) return;
            //VisibleJoin.BoolValue = false;
            //new CTimer(specific => VisibleJoin.BoolValue = true, 100);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                
            }

            UIController.Device.SigChange -= DeviceOnSigChange;
            UIController.Device.OnlineStatusChange -= DeviceOnOnlineStatusChange;
        }

        public void Back()
        {
            var previousPage = UIController.Pages.PreviousPages.LastOrDefault();
            if (previousPage == null) return;
            Visible = false;
            previousPage.Show();
        }

        #endregion
    }
}