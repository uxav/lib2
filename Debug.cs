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

using Crestron.SimplSharp;

namespace UX.Lib2
{
    
    public static class Debug
    {
        public const string AnsiReset = "\u001B[0m";
        public const string AnsiBlack = "\u001B[30m";
        public const string AnsiRed = "\u001B[31m";
        public const string AnsiGreen = "\u001B[32m";
        public const string AnsiYellow = "\u001B[33m";
        public const string AnsiBlue = "\u001B[34m";
        public const string AnsiPurple = "\u001B[35m";
        public const string AnsiCyan = "\u001B[36m";
        public const string AnsiWhite = "\u001B[37m";

        public static void WriteNormal(string title, string context, params object[] args)
        {
            var body = string.Format(context, args);
            WriteNormal(title, body);
        }

        public static void WriteNormal(string title, string context)
        {
            CrestronConsole.PrintLine("{0}: {1}", title, context);
        }

        public static void WriteNormal(string context, params object[] args)
        {
            CrestronConsole.PrintLine(context, args);
        }

        public static void WriteNormal(string context)
        {
            CrestronConsole.PrintLine(context);
        }

        public static void WriteWarn(string title, string context, params object[] args)
        {
            WriteNormal(AnsiYellow + title + AnsiReset, context, args);
        }

        public static void WriteWarn(string title, string context)
        {
            WriteNormal(AnsiYellow + title + AnsiReset, context);
        }

        public static void WriteWarn(string context, params object[] args)
        {
            WriteNormal(AnsiYellow + context + AnsiReset, args);
        }

        public static void WriteWarn(string context)
        {
            WriteNormal(AnsiYellow + context + AnsiReset);
        }

        public static void WriteError(string title, string context, params object[] args)
        {
            WriteNormal(AnsiRed + title + AnsiReset, context, args);
        }

        public static void WriteError(string title, string context)
        {
            WriteNormal(AnsiRed + title + AnsiReset, context);
        }

        public static void WriteError(string context, params object[] args)
        {
            WriteNormal(AnsiRed + context + AnsiReset, args);
        }

        public static void WriteError(string context)
        {
            WriteNormal(AnsiRed + context + AnsiReset);
        }

        public static void WriteSuccess(string title, string context, params object[] args)
        {
            WriteNormal(AnsiGreen + title + AnsiReset, context, args);
        }

        public static void WriteSuccess(string title, string context)
        {
            WriteNormal(AnsiGreen + title + AnsiReset, context);
        }

        public static void WriteSuccess(string context, params object[] args)
        {
            WriteNormal(AnsiGreen + context + AnsiReset, args);
        }

        public static void WriteSuccess(string context)
        {
            WriteNormal(AnsiGreen + context + AnsiReset);
        }

        public static void WriteInfo(string title, string context, params object[] args)
        {
            WriteNormal(AnsiCyan + title + AnsiReset, context, args);
        }

        public static void WriteInfo(string title, string context)
        {
            WriteNormal(AnsiCyan + title + AnsiReset, context);
        }

        public static void WriteInfo(string context, params object[] args)
        {
            WriteNormal(AnsiCyan + context + AnsiReset, args);
        }

        public static void WriteInfo(string context)
        {
            WriteNormal(AnsiCyan + context + AnsiReset);
        }
    }
}