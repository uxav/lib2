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
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;
using UX.Lib2.Models;
using UX.Lib2.WebApp.Templates;
using UX.Lib2.WebScripting2;

namespace UX.Lib2.WebApp
{
    public class DashboardHandler : BaseRequestHandler
    {
        #region Fields
        #endregion

        #region Constructors

        public DashboardHandler(SystemBase system, Request request, bool loginRequired)
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

        public virtual string ContentUrl
        {
            get { return string.Format("/dashboard/content/{0}", Request.PathArguments["page"]); }
        }

        #endregion

        #region Methods

        public void Get()
        {
            if(RequireLogin(true)) return;

            try
            {
                var template = new TemplateEngine(Assembly.GetExecutingAssembly(), "WebApp.Templates.dashboard_base.html",
                    InitialParametersClass.ControllerPromptName + " - Dashboard", LoggedIn);

                template.Context["system_name"] = System.Name;
                template.Context["content_url"] = ContentUrl;
                template.Context["has_xpanels"] = Request.Server.WebApp.XPanelLinks.Any();
                template.Context["xpanel_links"] = Request.Server.WebApp.XPanelLinks;
                template.Context["page_links"] = Request.Server.WebApp.UserPageLinks;
                var badgeClass = "primary";
                var messages = System.StatusMessages.ToArray();
                var errorCount = messages.Count(m => m.MessageLevel == StatusMessageWarningLevel.Error);
                var warningCount = messages.Count(m => m.MessageLevel == StatusMessageWarningLevel.Warning);
                if (errorCount > 0)
                {
                    badgeClass = "danger";
                } 
                else if (warningCount > 0)
                {
                    badgeClass = "warning";
                }
                var badgeCount = errorCount + warningCount;
                
                template.Context["diagnostics_badge_type"] = badgeClass;
                template.Context["diagnostics_badge_count"] = badgeCount > 0
                    ? badgeCount.ToString()
                    : string.Empty;

                var custromScriptUrl = System.WebApp.DashboardCustomScriptPath;
                if (!string.IsNullOrEmpty(custromScriptUrl))
                {
                    template.Context["custom_script"] = string.Format("<script src=\"{0}\"></script>", custromScriptUrl);
                }
                else
                {
                    template.Context["custom_script"] = string.Empty;
                }

                AddToTemplate(ref template);

                WriteResponse(template);
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }

        public virtual void AddToTemplate(ref TemplateEngine template)
        {
            
        }

        public class SMessage
        {
            public string Title { get; set; }
            public string Level { get; set; }
            public string Message { get; set; }
        }

        #endregion
    }
}