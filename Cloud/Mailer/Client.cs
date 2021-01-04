using System;
using System.Collections.Generic;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Net.Https;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UX.Lib2.Cloud.Logger;

namespace UX.Lib2.Cloud.Mailer
{
    public class CloudClient
    {
        public CloudClient(string apiKey)
        {
            _apiKey = apiKey;
            _client = new HttpsClient();
        }

        private readonly string _apiKey;
        private readonly HttpsClient _client;

        private string Post(string method, string request)
        {
            var uri = string.Format("https://crestroncloudmail.appspot.com/api/{0}", method);
#if DEBUG
            CrestronConsole.PrintLine("Posting to {0}\r\n{1}", uri, request);
#endif
            var result = _client.Post(uri,
                Encoding.UTF8.GetBytes(request));
#if DEBUG
            CrestronConsole.PrintLine("\r\nReceived Content:\r\n{0}", result);
#endif
            return result;
        }

        public IEnumerable<AssistanceRequest> GetHelpTasks(string siteId)
        {
            var jsonData = JsonConvert.SerializeObject(new
            {
                api_key = _apiKey,
                site_id = siteId
            }, Formatting.Indented);

            JObject result = JObject.Parse(Post("listtasks", jsonData));

            CrestronConsole.PrintLine("\r\n{0}", result);

            return result["tasks"].ToObject<List<AssistanceRequest>>();
        }

        public bool RequestTask(string siteId, long taskId, string roomName)
        {
            try
            {
                short adapter =
                    CrestronEthernetHelper.GetAdapterdIdForSpecifiedAdapterType(EthernetAdapterType.EthernetLANAdapter);
                string systemId =
                    CrestronEthernetHelper.GetEthernetParameter(
                        CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_MAC_ADDRESS, adapter);
                string ipAddress =
                    CrestronEthernetHelper.GetEthernetParameter(
                        CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, adapter);

                var jsonData = JsonConvert.SerializeObject(new
                {
                    api_key = _apiKey,
                    site_id = siteId,
                    system_id = systemId,
                    system_ip_address = ipAddress,
                    room_name = roomName,
                    task_id = taskId
                }, Formatting.Indented);

                JObject result = JObject.Parse(Post("sendmail", jsonData));

                CrestronConsole.PrintLine("\r\n{0}", result);

                return result["status"].Value<string>() == "200 OK";
            }
            catch (Exception e)
            {
                CloudLog.Error("AssistanceClient.RequestTask(taskId = {0}, roomName = {1}) Error, {2}",
                    taskId, roomName, e.Message);
                return false;
            }
        }
    }
}