using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using UX.Lib2.Cloud.Logger;
using UX.Lib2.Models;

namespace UX.Lib2.UI
{
    public abstract class UIControllerWithSmartObjects : UIController
    {
        #region Fields

        #endregion

        #region Constructors

        /// <summary>
        /// Create a UIController instance
        /// </summary>
        /// <param name="system">The base system</param>
        /// <param name="device">The UI device used for the UIController</param>
        /// <param name="defaultRoom">The default room for the UI</param>
        protected UIControllerWithSmartObjects(SystemBase system, BasicTriListWithSmartObject device, RoomBase defaultRoom)
            : base(system, device, defaultRoom)
        {

        }

        #endregion

        #region Finalizers
        #endregion

        #region Events
        #endregion

        #region Delegates
        #endregion

        #region Properties

        #region Overrides of UIController

        /// <summary>
        /// The UI Device or Gateway
        /// </summary>
        public new BasicTriListWithSmartObject Device
        {
            get { return base.Device as BasicTriListWithSmartObject; }
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Load the smartobject from a resource stream
        /// </summary>
        /// <param name="stream">The stream containing the smartobject definitions</param>
        public void LoadSmartObjects(Crestron.SimplSharp.CrestronIO.Stream stream)
        {
            Device.LoadSmartObjects(stream);

            foreach (var o in Device.SmartObjects)
            {
                o.Value.SigChange += SmartObjectsOnSigChange;
            }
        }

        private void SmartObjectsOnSigChange(GenericBase currentDevice, SmartObjectEventArgs args)
        {
#if DEBUG
            Debug.WriteInfo(currentDevice.Name + " SmartObjectsOnSigChange(), {0}, {1}", args.Event, args.Sig);
#endif
            if (args.Event == eSigEvent.BoolChange)
                OnActivity(this);
        }

        #endregion
    }
}