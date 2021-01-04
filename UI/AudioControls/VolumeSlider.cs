using Crestron.SimplSharpPro.DeviceSupport;
using UX.Lib2.Models;

namespace UX.Lib2.UI.AudioControls
{
    public class VolumeSlider : UISlider
    {
        #region Fields

        private IAudioLevelControl _control;

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public VolumeSlider(BasicTriList device, uint analogTouchNumber)
            : base(device, analogTouchNumber, true)
        {
        }

        public VolumeSlider(UIController uiController, uint analogTouchNumber)
            : base(uiController, analogTouchNumber, true)
        {
        }

        public VolumeSlider(UIViewController view, uint analogTouchNumber)
            : base(view, analogTouchNumber, true)
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

        public IAudioLevelControl LevelControl
        {
            get { return _control; }
            set
            {
                if (_control == value) return;

                if (_control != null)
                {
                    _control.LevelChange -= SetValue;
                    AnalogValueChanged -= OnLevelChange;
                }

                _control = value;

                Visible = _control != null;

                if (_control == null) return;

                _control.LevelChange += SetValue;
                AnalogValueChanged += OnLevelChange;
                AnalogValue = _control.Level;
            }
        }

        private void OnLevelChange(IAnalogTouch item, ushort value)
        {
            _control.Level = value;
        }

        #endregion

        #region Methods

        #region Overrides of UIObject

        protected override void Dispose(bool disposing)
        {
            LevelControl = null;

            base.Dispose(disposing);
        }

        #endregion

        #endregion
    }
}