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
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UX.Lib2.Cloud.Logger;
using UX.Lib2.Config;
using UX.Lib2.Models;
using UX.Lib2.WebApp.Templates;
using UX.Lib2.WebScripting2;

namespace UX.Lib2.WebApp
{
    public class DashboardContentHandler : BaseRequestHandler
    {
        #region Fields
        #endregion

        #region Constructors

        public DashboardContentHandler(SystemBase system, Request request, bool loginRequired)
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
            if (RequireLogin(false)) return;

            try
            {
                var pageName = Request.PathArguments["page"];
                var assembly = Assembly.GetExecutingAssembly();
                var template = new TemplateEngine(assembly, string.Format("WebApp.Templates.Dashboard.{0}.html", pageName),
                    null, LoggedIn);
                
                var method = GetType()
                    .GetCType()
                    .GetMethod(pageName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase, null, new CType[] { typeof(TemplateEngine) }, null);
                method.Invoke(this, new object[] {template});
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }

        public void Diagnostics(TemplateEngine template)
        {
            try
            {
                var statusMessages = System.StatusMessages.OrderByDescending(m => m.MessageLevel).ToList();
                if (statusMessages.Count == 0)
                {
                    statusMessages.Add(new StatusMessage(StatusMessageWarningLevel.Ok, "All systems working OK!"));
                }

                var messages = (from item in statusMessages
                    let title =
                        String.IsNullOrEmpty(item.SourceDeviceName)
                            ? ": &nbsp;&nbsp;"
                            : " - " + item.SourceDeviceName + "<hr>"
                    select new DashboardHandler.SMessage
                    {
                        Title = "<b>" + item.MessageLevel + "</b>" + title,
                        Level = item.BootStrapAlertClass(),
                        Message = item.MessageString
                    });

                template.Context.Add("status_messages", messages);

                WriteResponse(template);
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }

        public void Logs(TemplateEngine template)
        {
            try
            {
                WriteResponse(template);
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }

        public void Config(TemplateEngine template)
        {
            try
            {
                var config = System.Config;
                var data = JToken.FromObject(config);
                template.Context["config_data"] = data.ToString(Formatting.Indented);
                WriteResponse(template);
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }

        public void Home(TemplateEngine template)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var sw = new StringBuilder();
                foreach (var manifestResourceName in assembly.GetManifestResourceNames())
                {
                    sw.AppendLine(manifestResourceName);
                }
                template.Context["lib_resource_streams"] = sw.ToString();
                sw = new StringBuilder();
                foreach (var manifestResourceName in System.AppAssembly.GetManifestResourceNames())
                {
                    sw.AppendLine(manifestResourceName);
                }
                template.Context["app_resource_streams"] = sw.ToString();
                template.Context["app_number"] = InitialParametersClass.ApplicationNumber;
                template.Context["system_monitor_running"] = SystemMonitor.Running;
                WriteResponse(template);
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }

        public void Service(TemplateEngine template)
        {
            try
            {
                WriteResponse(template);
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }

        public void AVControl(TemplateEngine template)
        {
            try
            {
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