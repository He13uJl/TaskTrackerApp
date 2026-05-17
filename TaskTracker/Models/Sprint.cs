using System;
using System.Collections.Generic;

namespace TaskTracker.Models
{
    public class Sprint
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<UserTask> Tasks { get; set; } = new List<UserTask>();
    }
}