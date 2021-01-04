using Crestron.SimplSharpPro;

namespace UX.Lib2.UI
{
    public interface ISerialItem : IUIObject
    {
        /// <summary>
        /// The serial join to the device or smartobject
        /// </summary>
        StringInputSig SerialInputJoin { get; }
    }
}