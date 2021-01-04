using System;
using Crestron.SimplSharp.WebScripting;
using UX.Lib2.Cloud.Logger;

namespace UX.Lib2.WebScripting
{
    public abstract class BaseRequestHandler : IHttpCwsHandler
    {
        #region Fields

        private HttpCwsContext _context;

        #endregion

        #region Constructors

        /// <summary>
        /// The default Constructor.
        /// </summary>
        protected BaseRequestHandler(string name)
        {
            Name = name;
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events
        #endregion

        #region Delegates
        #endregion

        #region Properties

        public string Name { get; set; }

        public HttpCwsRequest Request
        {
            get { return _context.Request; }
        }

        public HttpCwsResponse Response
        {
            get { return _context.Response; }
        }

        #endregion

        #region Methods

        protected void Error(Exception e)
        {
            CloudLog.Exception(e);
            HandleError(500, "Server Error", e.Message);
        }

        protected void Error(int statusCode, string status)
        {
            HandleError(statusCode, status, string.Empty);
        }

        protected void Error(int statusCode, string status, string customMessage)
        {
            HandleError(statusCode, status, customMessage);
        }

        protected void HandleError(Exception e)
        {
            CloudLog.Exception(e);
            HandleError(500, e.Message, e.StackTrace);
        }

        protected virtual void HandleError(int statusCode, string status, string customMessage)
        {
            Response.Clear();
            Response.ClearHeaders();
            Response.StatusCode = statusCode;
            Response.StatusDescription = status;

            var content = string.Format("<H1>Error {0} <span>{1}</span></H1><pre>{2}</pre>",
                statusCode, status, customMessage);

            Response.Write(content, true);
        }

        /// <summary>
        /// Provides processing of HTTP requests by a HttpHandler that implements
        ///                 the <see cref="T:Crestron.SimplSharp.WebScripting.IHttpCwsHandler"/> interface.
        /// </summary>
        /// <param name="context">The object encapsulating the HTTP request.</param>
        /// <returns>
        /// true is the request was processed successfully; otherwise, false.
        /// </returns>
        public virtual void ProcessRequest(HttpCwsContext context)
        {
            try
            {
                _context = context;
                switch (_context.Request.HttpMethod)
                {
                    case "GET":
                        try
                        {
                            Get();
                        }
                        catch (Exception e)
                        {
                            if(e is NotImplementedException)
                                Error(405, "Method not allowed");
                            else
                                Error(e);
                        }
                        break;
                    case "POST":
                        try
                        {
                            Post();
                        }
                        catch (Exception e)
                        {
                            if(e is NotImplementedException)
                                Error(405, "Method not allowed");
                            else
                                Error(e);
                        }
                        break;
                    default:
                        Error(405, "Method not allowed");
                        break;
                }
                if(Response.IsClientConnected)
                    Response.Write("", true);
            }
            catch (Exception e)
            {
                Error(e);
            }
        }

        protected abstract void Get();

        protected abstract void Post();

        #endregion
    }
}