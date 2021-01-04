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
using System.Text.RegularExpressions;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.Net.Http;
using Crestron.SimplSharp.Reflection;
using UX.Lib2.Models;

namespace UX.Lib2.WebScripting2
{
    public abstract class FileHandlerBase : BaseRequestHandler
    {
        protected FileHandlerBase(SystemBase system, Request request, bool loginRequired)
            : base(system, request, loginRequired)
        {
        }

        protected abstract string RootFilePath { get; }

        public void Get()
        {
            try
            {
                Request.Response.ContentSource = ContentSource.ContentStream;
#if DEBUG
                Debug.WriteInfo(GetType().Name, "Looking for file resource: {0}", Request.PathArguments["filepath"]);
#endif
                var stream = GetResourceStream(Assembly.GetExecutingAssembly(), Request.PathArguments["filepath"]);
                if (stream == null)
                {
                    HandleError(404, "Not Found", "The requested file could not be found");
                    return;
                }
                Request.Response.ContentStream = stream;
                Request.Response.CloseStream = true;
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }

        public void Head()
        {
            try
            {
                Request.Response.ContentSource = ContentSource.ContentStream;
#if DEBUG
                Debug.WriteInfo(GetType().Name, "Looking for file resource: {0}", Request.PathArguments["filepath"]);
#endif
                var stream = GetResourceStream(Assembly.GetExecutingAssembly(), Request.PathArguments["filepath"]);
                if (stream == null)
                {
                    HandleError(404, "Not Found", "The requested file could not be found");
                    return;
                }
                Request.Response.ContentStream = stream;
                Request.Response.CloseStream = true;
                Request.Response.FinalizeHeader();
                //Request.Response.Header.AddHeader(new HttpHeader("Content-Length", stream.Length.ToString()));
            }
            catch (Exception e)
            {
                HandleError(e);
            }
        }

        protected void SetCacheTime(TimeSpan time)
        {
            Request.Response.Header.AddHeader(new HttpHeader("Cache-Control",
                string.Format("public, max-age={0}", time.TotalSeconds)));
        }

        protected virtual Stream GetResourceStream(Assembly assembly, string fileName)
        {
            if (RootFilePath.Contains(@"\"))
            {
                var filePath = RootFilePath + @"\" + fileName.Replace('/', (char)92);
#if DEBUG
                Debug.WriteInfo("Looking for file", filePath);
#endif
                var fileInfo = new FileInfo(filePath);
                Request.Response.Header.ContentType = MimeMapping.GetMimeMapping(fileInfo.Extension);
                Request.Response.Header.AddHeader(new HttpHeader("Last-Modified",
                    fileInfo.LastWriteTime.ToUniversalTime().ToString("R")));
                try
                {
                    return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                }
                catch (Exception e)
                {
                    if (e is FileNotFoundException)
                        return null;
                    throw e;
                }
            }
            
            if (RootFilePath.Contains("."))
            {
                var pathMatch = Regex.Match(fileName, @"^\/?(.*(?=\/)\/)?([\/\w\.\-\[\]\(\)\x20]+)$");
                if (!pathMatch.Success)
                {
                    return null;
                }
                var fPath = pathMatch.Groups[1].Value;
                var fName = pathMatch.Groups[2].Value;
                fPath = Regex.Replace(fPath, @"[\x20\[\]]", "_");
                fPath = Regex.Replace(fPath, @"\/", ".");

                var resourcePath = RootFilePath + "." + fPath + fName;
#if DEBUG
                Debug.WriteInfo("Looking for resource stream", resourcePath);
#endif
                Request.Response.Header.ContentType =
                    MimeMapping.GetMimeMapping(Regex.Match(resourcePath, @".+(\.\w+)$").Groups[1].Value);
                try
                {
                    var result = assembly.GetManifestResourceStream(resourcePath);
#if DEBUG
                    Debug.WriteSuccess("Resource File Found");
#endif
                    return result;
                }
                catch
                {
#if DEBUG
                    Debug.WriteError("Resource Not Found");
#endif
                    return null;
                }
            }

            return null;
        }
    }
}