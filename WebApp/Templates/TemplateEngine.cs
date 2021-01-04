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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.Reflection;
using UX.Lib2.Cloud.Logger;

namespace UX.Lib2.WebApp.Templates
{
    public class TemplateEngine
    {
        #region Fields

        private readonly string _templateContents;
        private readonly Dictionary<string, object> _context;

        #endregion

        #region Constructors

        public TemplateEngine(Assembly assembly, string resourcePath, string title, bool loggedIn)
        {
            _templateContents = GetResourceContents(assembly, resourcePath);

            while (Regex.IsMatch(_templateContents, @"{% extends ([\.\w]+) %}"))
            {
                var contents = _templateContents;
                
                var block = GetResourceContents(assembly, Regex.Match(_templateContents, @"{% extends ([\.\w]+) %}").Groups[1].Value);

                _templateContents = Regex.Replace(block, @"{{ block_content }}",
                    match => Regex.Replace(contents, @"{% extends ([\.\w]+) %}", string.Empty));
            }

            _context = new Dictionary<string, object>
            {
                {"page_title", title},
                {"processor_prompt", InitialParametersClass.ControllerPromptName},
                {
                    "processor_ip_address",
                    CrestronEthernetHelper.GetEthernetParameter(
                        CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS,
                        CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(
                            EthernetAdapterType.EthernetLANAdapter))
                },
                {
                    "processor_hostname",
                    CrestronEthernetHelper.GetEthernetParameter(
                        CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_HOSTNAME,
                        CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(
                            EthernetAdapterType.EthernetLANAdapter)).ToUpper()
                },
                {"authentication_enabled", InitialParametersClass.IsAuthenticationEnabled},
                {"login_logout_button_name", loggedIn ? "Logout" : "Login"},
                {"login_logout_link", loggedIn ? "/logout" : "/login"}
            };

            var domain = CrestronEthernetHelper.GetEthernetParameter(
                CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_DOMAIN_NAME,
                CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(
                    EthernetAdapterType.EthernetLANAdapter));

            domain = domain.Length > 0 ? domain : "local";
            _context.Add("processor_domain", domain);
            _context.Add("processor_fqdn", _context["processor_hostname"] + "." + domain);
            _context.Add("system_name", _context["processor_hostname"]);
        }

        #endregion

        #region Finalizers
        #endregion

        #region Events
        #endregion

        #region Delegates
        #endregion

        #region Properties

        public Dictionary<string, object> Context
        {
            get { return _context; }
        }

        #endregion

        #region Methods

        public static string GetResourceContents(Assembly assembly, string resourcePath)
        {
            var path = assembly.GetName().Name + "." + resourcePath;

            if (assembly.GetManifestResourceNames().All(n => n != path))
            {
                throw new FileNotFoundException("Could not open file stream: \"" + path + "\"");
            }

            try
            {
                var stream = assembly.GetManifestResourceStream(path);

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
                throw new FileNotFoundException("Could not open file stream: \"" + path + "\"", e);
            }
        }

        private static object GetObjectFromContext(string name, Dictionary<string, object> context)
        {
            try
            {
                if (!name.Contains("."))
                {
                    return context[name];
                }

                var split = name.Split('.');

                var item = context[split[0]];
                var propertyName = split[1];

                foreach (var propertyInfo in item.GetType()
                    .GetCType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    if (propertyInfo.Name == propertyName)
                    {
                        return propertyInfo.GetValue(item, null);
                    }

                    if (propertyInfo.Name == "Item" && propertyInfo.GetIndexParameters().Length > 0)
                    {
                        return propertyInfo.GetValue(item, new object[] {propertyName});
                    }
                }

                foreach (var fieldInfo in item.GetType()
                    .GetCType()
                    .GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    if (fieldInfo.Name == propertyName)
                    {
                        return fieldInfo.GetValue(item);
                    }
                }
            }
            catch (Exception e)
            {
                CloudLog.Exception(e);
            }

            return string.Format("Error({0})", name);
        }

        private static bool EvaluateStatement(string statementString, Dictionary<string, object> context)
        {
            var statement = Regex.Match(statementString,
                @"(!?[\w.]+)(?: ([!=<>]{1,2}) ([\w.]+))?");

            if (!statement.Success) throw new Exception("Statement could not be matched");

            var leftObject = GetObjectFromContext(statement.Groups[1].Value, context);

            if (statement.Groups[2].Success && statement.Groups[3].Success)
            {
                var rightObject = GetObjectFromContext(statement.Groups[3].Value, context);

                switch (statement.Groups[3].Value)
                {
                    case "==":
                        return leftObject == rightObject;
                    case "!=":
                        return leftObject != rightObject;
                    default:
                        throw new Exception("Statement could not be matched");
                }
            }

            if (!statement.Groups[1].Value.StartsWith("!")) return (bool) leftObject;
            var b = (bool) leftObject;
            return !b;
        }

        private static string ParseContent(string contents, Dictionary<string, object> context)
        {
            var newContents = contents;

            //Loop through and process any if statements
            newContents = Regex.Replace(newContents, @"{% if (.+?) %}([\s\S]*?){% endif %}", statementMatch =>
            {
                var trueResult = string.Empty;
                var falseResult = string.Empty;

                if (statementMatch.Groups[2].Value.Contains("{% else %}"))
                {
                    var split = Regex.Split(statementMatch.Groups[2].Value, @"{% else %}");
                    trueResult = split[0];
                    falseResult = split[1];
                }
                else
                {
                    trueResult = statementMatch.Groups[2].Value;
                }

                try
                {
                    return EvaluateStatement(statementMatch.Groups[1].Value, context) ? trueResult : falseResult;
                }
                catch (Exception e)
                {
                    CloudLog.Exception(e);
                    return statementMatch.ToString();
                }
            });

            //Loop through and process variables
            newContents = Regex.Replace(newContents, @"{{ ([\w.]+)(?: ([A-Z]))? }}",
                varMatch =>
                {
                    try
                    {
                        var item = GetObjectFromContext(varMatch.Groups[1].Value, context);
                        if (item == null)
                        {
#if DEBUG
                            //return string.Format("{0}=null", varMatch.Groups[1].Value);
#endif
                            return string.Empty;
                        }
                        
                        var result = item.ToString();

                        if (string.IsNullOrEmpty(varMatch.Groups[2].Value))
                        {
                            return result;
                        }

                        switch (varMatch.Groups[2].Value)
                        {
                            case "U":
                                return result.ToUpper();
                            case "L":
                                return result.ToLower();
                            case "S":
                                return result.SplitCamelCase();
                            default:
                                return result + " " + varMatch.Groups[2].Value;
                        }
                    }
                    catch(Exception e)
                    {
                        CloudLog.Exception(e);
                        return string.Format("Error({0})", varMatch.ToString());
                    }
                });

            return newContents;
        }

        public string Render()
        {
            var contents = _templateContents;

            //Loop through and process any for loops
            contents = Regex.Replace(contents, @"{% for (.+) %}(?:[\r\n]|\r|\n)?([\s\S]*?)(?:\s*)?{% endfor %}", loopMatch =>
            {
                var statement = Regex.Match(loopMatch.Groups[1].Value, @"(\w+) in (\w+)(?:.(\w+))?");
                var enumerable = Context[statement.Groups[2].Value] as IEnumerable;
                if (enumerable == null) return loopMatch.ToString();
                
                var result = string.Empty;

                foreach (var item in enumerable)
                {
                    var localContext = Context.ToDictionary(d => d.Key, d => d.Value);
                    localContext[statement.Groups[1].Value] = item;

                    result = result + ParseContent(loopMatch.Groups[2].Value, localContext);
                }

                return result;
            });

            //Parse document
            contents = ParseContent(contents, Context);

            return contents;
        }

        #endregion
    }
}