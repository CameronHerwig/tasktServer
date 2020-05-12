using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace tasktServer.Models
{
    public class MetricRequest
    {
        public DateTime? StartDate { get; set; }
        public StatusCode Status { get; set; }
        public enum StatusCode
        {
            Completed, Closed, Errored, Running
        }
    }
}

