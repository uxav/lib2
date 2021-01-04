using Crestron.SimplSharpPro;

namespace UX.Lib2.UI
{
    public interface IAnalogFeedback : IUIObject
    {
        #region Events
        #endregion

        #region Properties

        ushort AnalogValue { get; set; }

        UShortInputSig AnalogFeedbackJoin { get; }

        #endregion

        #region Methods

        void SetValue(ushort value);

        #endregion
    }
}