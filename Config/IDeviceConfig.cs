namespace UX.Lib2.Config
{
    public interface IDeviceConfig : IConfig
    {
        #region Properties

        /// <summary>
        /// Use for Hostname or IP Address
        /// </summary>
        string DeviceAddressString { get; set; }
        /// <summary>
        /// Use for Serial / IR port number or IPID / Cresnet ID
        /// </summary>
        uint DeviceAddressNumber { get; set; }
        DeviceConnectionType DeviceConnectionType { get; set; }

        #endregion

        #region Methods

        #endregion
    }

    public enum DeviceConnectionType
    {
        Network,
        Serial,
        IR,
        Cresnet,
        Cec,
        RF
    }
}