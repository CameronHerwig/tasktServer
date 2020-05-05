using System;

namespace tasktServer.Models
{
    public class BotStoreRequest
    {
        public Guid WorkerID { get; set; }
        public string BotStoreName { get; set; }
        public RequestType Type { get; set; }
        public enum RequestType
        {
            BotStoreValue,
            BotStoreModel
        }
    }
}
