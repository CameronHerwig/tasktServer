using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tasktServer.Models
{
    public class WorkerPool
    {
        [Key]
        public Guid WorkerPoolID { get; set; }
        public string WorkerPoolName { get; set; }
        public List<AssignedPoolWorker> PoolWorkers { get; set; }
    }
    public class AssignedPoolWorker
    {
        [Key]
        public Guid AssignedPoolWorkerItemID { get; set; }

        [ForeignKey("WorkerID")]
        public Guid WorkerID { get; set; }
    }
}
