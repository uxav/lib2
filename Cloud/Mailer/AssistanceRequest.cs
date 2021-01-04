using Newtonsoft.Json;

namespace UX.Lib2.Cloud.Mailer
{
    public class AssistanceRequest
    {
        [JsonProperty(PropertyName = "id")]
        public long ID { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Title { get; set; }
        [JsonProperty(PropertyName = "email_address")]
        public string EmailAddress { get; set; }
    }
}