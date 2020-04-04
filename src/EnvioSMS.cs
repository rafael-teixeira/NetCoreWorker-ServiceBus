using Newtonsoft.Json;

namespace SMSWorker
{
    [JsonObject(MemberSerialization.OptOut)]
    public class EnvioSMS
    {
        public string destination;
        public string messageText;
        public string correlationId;
    }
}