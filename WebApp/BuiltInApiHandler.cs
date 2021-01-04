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
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.CrestronThread;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UX.Lib2.Cloud.Logger;
using UX.Lib2.Logging;
using UX.Lib2.Models;
using UX.Lib2.WebApp.Templates;
using UX.Lib2.WebScripting2;

namespace UX.Lib2.WebApp
{
    public class BuiltInApiHandler : ApiHandler
    {
        #region Fields
        #endregion

        #region Constructors

        public BuiltInApiHandler(SystemBase system, Request request, bool loginRequired)
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

        public override void Get()
        {
            try
            {
                switch (Request.PathArguments["method"])
                {
                    case "bootprogress":
                        var response = new
                        {
                            @booted = System.Booted,
                            @restartpending = System.WillRestart,
                            @progress = System.BootProgressPercent,
                            @status = System.BootStatus
                        };
                        WriteResponse(response);
                        break;
                    case "version":
                        var roomName = "CONFIG ERROR";
                        if (System.Rooms.Any())
                        {
                            roomName = System.Rooms.First().Name;
                        }
                        var versionResponse = new
                        {
                            @version = System.Version.ToString(),
                            @name =  System.VersionName,
                            @type = System.SystemTypeInfo,
                            @room = roomName
                        };
                        WriteResponse(versionResponse);
                        break;
                    case "memory":
                        WriteResponse(new
                        {
                            @percent = SystemMonitor.RamUsedPercent,
                            @total = SystemMonitor.TotalRamSize,
                            @used = SystemMonitor.RamUsed,
                            @free = SystemMonitor.RamFree,
                            @free_min = SystemMonitor.RamFreeMinimum
                        });
                        break;
                    case "cpu":
                        WriteResponse(new
                        {
                            @cpu_load = SystemMonitor.CpuUtilization,
                            @cpu_max = SystemMonitor.CpuMaxUtilization,
                            @cpu_max_margin = SystemMonitor.CpuMaxUtilization - SystemMonitor.CpuUtilization
                        });
                        break;
                    case "logs":
                        var logs = CloudLog.GetSystemLogs().ToArray();
                        var logResponse = new
                        {
                            count = logs.Length,
                            errorCount = logs.Count(l => l.Level >= LoggingLevel.Error),
                            warningCount = logs.Count(l => l.Level == LoggingLevel.Warning),
                            logs
                        };
                        WriteResponse(logResponse);
                        break;
                    case "avstatus":
                        var rooms = new JArray();
                        foreach (var room in System.Rooms)
                        {
                            var sources = new JArray();
                            foreach (var source in room.Sources)
                            {
                                var s = new JObject();
                                s["id"] = source.Id;
                                s["name"] = source.Name;
                                s["type"] = source.Type.ToString();
                                s["icon_name"] = source.IconName;
                                s["icon_svg"] = source.GenerateIconData();
                                s["availability"] = source.AvailabilityType.ToString();
                                s["active"] = room.Source == source;

                                sources.Add(s);
                            }

                            var r = new JObject();
                            r["id"] = room.Id;
                            r["screen_name"] = room.Name;
                            r["name"] = room.NameFull;
                            r["location"] = room.Location;
                            r["contact_number"] = room.RoomContactNumber;
                            r["sources"] = sources;

                            rooms.Add(r);
                        }
                        
                        WriteResponse(new
                        {
                            rooms
                        });
                        break;
                    default:
                        HandleNotFound();
                        break;
                }
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }

        public void Post()
        {
            try
            {
                if (RequireLogin(false)) return;

                var data = JToken.Parse(Request.ContentString);

                switch (Request.PathArguments["method"])
                {
                    case "runcmd":
                        var cmd = data["command"].Value<string>();
                        var reply = string.Empty;
                        CrestronConsole.SendControlSystemCommand(cmd, ref reply);

                        WriteResponse(new
                        {
                            @command = cmd,
                            @reply = reply.Trim()
                        });
                        break;
                    case "config":
                        switch (data["method"].Value<string>())
                        {
                            case "save":
                                var configData = data["configdata"];

                                try
                                {
                                    System.SetConfig(configData);
                                }
                                catch (Exception e)
                                {
                                    var error = new
                                    {
                                        @error = true,
                                        @title = "Error",
                                        @message = e.Message + "\r\n" + e.StackTrace
                                    };
                                    WriteResponse(error);
                                    return;
                                }

                                WriteResponse(new
                                {
                                    @error = false,
                                    @title = "Success",
                                    @message = "Config has been saved!"
                                });
                                return;
                            case "loadtemplate":
                                try
                                {
                                    System.LoadConfigTemplate(data["systemid"].Value<string>());
                                    WriteResponse(new
                                    {
                                        @error = false,
                                        @title = "Success",
                                        @message = "Config has been created, check config and restart!"
                                    });
                                }
                                catch (Exception e)
                                {
                                    WriteResponse(new
                                    {
                                        @error = true,
                                        @title = "Error",
                                        @message = e.Message + "\r\n" + e.StackTrace
                                    });
                                }
                                break;
                            case "reset":
                                System.FactoryReset();
                                WriteResponse(new
                                {
                                    @error = false,
                                    @title = "Success",
                                    @message = "NVRAM has been cleared and system will now reboot!"
                                });
                                break;
                            default:
                                HandleError(400, "Bad Request", "Unknown API method for config path");
                                return;
                        }
                        break;
                    case "systemmonitor":
                        switch (data["method"].Value<string>())
                        {
                            case "reset_maximums":
                                Crestron.SimplSharpPro.Diagnostics.SystemMonitor.ResetMaximums();
                                break;
                            case "start":
                                SystemMonitor.Start();
                                break;
                            case "stop":
                                SystemMonitor.Stop();
                                break;
                            default:
                                HandleError(400, "Bad Request", "Unknown API method for systemmonitor path");
                                return;
                        }
                        break;
                    case "av":
                        RoomBase room;
                        switch (data["method"].Value<string>())
                        {
                            case "select_source":
                                room = System.Rooms[data["room"].Value<uint>()];
                                var sourceId = data["source"].Value<uint>();
                                var source = sourceId == 0 ? null : System.Sources[sourceId];
                                room.Source = source;
                                WriteResponse(new
                                {
                                    @error = false,
                                    @title = "Success",
                                    @message =
                                        string.Format("Source \"{0}\" has now been selected in Room \"{1}\"",
                                            source == null ? "NONE" : source.Name, room.Name)
                                });
                                break;
                            case "power_off":
                                room = System.Rooms[data["room"].Value<uint>()];
                                room.PowerOff(false, PowerOfFEventType.UserRequest);
                                WriteResponse(new
                                {
                                    @error = false,
                                    @title = "Success",
                                    @message =
                                        string.Format("Room \"{0}\" has been requested to power off",
                                            room.Name)
                                });
                                break;
                            default:
                                HandleError(400, "Bad Request", "Unknown API method for av path");
                                return;
                        }
                        break;
                    default:
                        HandleNotFound();
                        break;
                }
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }

        #endregion
    }
}