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
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronAuthentication;
using Crestron.SimplSharp.Net;
using Crestron.SimplSharp.Reflection;
using UX.Lib2.Models;
using UX.Lib2.WebApp.Templates;
using UX.Lib2.WebScripting2;

namespace UX.Lib2.WebApp
{
    public class LoginHandler : BaseRequestHandler
    {
        #region Fields
        #endregion

        #region Constructors

        public LoginHandler(SystemBase system, Request request, bool loginRequired)
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

        protected void Get()
        {
            try
            {
                var postloginurl = Request.QueryString["postloginurl"];
#if DEBUG
                Debug.WriteInfo("Login page request");
#endif
                if (!Authentication.Enabled || Request.User.Authenticated)
                {
                    Redirect(String.IsNullOrEmpty(postloginurl) ? "/" : postloginurl); 
                    return;
                }
                var template = new TemplateEngine(Assembly.GetExecutingAssembly(), "WebApp.Templates.login.html",
                    InitialParametersClass.ControllerPromptName + " - User Login", LoggedIn);
                template.Context["post_login_url"] = postloginurl;

                if (
                    (from object key in Request.QueryString.Keys select key as string).Any(
                        queryKey => queryKey == "badlogin"))
                {
                    template.Context["login_fail"] =
                        @"<div class=""alert alert-danger"" role=""alert"">Login was not successfull</div>";
                }
                else
                {
                    template.Context["login_fail"] = "";
                }

                template.Context["page_style_link"] = @"<link href=""/static/lib2/css/signin.css"" rel=""stylesheet"">";
                WriteResponse(template);
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }

        protected void Post()
        {
#if DEBUG
            Debug.WriteInfo("Login request", "\r\n{0}", Request.ContentString);
#endif
            var postloginurl = Request.QueryString["postloginurl"];

            try
            {
                var formData = new Dictionary<string, string>();
                var content = HttpUtility.UrlDecode(Request.ContentString);
                foreach (Match match in Regex.Matches(content, @"([^&=]+)=([^&=]+)"))
                {
                    formData[match.Groups[1].Value] = match.Groups[2].Value;
                }
                var session = SessionManager.StartSession(formData["username"], formData["password"]);
                if (session != null)
                {
                    var header = session.Validate();
                    if (header != null)
                    {
                        Request.Response.Header.AddHeader(header);
                        Redirect(String.IsNullOrEmpty(postloginurl) ? "/" : postloginurl);
                        return;
                    }
                }

                Redirect("/login?" + Request.QueryString + "&badlogin=true");
                
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }

        #endregion
    }
}