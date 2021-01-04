using Crestron.SimplSharpPro;

namespace UX.Lib2.UI
{
    public interface IAnalogTouch : IAnalogFeedback
    {
        #region Events

        event AnalogTouchValueChangeHandler AnalogValueChanged;

        #endregion

        #region Properties

        UShortOutputSig AnalogTouchJoin { get; }

        #endregion

        #region Methods

        #endregion
    }

    public delegate void AnalogTouchValueChangeHandler(IAnalogTouch item, ushort value);
    public delegate void AnalogTouchReleasedHandler(IAnalogTouch item, ushort value);
}