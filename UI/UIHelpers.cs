using Crestron.SimplSharpPro.DeviceSupport;

namespace UX.Lib2.UI
{
    public static class UIHelpers
    {
        public static string GetTextEntryValue(BasicTriList device, uint serialJoin)
        {
            return string.IsNullOrEmpty(device.StringOutput[serialJoin].StringValue)
                ? device.StringOutput[serialJoin].StringValue
                : device.StringInput[serialJoin].StringValue;
        }
    }
}