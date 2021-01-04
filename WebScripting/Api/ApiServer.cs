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

using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UX.Lib2.WebScripting.Api
{
    public class ApiServer : WebScriptingServer
    {
        /// <summary>
        /// The default Constructor.
        /// </summary>
        public ApiServer()
            : base("/api")
        {

        }

        protected sealed override void HandleError(HttpCwsContext context, int statusCode, string status, string customMessage)
        {
            context.Response.Clear();
            context.Response.ClearHeaders();
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;
            context.Response.StatusDescription = status;

            var error = new
            {
                statusCode,
                status,
                message = customMessage
            };

            var data = JObject.FromObject(error);

            context.Response.Write(data.ToString(Formatting.Indented), true);
        }
    }
}