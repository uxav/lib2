using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.CrestronThread;
using UX.Lib2.Models;
using UX.Lib2.UI.Formatters;

namespace UX.Lib2.UI
{
    public class UIControllerCollection : IEnumerable<UIController>
    {
        #region Fields

        readonly Dictionary<uint, UIController> _controllers = new Dictionary<uint, UIController>();
        private uint _timeTextJoinNumber;
        private IFormatProvider _timeFormat;
        private uint _dateTextJoinNumber;
        private IFormatProvider _dateFormat;

        #endregion

        #region Constructors

        /// <summary>
        /// The default Constructor.
        /// </summary>
        internal UIControllerCollection(SystemBase system)
        {
            System = system;
        }

        internal UIControllerCollection(SystemBase system, IEnumerable<UIController> controllers)
        {
            System = system;
            foreach (var uiController in controllers)
            {
                _controllers.Add(uiController.Id, uiController);
            }
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events
        #endregion

        #region Delegates
        #endregion

        public UIController this[uint id]
        {
            get { return _controllers[id]; }
        }

        #region Properties

        /// <summary>
        /// The System which owns the collection
        /// </summary>
        public SystemBase System { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Returns true if the collection contains the ID key
        /// </summary>
        /// <param name="id">The ID for the UIController</param>
        /// <returns>True if exists</returns>
        public bool ContainsKey(uint id)
        {
            return _controllers.ContainsKey(id);
        }

        internal void Add(UIController controller)
        {
            _controllers.Add(controller.Id, controller);
        }

        /// <summary>
        /// Initialize the collection of UI Controllers
        /// </summary>
        internal void Initialize()
        {
#if DEBUG
            Debug.WriteInfo(GetType().Name + ".Initialize()");
#endif
            OnTimeChange();
            UpdateDate(DateTime.Now);

            System.TimeChanged += (system, time) => OnTimeChange();

            foreach (var uiController in this)
            {
                uiController.InternalInitialize();
            }
        }

        public void SetupCustomTime(uint textJoin)
        {
            SetupCustomTime(textJoin, new DefaultTimeFormatter());
        }

        public void SetupCustomTime(uint textJoin, IFormatProvider timeFormat)
        {
            _timeTextJoinNumber = textJoin;
            _timeFormat = timeFormat;
        }

        public void SetupCustomDate(uint textJoin)
        {
            SetupCustomDate(textJoin, new DefaultDateFormatter());
        }

        public void SetupCustomDate(uint textJoin, IFormatProvider timeFormat)
        {
            _dateTextJoinNumber = textJoin;
            _dateFormat = timeFormat;
        }

        public UIControllerCollection ForRoom(RoomBase room)
        {
            return new UIControllerCollection(System, _controllers.Values.Where(ui => ui.Room == room));
        }

        public UIControllerCollection ForDefaultRoom(RoomBase room)
        {
            return new UIControllerCollection(System, _controllers.Values.Where(ui => ui.DefaultRoom == room));
        }

        private void OnTimeChange()
        {
            var time = DateTime.Now;

            if (_timeTextJoinNumber > 0 && _timeFormat != null)
            {
                var timeString = string.Format(_timeFormat, "{0}", time);
                foreach (var ui in this)
                {
                    ui.Device.StringInput[_timeTextJoinNumber].StringValue = timeString;
                }
            }

            if(time.Hour != 0 && time.Minute != 0) return;

            UpdateDate(time);
        }

        private void UpdateDate(DateTime time)
        {
            if (_timeTextJoinNumber <= 0 || _timeFormat == null) return;

            var dateString = string.Format(_dateFormat, "{0}", time);
            foreach (var ui in this)
            {
                ui.Device.StringInput[_dateTextJoinNumber].StringValue = dateString;
            }
        }

        public void ConnectToDefaultRooms()
        {
            foreach (var ui in _controllers.Values)
            {
                ui.ConnectToDefaultRoom();
            }
        }

        public IEnumerator<UIController> GetEnumerator()
        {
            return _controllers.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}