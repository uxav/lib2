using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UX.Lib2.Models
{
    public class AudioLevelCollection : IEnumerable<IAudioLevelControl>
    {
        #region Fields

        private readonly List<IAudioLevelControl> _controls;

        #endregion

        #region Constructors

        /// <summary>
        /// The default Constructor.
        /// </summary>
        public AudioLevelCollection()
        {
            _controls = new List<IAudioLevelControl>();
        }

        internal AudioLevelCollection(IEnumerable<IAudioLevelControl> fromControls)
        {
            _controls = new List<IAudioLevelControl>(fromControls);
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events
        #endregion

        #region Delegates
        #endregion

        #region Indexers

        public AudioLevelCollection this[AudioLevelType type]
        {
            get
            {
                return new AudioLevelCollection(this.Where(c => c.ControlType == type));
            }
        }

        #endregion

        #region Properties

        public AudioLevelCollection MuteControls
        {
            get
            {
                return new AudioLevelCollection(this.Where(c => c.SupportsMute));
            }
        }

        public AudioLevelCollection LevelControls
        {
            get
            {
                return new AudioLevelCollection(this.Where(c => c.SupportsLevel));
            }
        }

        #endregion

        #region Methods

        public void Add(IAudioLevelControl control)
        {
            if(_controls.Contains(control))
                throw new Exception("IAudioLevelControl instance already exists!");
            _controls.Add(control);
        }

        public void Clear()
        {
            _controls.Clear();
        }

        public IEnumerator<IAudioLevelControl> GetEnumerator()
        {
            return _controls.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}