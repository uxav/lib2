using System;
using Crestron.SimplSharp;
using Crestron.SimplSharp.WebScripting;
using UX.Lib2.Cloud.Logger;

namespace UX.Lib2.WebScripting
{
    public class WebScriptingServer
    {

        #region Fields

        private readonly string _path;
        private readonly HttpCwsServer _server;

        #endregion

        #region Constructors

        /// <summary>
        /// The default Constructor.
        /// </summary>
        /// <param name="path"></param>
        public WebScriptingServer(string path)
        {
            _path = path;
            _server = new HttpCwsServer(path);
            _server.Register();

            Debug.WriteSuccess("HttpCwsServer", "Started for path: {0}", _path);
            Debug.WriteInfo("HttpCwsServer", "Base URL: {0}", BaseUrlDns);
            Debug.WriteInfo("HttpCwsServer", "Base URL: {0}", BaseUrlIp);

            _server.ReceivedRequestEvent += (sender, args) =>
            {
#if DEBUG
                CrestronConsole.PrintLine("Incoming http request from {0} {1}", args.Context.Request.UserHostAddress, args.Context.Request.UserHostName);
                CrestronConsole.PrintLine("Request AbsolutePath: {0}", args.Context.Request.Url.AbsolutePath);
                CrestronConsole.PrintLine("Request AbsoluteUri: {0}", args.Context.Request.Url.AbsoluteUri);
                CrestronConsole.PrintLine("Request PhysicalPath: {0}", args.Context.Request.PhysicalPath);
                CrestronConsole.PrintLine("Request PathAndQuery: {0}", args.Context.Request.Url.PathAndQuery);
                CrestronConsole.PrintLine("Request Query: {0}", args.Context.Request.Url.Query);
                CrestronConsole.PrintLine("Request Path: {0}", args.Context.Request.Path);
                CrestronConsole.PrintLine("Request Method: {0}", args.Context.Request.HttpMethod);
#endif
                try
                {
                    if (args.Context.Request.RouteData != null)
                    {
#if DEBUG
                        CrestronConsole.PrintLine("Request handler: {0}",
                            args.Context.Request.RouteData.RouteHandler.GetType());
                        CrestronConsole.PrintLine("Route URL Pattern: {0}", args.Context.Request.RouteData.Route.Url);
#endif
                    }
                    else if (RootHandler == null)
                    {
#if DEBUG
                        CrestronConsole.PrintLine("Request has no handler!");
#endif
                        HandleError(args.Context, 404, "Not Found", "The requested resource does not exist");
                    }
                    else if (args.Context.Request.PhysicalPath != string.Format("\\HTML\\{0}", _path) &&
                             args.Context.Request.PhysicalPath != string.Format("\\HTML\\{0}\\", _path))
                    {
#if DEBUG
                        CrestronConsole.PrintLine("Request handler: {0}", RootHandler.GetType());
#endif
                        RootHandler.ProcessRequest(args.Context);
                    }
                }
                catch (Exception e)
                {
                    CloudLog.Exception("Error in ApiServer main request handler", e);
                    HandleError(args.Context, 500, "Server Error", string.Format("{0}<BR>{1}", e.Message, e.StackTrace));
                }
            };
            CrestronEnvironment.ProgramStatusEventHandler += type =>
            {
                if(type == eProgramStatusEventType.Stopping)
                    _server.Dispose();
            };
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events
        #endregion

        #region Delegates
        #endregion

        #region Properties

        public BaseRequestHandler RootHandler { get; set; }

        public string BaseUrlDns
        {
            get
            {
                var adapterId =
                    CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(EthernetAdapterType.EthernetLANAdapter);
                var hostName =
                    CrestronEthernetHelper.GetEthernetParameter(
                        CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_HOSTNAME, adapterId);
                var domainName =
                    CrestronEthernetHelper.GetEthernetParameter(
                        CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_DOMAIN_NAME, adapterId);
                var port =
                    CrestronEthernetHelper.GetEthernetParameter(
                        CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_WEB_PORT, adapterId);
                return string.Format("http://{0}{1}{2}/cws{3}/", hostName,
                    domainName.Length > 0 ? "." + domainName : "",
                    port == "80" ? "" : ":" + port,
                    _path.StartsWith("/") ? _path : "/" + _path);
            }
        }

        public string BaseUrlIp
        {
            get
            {
                var adapterId =
                    CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(EthernetAdapterType.EthernetLANAdapter);
                var ipAddress =
                    CrestronEthernetHelper.GetEthernetParameter(
                        CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, adapterId);
                var port =
                    CrestronEthernetHelper.GetEthernetParameter(
                        CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_WEB_PORT, adapterId);
                return string.Format("http://{0}{1}/cws{2}/", ipAddress, port == "80" ? "" : ":" + port,
                    _path.StartsWith("/") ? _path : "/" + _path);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Add a handler to a route
        /// </summary>
        /// <param name="handler">The request handler to deal with incoming routes</param>
        /// <param name="url">The URL pattern for the route.</param>
        /// <remarks>
        /// URL pattern cannot start with a '/' or '~' character and it cannot contain a '?' character.
        ///             It should not also start with the virtual folder the server is registered for,
        ///             i.e. {device}/Presets/{id}/Recall is correct while /API/{device}/Presets/{id}/Recall is not in both ways.
        /// 
        /// </remarks>
        public void RegisterHandler(BaseRequestHandler handler, string url)
        {
            var route = new HttpCwsRoute(url)
            {
                RouteHandler = handler
            };

#if DEBUG
            CrestronConsole.PrintLine("Added route for {0}, url: {1}", handler.Name, route.Url);
#endif

            _server.AddRoute(route);
        }

        protected virtual void HandleError(HttpCwsContext context, int statusCode, string status, string customMessage)
        {
            context.Response.Clear();
            context.Response.ClearHeaders();
            context.Response.StatusCode = statusCode;
            context.Response.StatusDescription = status;

            var content = string.Format("<H1>Error {0} <span>{1}</span></H1><P>{2}</P>",
                statusCode, status, customMessage);

            context.Response.Write(content, true);
        }

        #endregion
    }
}