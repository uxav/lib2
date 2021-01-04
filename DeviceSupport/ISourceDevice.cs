/* License
 * ------------------------------------------------------------------------------
 * Copyright (c) 2019 UX Digital Systems Ltd
 *
 * Permission is hereby granted, to any person obtaining a copy of this software
 * and associated documentation files (the "Software"), to deal in the Software
 * for the continued use and development of the system on which it was installed,
 * and to permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * Any persons obtaining the software have no rights to use, copy, modify, merge,
 * publish, distribute, sublicense, and/or sell copies of the Software without
 * written persmission from UX Digital Systems Ltd, if it is not for use on the
 * system on which it was originally installed.
 * ------------------------------------------------------------------------------
 * UX.Digital
 * ----------
 * http://ux.digital
 * support@ux.digital
 */

namespace UX.Lib2.DeviceSupport
{
    public interface ISourceDevice : IDevice
    {
        /// <summary>
        /// Called when the source is selected in a room so the device can update it's details with the UI
        /// </summary>
        void UpdateOnSourceRequest();

        /// <summary>
        /// Called when the source has a room count equal to 1 when selected to begin playback or wake
        /// </summary>
        void StartPlaying();

        /// <summary>
        /// Called when the source has a room count equal to 0 when deselected to end playback or sleep
        /// </summary>
        void StopPlaying();

        /// <summary>
        /// Initialize the source device on system start
        /// </summary>
        void Initialize();
    }
}