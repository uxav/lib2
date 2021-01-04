using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronThread;
using UX.Lib2.Cloud.Logger;

namespace UX.Lib2.UI
{
    /// <summary>
    /// Base view controller class
    /// </summary>
    public abstract class UIViewController : UIObject, IVisibleItem
    {
        #region Fields

        private readonly UIController _uiController;
        private readonly IVisibleItem _parent;
        private CTimer _timeOut;
        private TimeSpan _timeOutTime = TimeSpan.Zero;
        private bool _parentViewHiding;

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        protected UIViewController(UIController uiController, BoolInputSig visibleJoin)
            : base(uiController.Device)
        {
            _uiController = uiController;
            VisibleJoin = visibleJoin;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uiController"></param>
        /// <param name="visibleJoin"></param>
        /// <param name="parentVisibleItem"></param>
        protected UIViewController(UIController uiController, BoolInputSig visibleJoin, IVisibleItem parentVisibleItem)
            : this(uiController, visibleJoin)
        {
            _parent = parentVisibleItem;
            _parent.VisibilityChanged += ParentVisibilityChanged;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentViewController"></param>
        /// <param name="visibleJoin"></param>
        protected UIViewController(UIViewController parentViewController, BoolInputSig visibleJoin)
            : this(parentViewController.UIController, visibleJoin, parentViewController)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uiController"></param>
        /// <param name="parentVisibleItem"></param>
        protected UIViewController(UIController uiController, IVisibleItem parentVisibleItem)
            : this(uiController, parentVisibleItem.VisibleJoin)
        {
        }

        #endregion

        #region Finalizers

        ~UIViewController()
        {
            Dispose(false);
        }

        #endregion

        #region Events

        /// <summary>
        /// Triggered when the visibility changes on the view
        /// </summary>
        public event VisibilityChangeEventHandler VisibilityChanged;

        #endregion

        #region Properties

        /// <summary>
        /// The UIController that contains the view
        /// </summary>
        public UIController UIController
        {
            get { return _uiController; }
        }

        /// <summary>
        /// Parent visible item which this follows on hide
        /// </summary>
        public IVisibleItem Parent
        {
            get { return _parent; }
        }

        /// <summary>
        /// True if currently visible
        /// </summary>
        public virtual bool Visible
        {
            get { return VisibleJoin == null || VisibleJoin.BoolValue; }
            protected set
            {
                if (VisibleJoin == null || VisibleJoin.BoolValue == value) return;

                RequestedVisibleState = value;

                OnVisibilityChanged(this,
                    new VisibilityChangeEventArgs(value, value
                        ? VisibilityChangeEventType.WillShow
                        : VisibilityChangeEventType.WillHide));

                if (value == false && _parentViewHiding)
                {
                    var thread = new Thread(specific =>
                    {
                        _parentViewHiding = false;
                        Thread.Sleep(10);
                        VisibleJoin.BoolValue = false;
                        return null;
                    }, null);
                }
                else
                {
                    VisibleJoin.BoolValue = value;
                }

                OnVisibilityChanged(this,
                    new VisibilityChangeEventArgs(value, value
                        ? VisibilityChangeEventType.DidShow
                        : VisibilityChangeEventType.DidHide));
            }
        }

        public bool RequestedVisibleState { get; protected set; }

        /// <summary>
        /// The digital join for the visible feedback
        /// </summary>
        public virtual BoolInputSig VisibleJoin { get; set; }

        /// <summary>
        /// The digital join from the panel to go high once transition is complete
        /// </summary>
        public BoolOutputSig TransitionCompleteJoin { get; private set; }

        public abstract string ViewName { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Show the view. Also cancels a timeout if active.
        /// </summary>
        public virtual void Show()
        {
            _timeOutTime = TimeSpan.Zero;
            if (_timeOut != null && !_timeOut.Disposed)
            {
                _timeOut.Dispose();
            }
            Visible = true;
        }

        /// <summary>
        /// Show the view with a timeout. Call again to reset the timeout.
        /// </summary>
        /// <param name="time">TimeSpan duration to timeout</param>
        public virtual void Show(TimeSpan time)
        {
            _timeOutTime = time;
            if (time == TimeSpan.Zero && _timeOut != null && !_timeOut.Disposed)
            {
                _timeOut.Dispose();
            }
            Visible = true;
            if (_timeOut == null || _timeOut.Disposed)
            {
                if (time > TimeSpan.Zero)
                {
                    _timeOut = new CTimer(specific => Hide(), null, (long)time.TotalMilliseconds);
                }
            }
            else
            {
                _timeOut.Reset((long)time.TotalMilliseconds);
            }
        }

        /// <summary>
        /// Set the timeout if the page is already showing
        /// </summary>
        /// <param name="time">TimeSpan duration to timeout</param>
        protected void SetTimeOut(TimeSpan time)
        {
            _timeOutTime = time;
            if (time == TimeSpan.Zero && _timeOut != null && !_timeOut.Disposed)
            {
                _timeOut.Dispose();
                return;
            }
            if (_timeOut == null || _timeOut.Disposed)
            {
                if (time > TimeSpan.Zero)
                {
                    _timeOut = new CTimer(specific => Hide(), null, (long) time.TotalMilliseconds);
                }
            }
            else
            {
                _timeOut.Reset((long)time.TotalMilliseconds);
            }
        }

        /// <summary>
        /// Hide the view
        /// </summary>
        public void Hide()
        {
            Visible = false;
        }

        protected void OnVisibilityChanged(IVisibleItem item, VisibilityChangeEventArgs args)
        {
            var handler = VisibilityChanged;
            CloudLog.Debug("{0} ({1}) : {2}", GetType().Name, VisibleJoin.Number, args.EventType);
            switch (args.EventType)
            {
                case VisibilityChangeEventType.WillShow:
                    try
                    {
                        WillShow();
                    }
                    catch (Exception e)
                    {
                        CloudLog.Exception(e);
                    }
                    break;
                case VisibilityChangeEventType.DidShow:
                    _uiController.Activity += UIControllerOnActivity;
                    try
                    {
                        DidShow();
                    }
                    catch (Exception e)
                    {
                        CloudLog.Exception(e);
                    }
                    break;
                case VisibilityChangeEventType.WillHide:
                    _uiController.Activity -= UIControllerOnActivity;
                    try
                    {
                        WillHide();
                    }
                    catch (Exception e)
                    {
                        CloudLog.Exception(e);
                    }
                    break;
                case VisibilityChangeEventType.DidHide:
                    try
                    {
                        DidHide();
                    }
                    catch (Exception e)
                    {
                        CloudLog.Exception(e);
                    }
                    if (_timeOut != null && !_timeOut.Disposed)
                    {
                        _timeOut.Dispose();
                    }
                    break;
            }
            if (handler != null) handler(item, args);
        }

        private void UIControllerOnActivity(UIController uiController)
        {
            if (!Visible || _timeOutTime.Equals(TimeSpan.Zero)) return;
            CloudLog.Debug("{0} Activity - Resetting Timeout", GetType().Name);
            SetTimeOut(_timeOutTime);
        }

        protected abstract void WillShow();

        protected abstract void DidShow();

        protected abstract void WillHide();

        protected abstract void DidHide();

        private void ParentVisibilityChanged(IVisibleItem item, VisibilityChangeEventArgs args)
        {
            if (args.EventType == VisibilityChangeEventType.DidHide)
            {
                _parentViewHiding = true;
                Hide();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (Disposed) return;
            if (disposing)
            {
                // Free other state (managed objects).
                    
            }
            // Free your own state (unmanaged objects).
            // Set large fields to null.

            if (_parent != null)
                _parent.VisibilityChanged -= ParentVisibilityChanged;

            base.Dispose(disposing);
        }

        #endregion
    }
}