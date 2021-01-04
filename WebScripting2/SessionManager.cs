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
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp.CrestronAuthentication;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.Cryptography;

namespace UX.Lib2.WebScripting2
{
    public static class SessionManager
    {
        private static readonly Dictionary<string, Session> Sessions = new Dictionary<string, Session>();

        public static Session StartSession(string username, string password)
        {
            var user = Authentication.ValidateUserInformation(username, password);
            if (!user.Authenticated) return null;
            var session = new Session(user);
            Sessions[session.SessionId] = session;
            return session;
        }

        public static Session GetSession(string sessionId)
        {
            return Sessions.ContainsKey(sessionId) ? Sessions[sessionId] : null;
        }

        public static void RemoveSession(string sessionId)
        {
            if (Sessions.ContainsKey(sessionId)) Sessions.Remove(sessionId);
        }
    }
}