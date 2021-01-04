using Crestron.SimplSharpPro;

namespace UX.Lib2.UI
{
// ReSharper disable once InconsistentNaming
    public interface IUIObject
    {
        #region Properties

        /// <summary>
        /// This will either be a panel device itself or a smart graphic extender object.
        /// Should be cast depending on the state of the Type property.
        /// </summary>
        object Owner { get; }

        /// <summary>
        /// The device to which this object belongs
        /// </summary>
        GenericBase Device { get; }

        /// <summary>
        /// This defines the type of object which owns this object.
        /// </summary>
        UIObjectType Type { get; }

        /// <summary>
        /// Set or get the name of the UI item
        /// </summary>
        string Name { get; set; }

        #endregion

        #region Methods

        #endregion
    }

// ReSharper disable once InconsistentNaming
    public enum UIObjectType
    {
        /// <summary>
        /// This type of object is tied to joins on the main collection from the device
        /// </summary>
        DeviceJoinObject,
        /// <summary>
        /// This type of object is tied to joins from a smart object extender
        /// </summary>
        SmartGraphicJoinObject
    }
}