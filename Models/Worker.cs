using System;
using System.ComponentModel.DataAnnotations;

namespace tasktServer.Models
{
    public class Worker
    {
        [Key]
        public Guid WorkerID { get; set; }
        public string MachineName { get; set; }
        public string UserName { get; set; }
        public DateTime LastCheckIn { get; set; }
        public WorkerStatus Status { get; set; }
    }

    public enum WorkerStatus
    {
        Pending = 0,
        Authorized = 1,
        Revoked = 2
    }


}
