﻿using System;
using System.ComponentModel.DataAnnotations;

namespace tasktServer.Models
{
    public class Assignment
    {
        [Key]
        public Guid AssignmentID { get; set; }
        public bool Enabled { get; set; }
        public string AssignmentName { get; set; }
        public Guid PublishedScriptID { get; set; }
        public Guid AssignedWorker { get; set; }
        public int Frequency { get; set; }
        public TimeInterval Interval { get; set; }
        public DateTime NewTaskDue { get; set; }

        public enum TimeInterval
        {
            Seconds, Minutes, Days, Months
        }
    }
}
