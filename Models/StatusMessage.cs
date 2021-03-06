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

using Crestron.SimplSharpPro;

namespace UX.Lib2.Models
{
    public class StatusMessage : IStatusMessageItem
    {
        private string _sourceDeviceName = string.Empty;

        public StatusMessage(StatusMessageWarningLevel level, string message, params object[] args)
        {
            MessageLevel = level;
            MessageString = string.Format(message, args);
        }

        public StatusMessageWarningLevel MessageLevel { get; private set; }
        public string MessageString { get; set; }

        public string SourceDeviceName
        {
            get { return _sourceDeviceName; }
            set { _sourceDeviceName = value; }
        }
    }
}