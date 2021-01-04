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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UX.Lib2.Models;
using UX.Lib2.WebScripting2;

namespace UX.Lib2.WebApp
{
    public abstract class ApiHandler : BaseRequestHandler
    {
        #region Fields
        #endregion

        #region Constructors

        protected ApiHandler(SystemBase system, Request request, bool loginRequired)
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

        public virtual void Get()
        {
            HandleError(405, "Method not allowed", "Get is not inherited in api handler");
        }

        protected void WriteResponse(object responseObject)
        {
            WriteResponse(responseObject, new JsonSerializerSettings());
        }

        protected void WriteResponse(object responseObject, JsonSerializerSettings settings)
        {
            Request.Response.Header.ContentType = "application/json";
            var data = new
            {
                @code = Request.Response.Code,
                @error = false,
                @path = Request.Path,
                @requestId = Request.QueryString.ContainsKey("id") ? int.Parse(Request.QueryString["id"]) : 0,
                @response = responseObject
            };

            var json = JsonConvert.SerializeObject(data, Formatting.None, settings);
            WriteResponse(json, true);
        }

        protected override void HandleError(Exception e)
        {
            HandleError(500, "Server Error: " + e.Message, e.StackTrace);
        }

        protected override void HandleError(int code, string title, string message)
        {
            Request.Response.Code = code;
            Request.Response.ResponseText = title;
            Request.Response.Header.ContentType = "application/json";
            var data = new
            {
                @code = code,
                @error = true,
                @error_title = title,
                @error_message = message
            };
            WriteResponse(JToken.FromObject(data).ToString(Formatting.Indented), true);
        }

        protected override void HandleNotFound()
        {
            HandleError(404, "Not Found", "This resource is not known");
        }

        #endregion
    }
}