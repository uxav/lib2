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

using System;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.Reflection;
using UX.Lib2.Models;

namespace UX.Lib2.WebScripting2
{
    public class InternalResourceFileHandler : FileHandlerBase
    {
        public InternalResourceFileHandler(SystemBase system, Request request, bool loginRequired)
            : base(system, request, loginRequired)
        {
        }

        protected override string RootFilePath
        {
            get { return "UX.Lib2.WebScripting2.StaticFiles"; }
        }

        protected override Stream GetResourceStream(Assembly assembly, string fileName)
        {
            SetCacheTime(TimeSpan.FromHours(1));
            return base.GetResourceStream(assembly, fileName);
        }
    }
}