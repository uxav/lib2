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
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronAuthentication;
using Crestron.SimplSharp.Cryptography;
using Crestron.SimplSharp.Net.Http;

namespace UX.Lib2.WebScripting2
{
    public class Session
    {
        private readonly Authentication.UserInformation _user;

        #region Fields

        private readonly string _sessionId;
        private DateTime _expiryTime;

        #endregion

        #region Constructors

        /// <summary>
        /// The default Constructor.
        /// </summary>
        internal Session(Authentication.UserInformation user)
        {
            _expiryTime = DateTime.Now.Add(TimeSpan.FromMinutes(120));
            _user = user;
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.ASCII.GetBytes(DateTime.Now + _user.UserName));
            _sessionId = string.Empty;
            foreach (var b in hash)
            {
                _sessionId = _sessionId + b.ToString("x2");
            }
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events
        #endregion

        #region Delegates
        #endregion

        #region Properties

        public string SessionId
        {
            get { return _sessionId; }
        }

        public Authentication.UserInformation User
        {
            get { return _user; }
        }

        public DateTime ExpiryTime
        {
            get { return _expiryTime; }
        }

        #endregion

        #region Methods

        public HttpHeader Validate()
        {
            if (DateTime.Now >= _expiryTime || !_user.Authenticated) return null;
            _expiryTime = DateTime.Now.Add(TimeSpan.FromMinutes(120));
            return new HttpHeader(string.Format("Set-Cookie: sessionid={0}; HttpOnly; Path=/; Expires={1}",
                SessionId, _expiryTime.ToUniversalTime().ToString("R")));
        }

        #endregion
    }
}