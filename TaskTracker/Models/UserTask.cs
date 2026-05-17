using System;

namespace TaskTracker.Models
{
    public enum TaskState
    {
        ToDo,
        InProgress,
        Done
    }

    public enum TaskPriority
    {
        Низкий,
        Средний,
        Высокий
    }

    public class UserTask
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public TaskState Status { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime Deadline { get; set; }
        public int SprintId { get; set; }
        public Sprint Sprint { get; set; }

        public bool IsOverdue
        {
            get
            {
                if (Deadline == DateTime.MinValue)
                    return false;

                return Deadline.Date < DateTime.Today && Status != TaskState.Done;
            }
        }
    }
}