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
using Crestron.SimplSharpPro.Diagnostics;
using UX.Lib2.Cloud.Logger;

namespace UX.Lib2.Models
{
    public static class SystemMonitor
    {
        private static ushort _cpuMaxUtilization;
        private static ushort _cpuUtilization;
        private static bool _started;

        public static void Start()
        {
            if(_started) return;

            Crestron.SimplSharpPro.Diagnostics.SystemMonitor.SetUpdateInterval(10);
            try
            {
                Crestron.SimplSharpPro.Diagnostics.SystemMonitor.ProcessStatisticChange +=
                    SystemMonitorOnProcessStatisticChange;
                Crestron.SimplSharpPro.Diagnostics.SystemMonitor.CPUStatisticChange += SystemMonitorOnCpuStatisticChange;
            }
            catch (Exception e)
            {
                CloudLog.Error("Error starting system monitor, {0}", e.Message);
            }

            _started = true;
        }

        public static void Stop()
        {
            if (!_started) return;

            try
            {
                Crestron.SimplSharpPro.Diagnostics.SystemMonitor.ProcessStatisticChange -=
                    SystemMonitorOnProcessStatisticChange;
                Crestron.SimplSharpPro.Diagnostics.SystemMonitor.CPUStatisticChange -= SystemMonitorOnCpuStatisticChange;
            }
            catch (Exception e)
            {
                CloudLog.Error("Error stopping system monitor, {0}", e.Message);
            }

            _started = false;
        }

        private static void SystemMonitorOnProcessStatisticChange(ProcessStatisticChangeEventArgs args)
        {
            switch (args.StatisticWhichChanged)
            {
                case eProcessStatisticChange.MaximumNumberOfRunningProcesses:
                    MaximumNumberOfRunningProcesses = args.MaximumNumberOfRunningProcesses;
                    break;
                case eProcessStatisticChange.NumberOfRunningProcesses:
                    NumberOfRunningProcesses = args.NumberOfRunningProcesses;
                    break;
                case eProcessStatisticChange.RAMFree:
                    RamFree = args.RAMFree;
                    break;
                case eProcessStatisticChange.RAMFreeMinimum:
                    RamFreeMinimum = args.RAMFreeMinimum;
                    break;
                case eProcessStatisticChange.TotalRAMSize:
                    TotalRamSize = args.TotalRAMSize;
                    break;
            }
        }

        private static void SystemMonitorOnCpuStatisticChange(CPUStatisticChangeEventArgs args)
        {
            switch (args.StatisticWhichChanged)
            {
                case eCPUStatisticChange.Utilization:
                    _cpuUtilization = args.Utilization;
                    break;
                case eCPUStatisticChange.MaximumUtilization:
                    _cpuMaxUtilization = args.MaximumUtilization;
                    break;
            }
        }

        public static uint CpuUtilization
        {
            get { return _cpuUtilization; }
        }

        public static uint CpuMaxUtilization
        {
            get { return _cpuMaxUtilization; }
        }

        public static bool Running
        {
            get { return _started; }
        }

        public static ushort MaximumNumberOfRunningProcesses { get; private set; }
        public static ushort NumberOfRunningProcesses { get; private set; }

        public static uint RamUsed
        {
            get
            {
                try
                {
                    return TotalRamSize - RamFree;
                }
                catch
                {
                    return 0;
                }
            }
        }

        public static uint RamFree { get; private set; }
        public static uint RamFreeMinimum { get; private set; }
        public static uint TotalRamSize { get; private set; }

        public static uint RamUsedPercent
        {
            get
            {
                try
                {
                    return (uint) Tools.ScaleRange(RamUsed, uint.MinValue, TotalRamSize, 0, 100);
                }
                catch
                {
                    return 0;
                }
            }
        }
    }
}