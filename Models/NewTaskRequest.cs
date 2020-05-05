using System;

namespace tasktServer.Models
{
    public class NewTaskRequest
    {
        public Guid WorkerID { get; set; }
        public Guid PublishedScriptID { get; set; }
    }
}
