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
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;
using UX.Lib2.Models;
using UX.Lib2.WebApp.Templates;
using UX.Lib2.WebScripting2;

namespace UX.Lib2.WebApp
{
    public class BootPageHandler : BaseRequestHandler
    {
        #region Fields
        #endregion

        #region Constructors

        public BootPageHandler(SystemBase system, Request request, bool loginRequired)
            : base(system, request, loginRequired)
        {
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events
        #endregion

        #region Delegates
        #endregion

        #region Properties
        #endregion

        #region Methods

        public void Get()
        {
            try
            {
                var action = Request.QueryString["action"];

                if (!string.IsNullOrEmpty(action))
                {
                    if (RequireLogin(false)) return;

                    switch (Request.QueryString["action"].ToLower())
                    {
                        case "restart":
                            System.Restart();
                            break;
                        case "reboot":
                            System.Reboot();
                            break;
                    }

                    Redirect(Request.Path);
                }

                var template = new TemplateEngine(Assembly.GetExecutingAssembly(), "WebApp.Templates.boot.html",
                    "System Booting", LoggedIn);
                template.Context["page_style_link"] = @"<link href=""/static/lib2/css/boot.css"" rel=""stylesheet"">";
                template.Context["status_text"] = System.BootStatus;

                WriteResponse(template);
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }

        #endregion
    }
}