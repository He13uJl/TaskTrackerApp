using System.Collections.Generic;
using System.Linq;

namespace TaskTracker.Models
{
    public class Statistics
    {
        public int Completed { get; set; }
        public int InProgress { get; set; }
        public int Overdue { get; set; }

        public void Update(IEnumerable<UserTask> tasks)
        {
            var taskList = tasks.ToList();

            Completed = taskList.Count(t => t.Status == TaskState.Done);
            InProgress = taskList.Count(t => t.Status == TaskState.InProgress);

            Overdue = taskList.Count(t =>
                t.Status != TaskState.Done &&
                t.Deadline != DateTime.MinValue &&
                t.Deadline.Date < DateTime.Today);
        }
    }
}