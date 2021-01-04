using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace UX.Lib2.UI
{
    public class UIColorChip : UIObject
    {
        private readonly UShortInputSig _redJoin;
        private readonly UShortInputSig _greenJoin;
        private readonly UShortInputSig _blueJoin;

        public UIColorChip(BasicTriList device, uint redJoin, uint greenJoin, uint blueJoin)
            : base(device)
        {
            _redJoin = device.UShortInput[redJoin];
            _greenJoin = device.UShortInput[greenJoin];
            _blueJoin = device.UShortInput[blueJoin];
        }

        public UIColorChip(BasicTriList device, uint redJoin, uint greenJoin, uint blueJoin, UIColor defaultColor)
            : this(device, redJoin, greenJoin, blueJoin)
        {
            Color = defaultColor;
        }

        public UIColorChip(SmartObject smartObject, string redSigName, string greenSigName, string blueSigName)
            : base(smartObject)
        {
            _redJoin = smartObject.UShortInput[redSigName];
            _greenJoin = smartObject.UShortInput[greenSigName];
            _blueJoin = smartObject.UShortInput[blueSigName];
        }

        public UIColorChip(SmartObject smartObject, string redSigName, string greenSigName, string blueSigName,
            UIColor defaultColor)
            : this(smartObject, redSigName, greenSigName, blueSigName)
        {
            Color = defaultColor;
        }

        public UIColor Color
        {
            get { return new UIColor(_redJoin.UShortValue, _greenJoin.UShortValue, _blueJoin.UShortValue); }
            set
            {
                _redJoin.UShortValue = (ushort) value.Red;
                _greenJoin.UShortValue = (ushort) value.Green;
                _blueJoin.UShortValue = (ushort) value.Blue;
            }
        }

        protected override void OnSigChange(GenericBase owner, SigEventArgs args)
        {

        }
    }
}