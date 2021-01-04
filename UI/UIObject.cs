using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace UX.Lib2.UI
{
    /// <summary>
    /// A generic UI object for a device or smartobject
    /// </summary>
    public abstract class UIObject : IUIObject, IDisposable
    {
        #region Fields

        private bool _sigChangesRegistered;
        private string _name;
        private bool _disposed;

        #endregion

        #region Constructors

        /// <summary>
        /// The default Constructor.
        /// </summary>
        protected UIObject(BasicTriList device)
        {
            Owner = device;
            Type = UIObjectType.DeviceJoinObject;
        }

        protected UIObject(SmartObject smartObject)
        {
            Owner = smartObject;
            Type = UIObjectType.SmartGraphicJoinObject;
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
        /// This will either be a panel or gateway device itself or a smart graphic extender object.
        /// Should be cast depending on the state of the Type property.
        /// </summary>
        public object Owner { get; private set; }

        /// <summary>
        /// The device to which this object belongs
        /// </summary>
        public GenericBase Device
        {
            get
            {
                if (Type == UIObjectType.SmartGraphicJoinObject)
                    return ((SmartObject) Owner).Device;
                return Owner as GenericBase;
            }
        }

        /// <summary>
        /// This defines the type of object which owns this object.
        /// </summary>
        public UIObjectType Type { get; private set; }

        /// <summary>
        /// Set or get the name of the UI item
        /// </summary>
        public string Name
        {
            get
            {
                return string.IsNullOrEmpty(_name) ? ToString() : _name;
            }
            set { _name = value; }
        }

        /// <summary>
        /// True if item is disposed
        /// </summary>
        public bool Disposed
        {
            get { return _disposed; }
        }

        #endregion

        #region Methods

        internal void RegisterToSigChanges()
        {
            if (_sigChangesRegistered) return;
            switch (Type)
            {
                case UIObjectType.DeviceJoinObject:
                    ((BasicTriList) Owner).SigChange += OnSigChange;
                    break;
                case UIObjectType.SmartGraphicJoinObject:
                    ((SmartObject) Owner).SigChange += OnSigChange;
                    break;
            }
            _sigChangesRegistered = true;
        }

        internal void UnregisterToSigChanges()
        {
            if (!_sigChangesRegistered) return;
            switch (Type)
            {
                case UIObjectType.DeviceJoinObject:
                    ((BasicTriList) Owner).SigChange -= OnSigChange;
                    break;
                case UIObjectType.SmartGraphicJoinObject:
                    ((SmartObject) Owner).SigChange -= OnSigChange;
                    break;
            }
            _sigChangesRegistered = false;
        }

        protected abstract void OnSigChange(GenericBase owner, SigEventArgs args);

        public void Dispose()
        {
            Dispose(true);
            CrestronEnvironment.GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Free other state (managed objects).

                }
                // Free your own state (unmanaged objects).
                // Set large fields to null.

                UnregisterToSigChanges();

                _disposed = true;
            }
        }

        #endregion
    }
}