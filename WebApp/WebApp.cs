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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Crestron.SimplSharp.Reflection;
using UX.Lib2.Models;
using UX.Lib2.WebScripting2;

namespace UX.Lib2.WebApp
{
    public class WebApp
    {
        #region Fields

        private readonly WebScriptingServer _server;
        private readonly List<UserPageLink> _userPageLinks = new List<UserPageLink>();
        private readonly List<XPanelLink> _xpanelLinks = new List<XPanelLink>(); 

        #endregion

        #region Constructors

        public WebApp(SystemBase system, int httpPort)
        {
            _server = new WebScriptingServer(system, httpPort, this);
            _server.AddRoute(@"/api/internal/<method:runcmd>", typeof(BuiltInApiHandler), false);
            _server.AddRoute(@"/api/internal/<method:\w+>", typeof(BuiltInApiHandler));
            _server.AddRoute(@"/boot", typeof(BootPageHandler));
            _server.AddRoute(@"/login", typeof(LoginHandler));
            _server.AddRoute(@"/logout", typeof(LogoutHandler));
            _server.AddRedirect("/", "/dashboard/home");
            _server.AddRedirect("/dashboard", "/dashboard/home");
            _server.AddRoute(@"/dashboard/<page:home>", typeof(DashboardHandler), false);
            _server.AddRoute(@"/dashboard/<page:diagnostics>", typeof(DashboardHandler), false);
            _server.AddRoute(@"/dashboard/<page:service>", typeof(DashboardHandler), false);
            _server.AddRoute(@"/dashboard/<page:\w+>", typeof(DashboardHandler));
            _server.AddRoute(@"/dashboard/content/<page:home>", typeof(DashboardContentHandler), false);
            _server.AddRoute(@"/dashboard/content/<page:diagnostics>", typeof(DashboardContentHandler), false);
            _server.AddRoute(@"/dashboard/content/<page:\w+>", typeof(DashboardContentHandler));
            _server.AddRoute(@"/servicepackage/download", typeof(ProcessorServiceReportHandler), false);
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events
        #endregion

        #region Delegates
        #endregion

        #region Properties

        public IEnumerable<UserPageLink> UserPageLinks
        {
            get { return new ReadOnlyCollection<UserPageLink>(_userPageLinks); }
        }

        public IEnumerable<XPanelLink> XPanelLinks
        {
            get { return new ReadOnlyCollection<XPanelLink>(_xpanelLinks); }
        } 

        public string DashboardCustomScriptPath { get; set; }

        #endregion

        #region Methods

        public void AddRoute(string pattern, CType handlerType)
        {
            _server.AddRoute(pattern, handlerType);
        }

        public void AddRedirect(string pattern, string redirect)
        {
            _server.AddRedirect(pattern, redirect);
        }

        public void AddUserPageLink(string path, string linkName, string iconClass)
        {
            _userPageLinks.Add(new UserPageLink
            {
                Url = path,
                Name = linkName,
                IconClass = iconClass
            });
        }

        public void AddXpanelLink(string path, string name)
        {
            _xpanelLinks.Add(new XPanelLink
            {
                Url = path,
                Name = name
            });
        }

        #endregion
    }

    public struct UserPageLink
    {
        public string Url;
        public string IconClass;
        public string Name;
    }

    public struct XPanelLink
    {
        public string Url;
        public string Name;
    }
}