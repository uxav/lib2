using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UX.Lib2.WebScripting.Api
{
    public abstract class BaseApiRequestHandler : BaseRequestHandler
    {
        #region Fields
        #endregion

        #region Constructors

        /// <summary>
        /// The default Constructor.
        /// </summary>
        public BaseApiRequestHandler(string name)
            : base(name)
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

        protected sealed override void HandleError(int statusCode, string status, string customMessage)
        {
            Response.Clear();
            Response.ClearHeaders();
            Response.ContentType = "application/json";
            Response.StatusCode = statusCode;
            Response.StatusDescription = status;

            var error = new
            {
                statusCode,
                status,
                message = customMessage
            };

            var data = JObject.FromObject(error);

            Response.Write(data.ToString(Formatting.Indented), true);
        }

        protected void WriteOk(string customMessage)
        {
            Response.Clear();
            Response.ClearHeaders();
            Response.ContentType = "application/json";
            Response.StatusCode = 200;
            Response.StatusDescription = "OK";

            var response = new
            {
                statusCode = 200,
                status = "OK",
                message = customMessage
            };

            var data = JObject.FromObject(response);

            Response.Write(data.ToString(Formatting.Indented), true);
        }

        /// <summary>
        /// Provides processing of HTTP requests by a HttpHandler that implements
        ///                 the <see cref="T:Crestron.SimplSharp.WebScripting.IHttpCwsHandler"/> interface.
        /// </summary>
        /// <param name="context">The object encapsulating the HTTP request.</param>
        /// <returns>
        /// true is the request was processed successfully; otherwise, false.
        /// </returns>
        public override void ProcessRequest(HttpCwsContext context)
        {
            context.Response.ContentType = "application/json";
            base.ProcessRequest(context);
        }

        #endregion
    }
}