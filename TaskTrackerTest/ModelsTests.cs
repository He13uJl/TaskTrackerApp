using System;
using TaskTracker.Models;
using Xunit;
using Assert = Xunit.Assert;

namespace TaskTracker.Tests
{
    public class ModelsTests
    {
        private readonly DateTime _futureDate = DateTime.Today.AddDays(10);
        private readonly DateTime _pastDate = DateTime.Today.AddDays(-1);

        // ========== ТЕСТЫ ПРОСРОЧКИ ==========
        [Fact]
        public void Task_IsOverdue_WhenDeadlineInPastAndNotDone_ReturnsTrue()
        {
            var task = new UserTask
            {
                Deadline = _pastDate,
                Status = TaskState.ToDo
            };
            Assert.True(task.IsOverdue);
        }

        [Fact]
        public void Task_IsNotOverdue_WhenDeadlineInFuture()
        {
            var task = new UserTask
            {
                Deadline = _futureDate,
                Status = TaskState.InProgress
            };
            Assert.False(task.IsOverdue);
        }

        [Fact]
        public void Task_IsNotOverdue_WhenDeadlineInPastButStatusDone()
        {
            var task = new UserTask
            {
                Deadline = _pastDate,
                Status = TaskState.Done
            };
            Assert.False(task.IsOverdue);
        }

        [Fact]
        public void Task_IsNotOverdue_WhenNoDeadlineSet()
        {
            var task = new UserTask
            {
                Deadline = DateTime.MinValue,
                Status = TaskState.ToDo
            };
            Assert.False(task.IsOverdue);
        }

        // ========== ТЕСТЫ ПРИОРИТЕТОВ ==========
        [Fact]
        public void Task_Priority_CanBeSetToHigh()
        {
            var task = new UserTask { Priority = TaskPriority.Высокий };
            Assert.Equal(TaskPriority.Высокий, task.Priority);
        }

        [Fact]
        public void Task_Priority_CanBeSetToMedium()
        {
            var task = new UserTask { Priority = TaskPriority.Средний };
            Assert.Equal(TaskPriority.Средний, task.Priority);
        }

        [Fact]
        public void Task_Priority_CanBeSetToLow()
        {
            var task = new UserTask { Priority = TaskPriority.Низкий };
            Assert.Equal(TaskPriority.Низкий, task.Priority);
        }

        // ========== ТЕСТЫ СТАТУСОВ ==========
        [Fact]
        public void Task_Status_CanBeSetToDo()
        {
            var task = new UserTask { Status = TaskState.ToDo };
            Assert.Equal(TaskState.ToDo, task.Status);
        }

        [Fact]
        public void Task_Status_CanBeSetToInProgress()
        {
            var task = new UserTask { Status = TaskState.InProgress };
            Assert.Equal(TaskState.InProgress, task.Status);
        }

        [Fact]
        public void Task_Status_CanBeSetToDone()
        {
            var task = new UserTask { Status = TaskState.Done };
            Assert.Equal(TaskState.Done, task.Status);
        }

        // ========== ТЕСТЫ СТАТИСТИКИ ==========
        [Fact]
        public void Statistics_Update_CalculatesCompletedCorrectly()
        {
            var tasks = new[]
            {
                new UserTask { Status = TaskState.Done, Deadline = _futureDate },
                new UserTask { Status = TaskState.Done, Deadline = _futureDate },
                new UserTask { Status = TaskState.InProgress, Deadline = _futureDate },
                new UserTask { Status = TaskState.ToDo, Deadline = _futureDate }
            };

            var stats = new Statistics();
            stats.Update(tasks);

            Assert.Equal(2, stats.Completed);
            Assert.Equal(1, stats.InProgress);
            Assert.Equal(0, stats.Overdue);
        }

        [Fact]
        public void Statistics_Update_CalculatesOverdueCorrectly()
        {
            var tasks = new[]
            {
                new UserTask { Status = TaskState.ToDo, Deadline = _pastDate },
                new UserTask { Status = TaskState.ToDo, Deadline = _pastDate },
                new UserTask { Status = TaskState.InProgress, Deadline = _pastDate },
                new UserTask { Status = TaskState.Done, Deadline = _pastDate }
            };

            var stats = new Statistics();
            stats.Update(tasks);

            Assert.Equal(1, stats.Completed);
            Assert.Equal(1, stats.InProgress);
            Assert.Equal(3, stats.Overdue);
        }

        [Fact]
        public void Statistics_EmptyList_ReturnsZeros()
        {
            var stats = new Statistics();
            stats.Update(Array.Empty<UserTask>());

            Assert.Equal(0, stats.Completed);
            Assert.Equal(0, stats.InProgress);
            Assert.Equal(0, stats.Overdue);
        }

        // ========== КРАЕВЫЕ СЛУЧАИ ==========
        [Fact]
        public void Task_CanHaveEmptyName()
        {
            var task = new UserTask { Name = "" };
            Assert.Equal("", task.Name);
        }

        [Fact]
        public void Task_CanHaveNullName()
        {
            var task = new UserTask { Name = null };
            Assert.Null(task.Name);
        }

        [Fact]
        public void Task_CanHaveVeryLongName()
        {
            var longName = new string('A', 500);
            var task = new UserTask { Name = longName };
            Assert.Equal(500, task.Name.Length);
        }

        [Fact]
        public void Sprint_CanHaveEmptyTasksList()
        {
            var sprint = new Sprint();
            Assert.NotNull(sprint.Tasks);
            Assert.Empty(sprint.Tasks);
        }

        [Fact]
        public void Sprint_CanHaveTasks()
        {
            var sprint = new Sprint();
            var task = new UserTask { Name = "Test Task" };
            sprint.Tasks.Add(task);

            Assert.Single(sprint.Tasks);
            Assert.Equal("Test Task", sprint.Tasks[0].Name);
        }
    }
}