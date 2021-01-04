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
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.Net.Http;
using Ionic.Zip;
using UX.Lib2.Models;
using UX.Lib2.WebScripting2;

namespace UX.Lib2.WebApp
{
    public class ProcessorServiceReportHandler : BaseRequestHandler
    {
        #region Fields
        #endregion

        #region Constructors

        public ProcessorServiceReportHandler(SystemBase system, Request request, bool loginRequired)
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

        public void Get()
        {
            if (RequireLogin(true)) return;

            try
            {
                var ms = new MemoryStream();
                var appNumber = InitialParametersClass.ApplicationNumber;
                var commands = new[]
                {
                    "hostname",
                    "mycrestron",
                    "showlicense",
                    "osd",
                    "uptime",
                    "ver -v",
                    "ver all",
                    "uptime",
                    "time",
                    "timezone",
                    "sntp",
                    "showhw",
                    "ipconfig /all",
                    "progregister",
                    "progcomments:1",
                    "progcomments:2",
                    "progcomments:3",
                    "progcomments:4",
                    "progcomments:5",
                    "progcomments:6",
                    "progcomments:7",
                    "progcomments:8",
                    "progcomments:9",
                    "progcomments:10",
                    "proguptime:1",
                    "proguptime:2",
                    "proguptime:3",
                    "proguptime:4",
                    "proguptime:5",
                    "proguptime:6",
                    "proguptime:7",
                    "proguptime:8",
                    "proguptime:9",
                    "proguptime:10",
                    "ssptasks:1",
                    "ssptasks:2",
                    "ssptasks:3",
                    "ssptasks:4",
                    "ssptasks:5",
                    "ssptasks:6",
                    "ssptasks:7",
                    "ssptasks:8",
                    "ssptasks:9",
                    "ssptasks:10",
                    "appstat -p:" + appNumber,
                    "taskstat",
                    "ramfree",
                    "cpuload",
                    "cpuload",
                    "cpuload",
                    "showportmap -all",
                    "ramfree",
                    "showdiskinfo",
                    "ethwdog",
                    "iptable -p:all -t",
                    "who",
                    "netstat",
                    "threadpoolinfo",
                    "autodiscover query tableformat",
                    "reportcresnet",
                };
                using (var zip = new ZipFile())
                {
                    var directory = new DirectoryInfo(@"\NVRAM");
                    foreach (var file in directory.GetFiles())
                    {
                        zip.AddFile(file.FullName, @"\NVRAM Root");
                    }

                    directory = new DirectoryInfo(SystemBase.AppStoragePath);
                    foreach (var file in directory.GetFiles())
                    {
                        zip.AddFile(file.FullName,
                            string.Format("\\NVRAM for App {0}", InitialParametersClass.ApplicationNumber.ToString("D2")));
                    }

                    var diagnosticsData = new StringWriter();
                    foreach (var message in System.StatusMessages)
                    {
                        diagnosticsData.WriteLine("{0}: {2} {1}", message.MessageLevel, message.MessageString,
                            message.SourceDeviceName);
                    }
                    zip.AddEntry("diagnostics.txt", diagnosticsData.ToString());

                    directory = new DirectoryInfo(@"/Sys/PLog/CurrentBoot");
                    foreach (var file in directory.GetFiles())
                    {
                        zip.AddFile(file.FullName, @"\CurrentBootLogs");
                    }

                    directory = new DirectoryInfo(@"/Sys/PLog/PreviousBoot");
                    foreach (var file in directory.GetFiles())
                    {
                        zip.AddFile(file.FullName, @"\PreviousBootLogs");
                    }

                    var statusData = new MemoryStream();
                    var sw = new StreamWriter(statusData);
                    foreach (var command in commands)
                    {
                        sw.WriteLine("Console Cmd: {0}", command);
                        var response = string.Empty;
                        CrestronConsole.SendControlSystemCommand(command, ref response);
                        sw.WriteLine(response);
                    }
                    statusData.Position = 0;
                    zip.AddEntry(@"processor_status.txt", statusData);

                    zip.Save(ms);
                }
                Request.Response.Header.ContentType = MimeMapping.GetMimeMapping(".zip");
                Request.Response.ContentSource = ContentSource.ContentStream;
                var hostname =
                    CrestronEthernetHelper.GetEthernetParameter(
                        CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_HOSTNAME,
                        CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(
                            EthernetAdapterType.EthernetLANAdapter));
                Request.Response.Header.AddHeader(new HttpHeader("content-disposition",
                    string.Format("attachment; filename=processor_report_{0}_{1}.zip",
                        hostname, DateTime.Now.ToString("yyyyMMddTHHmmss"))));
                ms.Position = 0;
                Request.Response.ContentStream = ms;
                Request.Response.CloseStream = true;
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }

        #endregion
    }
}