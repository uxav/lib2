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
using Crestron.SimplSharp.Net;
using Crestron.SimplSharp.Net.Http;
using Crestron.SimplSharp.Reflection;
using UX.Lib2.Cloud.Logger;
using UX.Lib2.Models;
using UX.Lib2.WebApp.Templates;

namespace UX.Lib2.WebScripting2
{
    public class WebScriptingServer
    {

        #region Fields

        private readonly SystemBase _system;
        private readonly HttpServer _server;
        private readonly WebApp.WebApp _webApp;
        private readonly Dictionary<string, List<string>> _keyNames = new Dictionary<string, List<string>>();
        private readonly Dictionary<string, CType> _handlers = new Dictionary<string, CType>();
        private readonly Dictionary<string, bool> _loginRequired = new Dictionary<string, bool>(); 
        private readonly Dictionary<string, string> _redirects = new Dictionary<string, string>(); 

        #endregion

        #region Constructors

        public WebScriptingServer(SystemBase system, int port)
        {
            _system = system;
            _server = new HttpServer()
            {
                Port = port,
                ServerName = "UX WebScripting Server: " + system.Name
            };
            _server.OnHttpRequest += OnHttpRequest;
            _server.Open();
            CrestronEnvironment.ProgramStatusEventHandler += type =>
            {
                if (type == eProgramStatusEventType.Stopping)
                {
                    _server.Close();
                    _server.Dispose();
                }
            };

            AddRoute(@"/static/lib2/<filepath:[\/\w\.\-\[\]\(\)\x20]+>", typeof(InternalResourceFileHandler));
        }

        internal WebScriptingServer(SystemBase system, int port, WebApp.WebApp webApp)
            : this(system, port)
        {
            _webApp = webApp;
        }

        #endregion

        #region Finalizers

        #endregion

        #region Events

        #endregion

        #region Delegates

        #endregion

        #region Properties

        internal HttpServer HttpServer
        {
            get { return _server; }
        }

        public WebApp.WebApp WebApp
        {
            get { return _webApp; }
        }

        #endregion

        #region Methods

        private void OnHttpRequest(object sender, OnHttpRequestArgs context)
        {
            try
            {
                var decodedPath = HttpUtility.UrlDecode(context.Request.Path);
#if DEBUG
                var remoteAddress = context.Request.DataConnection.RemoteEndPointAddress;
                //var hostName = Dns.GetHostEntry(remoteAddress).HostName;
                Debug.WriteInfo("New WebScripting Request", "From {0}, Path \"{1}\"", remoteAddress, decodedPath);
#endif
                foreach (var redirect in _redirects)
                {
                    var pattern = redirect.Key;

                    var match = Regex.Match(decodedPath, pattern);

                    if (!match.Success) continue;
#if DEBUG
                    Debug.WriteSuccess("Redirect found!", "Pattern: \"{0}\"", pattern);
#endif
                    var queryString = context.Request.QueryString.ToString();
                    if (queryString.Length > 0 && !queryString.StartsWith("?"))
                    {
                        queryString = "?" + queryString;
                    }
                    context.Response.Header.AddHeader(
                        new HttpHeader(string.Format("Location: {0}{1}", redirect.Value, queryString)));
                    context.Response.Code = 302;
                    //context.Response.FinalizeHeader();
                    return;
                }

                foreach (var handler in _handlers)
                {
                    var pattern = handler.Key;
                    
                    var match = Regex.Match(decodedPath, pattern);

                    if (!match.Success) continue;
#if DEBUG
                    Debug.WriteSuccess("Handler found!", "Pattern: \"{0}\"", pattern);
#endif
                    var loginRequired = _loginRequired[handler.Key];

                    try
                    {
                        var keyNames = _keyNames[pattern];
                        var args = new Dictionary<string, string>();
                        var index = 0;
                        foreach (var keyName in keyNames)
                        {
                            if (keyName.Length > 0)
                            {
                                args[keyName] = match.Groups[index + 1].Value;
                            }
                            index ++;
                        }
#if DEBUG
                        foreach (var arg in args)
                        {
                            Debug.WriteInfo("  " + arg.Key, arg.Value);
                        }
#endif
                        var request = new Request(this, context, args);
                        var ctor =
                            handler.Value.GetConstructor(
                                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null,
                                new CType[] {typeof (SystemBase), typeof (Request), typeof(bool)}, null);
                        var instance = ctor.Invoke(new object[] {_system, request, loginRequired}) as BaseRequestHandler;
                        if (instance == null) continue;
                        instance.Process();
                        return;
                    }
                    catch (Exception e)
                    {
#if DEBUG                        
                        CloudLog.Exception(e);
#endif
                        HandleError(context, e);
                        return;
                    }
                }
#if DEBUG
                Debug.WriteError("No handler found for request!");
#endif
                HandleNotFound(context);
            }
            catch (Exception e)
            {
                HandleError(context, e);
            }
        }

        internal void HandleError(OnHttpRequestArgs args, Exception e)
        {
            HandleError(args, 500, "Server Error - " + e.GetType().Name, e.Message + "\r\n" + e.StackTrace);
        }

        internal void HandleNotFound(OnHttpRequestArgs args)
        {
            HandleError(args, 404, "Not Found", "The resource could not be found");
        }

        internal void HandleError(OnHttpRequestArgs args, int code, string title, string message)
        {
//#if DEBUG
            CloudLog.Warn("Webserver Error {0}, {1}, Path: {2}\r\n{3}", code, title, args.Request.Path, message);
//#endif
            try
            {
                var errorTemplate = new TemplateEngine(Assembly.GetExecutingAssembly(), "WebApp.Templates.error.html",
                    "Error" + code, false);
                errorTemplate.Context.Add("error_code", code.ToString());
                errorTemplate.Context.Add("error_title", title);
                errorTemplate.Context.Add("error_message", message);
                errorTemplate.Context["page_style_link"] =
                    @"<link href=""/static/lib2/css/error.css"" rel=""stylesheet"">";
                args.Response.Code = code;
                args.Response.ResponseText = title;
                args.Response.Header.ContentType = "text/html";
                args.Response.ContentSource = ContentSource.ContentString;
                args.Response.ContentString = errorTemplate.Render();
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }
            try
            {
                //args.Response.FinalizeHeader();
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }
        }

        public void AddRedirect(string routePattern, string redirectUrl)
        {
            var finalPattern = Regex.Replace(routePattern, @"\/([^\s<\/]+)|\/<(\w*)(?::([^\s>]+))?>|\/",
                delegate(Match match)
                {
                    if (!String.IsNullOrEmpty(match.Groups[1].Value))
                    {
                        return @"\/" + match.Groups[1].Value;
                    }

                    if (!String.IsNullOrEmpty(match.Groups[3].Value))
                    {
                        return @"\/(" + match.Groups[3].Value + ")";
                    }

                    return !String.IsNullOrEmpty(match.Groups[2].Value) ? @"\/(\w+)" : @"\/";
                });

            finalPattern = "^" + finalPattern + "$";

            _redirects[finalPattern] = redirectUrl;
        }

        public void AddRoute(string routePattern, CType handlerType, bool requireLogin)
        {
            if (!handlerType.IsSubclassOf(typeof(BaseRequestHandler)))
                throw new Exception(string.Format("Type \"{0}\" is not derrived from {1}", handlerType.Name,
                    typeof (BaseRequestHandler).Name));

            var keyNames =
                (from Match match in Regex.Matches(routePattern, @"\/<(\w*)(?::([^\s>]+))?>")
                    select match.Groups[1].Value).ToList();

            var finalPattern = Regex.Replace(routePattern, @"\/([^\s<\/]+)|\/<(\w*)(?::([^\s>]+))?>|\/",
                delegate(Match match)
                {
                    if (!String.IsNullOrEmpty(match.Groups[1].Value))
                    {
                        return @"\/" + match.Groups[1].Value;
                    }

                    if (!String.IsNullOrEmpty(match.Groups[3].Value))
                    {
                        return @"\/(" + match.Groups[3].Value + ")";
                    }

                    return !String.IsNullOrEmpty(match.Groups[2].Value) ? @"\/(\w+)" : @"\/";
                });

            finalPattern = "^" + finalPattern + "$";

            _handlers[finalPattern] = handlerType;
            _keyNames[finalPattern] = keyNames;
            _loginRequired[finalPattern] = requireLogin;

#if DEBUG
            Debug.WriteInfo("Added handler", "\"{0}\"", finalPattern);

            var index = 0;
            foreach (var arg in keyNames)
            {
                Debug.WriteInfo("  Pattern Group " + (index + 1), arg);
                index ++;
            }
#endif
        }

        public void AddRoute(string routePattern, CType handlerType)
        {
            AddRoute(routePattern, handlerType, true);
        }

        #endregion
    }
}