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
using System.Text;
using Crestron.SimplSharp.CrestronAuthentication;
using Crestron.SimplSharp.Net.Http;
using Crestron.SimplSharp.Reflection;
using UX.Lib2.Cloud.Logger;
using UX.Lib2.Models;
using UX.Lib2.WebApp.Templates;

namespace UX.Lib2.WebScripting2
{
    public abstract class BaseRequestHandler
    {
        #region Fields

        private readonly SystemBase _system;
        private readonly Request _request;
        private readonly bool _loginRequired;

        #endregion

        #region Constructors

        protected BaseRequestHandler(SystemBase system, Request request, bool loginRequired)
        {
            _system = system;
            _request = request;
            _loginRequired = loginRequired;
#if DEBUG
            Debug.WriteInfo(GetType().Name, "{0} Path: {1}", request.Header.RequestType, request.Path);
            Debug.WriteInfo("  Header", request.Header.ToString());
            if (request.RequestType == RequestType.Post)
            {
                Debug.WriteNormal(Debug.AnsiPurple + request.ContentString + Debug.AnsiReset);
            }
#endif
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events
        #endregion

        #region Delegates
        #endregion

        #region Properties

        public SystemBase System
        {
            get { return _system; }
        }

        public Request Request
        {
            get { return _request; }
        }

        public bool LoggedIn
        {
            get { return Authentication.Enabled && Request.User.Authenticated; }
        }

        #endregion

        #region Methods

        internal virtual void Process()
        {
            try
            {
                _request.Response.Header.AddHeader(new HttpHeader("X-App-Handler", GetType().FullName));

                var method = GetType()
                    .GetCType()
                    .GetMethod(_request.Header.RequestType,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase | BindingFlags.Instance, null,
                        new CType[] {}, null);

                if (method == null)
                {
                    HandleError(405, "Method not allowed",
                        string.Format("{0} does not allow method \"{1}\"", GetType().Name, _request.Header.RequestType));
                    return;
                }

                try
                {
                    method.Invoke(this, new object[] {});
                }
                catch (Exception e)
                {
                    CloudLog.Exception(e);
                    HandleError(e);
                }

                try
                {
                    //_request.Response.FinalizeHeader();
                }
                catch (Exception e)
                {
                    CloudLog.Exception(e);
                    HandleError(e);
                }
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }

        protected bool RequireLogin(bool redirectToLogin)
        {
            if (!System.Booted)
            {
                Redirect("/boot");
            }
            if (!Authentication.Enabled || Request.User.Authenticated)
            {
                return false;
            }
            if (!_loginRequired)
            {
                return false;
            }
            if (redirectToLogin)
            {
                Redirect("/login?postloginurl={0}", Request.PathAndQueryString);
            }
            else
            {
                HandleError(401, "Unauthorized", "Please login to begin a session");
            }
            return true;
        }

        protected void Redirect(string url, params object[] args)
        {
            Request.Response.Code = 302;
            Request.Response.Header.AddHeader(new HttpHeader("Location: " + string.Format(url, args)));
            //Request.Response.FinalizeHeader();
        }

        protected virtual void HandleError(Exception e)
        {
            Request.Server.HandleError(Request.Context, e);
        }

        protected virtual void HandleError(int code, string title, string message)
        {
            Request.Server.HandleError(Request.Context, code, title, message);
        }

        protected virtual void HandleNotFound()
        {
            Request.Server.HandleNotFound(Request.Context);
        }

        public virtual void WriteResponse(TemplateEngine template)
        {
            Request.Response.Header.ContentType = "text/html; charset=UTF-8";
            WriteResponse(Encoding.UTF8.GetBytes(template.Render()));
        }

        public virtual void WriteResponse(byte[] bytes)
        {
            Request.Response.ContentSource = ContentSource.ContentBytes;
            Request.Response.ContentBytes = bytes;
            var time = DateTime.Now - Request.Time;
            Request.Response.Header.AddHeader(new HttpHeader("X-App-ProcessTime", time.TotalMilliseconds.ToString()));
        }

        public virtual void WriteResponse(string content, bool isFinal)
        {
            Request.Response.ContentSource = ContentSource.ContentString;
            Request.Response.ContentString = Request.Response.ContentString + content;
            var time = DateTime.Now - Request.Time;
            Request.Response.Header.AddHeader(new HttpHeader("X-App-ProcessTime", time.TotalMilliseconds.ToString()));

            if (isFinal)
            {
                //Request.Response.FinalizeHeader();
            }
        }

        #endregion
    }
}