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
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronAuthentication;
using Crestron.SimplSharp.Net;
using Crestron.SimplSharp.Net.Http;

namespace UX.Lib2.WebScripting2
{
    public class Request
    {
        private readonly WebScriptingServer _server;
        private readonly OnHttpRequestArgs _context;
        private readonly Session _session;
        private DateTime _time;

        internal Request(WebScriptingServer server, OnHttpRequestArgs context, Dictionary<string, string> pathArguments)
        {
            _time = DateTime.Now;
            _server = server;
            _context = context;
            //UserHostName = Dns.GetHostEntry(UserIpAddress).HostName;
            PathArguments = new ReadOnlyDictionary<string, string>(pathArguments);
            foreach (var match in from HttpHeader header in Header
                where header.Name == "Cookie"
                select Regex.Match(header.Value, @"sessionid=(\w+)")
                into match
                where match.Success
                select match)
            {
                _session = SessionManager.GetSession(match.Groups[1].Value);
                if (_session != null)
                {
                    var header = _session.Validate();
                    if (header != null)
                    {
                        User = _session.User;
                        Response.Header.AddHeader(header);
                    }
                }
                break;
            }
        }

        public string UserIpAddress
        {
            get { return _context.Request.DataConnection.RemoteEndPointAddress; }
        }

        public string UserHostName { get; private set; }

        public string Path
        {
            get { return HttpUtility.UrlDecode(_context.Request.Path); }
        }

        public string PathAndQueryString
        {
            get { return Path + "?" + QueryString; }
        }

        public DateTime Time
        {
            get { return _time; }
        }

        public ReadOnlyDictionary<string, string> PathArguments { get; private set; }

        public QueryString QueryString
        {
            get { return _context.Request.QueryString; }
        }

        public HttpServerResponse Response
        {
            get { return _context.Response; }
        }

        public HttpHeaders Header
        {
            get { return _context.Request.Header; }
        }

        public Authentication.UserInformation User { get; private set; }

        public string Host
        {
            get
            {
                try
                {
                    return _context.Request.Header["Host"].Value;
                }
                catch
                {
                    Debug.WriteWarn("Header does not contain item \"Host\"");
                }

                return
                    CrestronEthernetHelper.GetEthernetParameter(
                        CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS,
                        CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(
                            EthernetAdapterType.EthernetLANAdapter));
            }
        }

        public RequestType RequestType
        {
            get { return (RequestType) Enum.Parse(typeof (RequestType), _context.Request.Header.RequestType, true); }
        }

        public bool HasContent
        {
            get { return _context.Request.HasContentLength; }
        }

        public int ContentLength
        {
            get { return _context.Request.ContentLength; }
        }

        public string ContentString
        {
            get
            {
                return Encoding.UTF8.GetString(_context.Request.ContentBytes, 0,
                    _context.Request.ContentLength);
            }
        }

        internal WebScriptingServer Server
        {
            get { return _server; }
        }

        internal OnHttpRequestArgs Context
        {
            get { return _context; }
        }

        public void EndSession()
        {
            if (_session != null)
            {
                SessionManager.RemoveSession(_session.SessionId);
            }
        }
    }
}