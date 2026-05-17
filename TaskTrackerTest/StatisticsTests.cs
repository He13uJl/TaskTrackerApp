using System;
using System.Collections.Generic;
using TaskTracker.Models;
using Xunit;
using Assert = Xunit.Assert;

namespace TaskTracker.Tests
{
    public class StatisticsTests
    {
        private readonly DateTime _futureDate;
        private readonly DateTime _pastDate;

        public StatisticsTests()
        {
            _futureDate = DateTime.Today.AddDays(10);
            _pastDate = DateTime.Today.AddDays(-1);
        }

        [Fact]
        public void Statistics_Constructor_InitializesWithZeros()
        {
            var stats = new Statistics();
            Assert.Equal(0, stats.Completed);
            Assert.Equal(0, stats.InProgress);
            Assert.Equal(0, stats.Overdue);
        }

        [Fact]
        public void Statistics_Update_WithOnlyCompletedTasks_ReturnsCorrectCounts()
        {
            var stats = new Statistics();
            var tasks = new List<UserTask>
            {
                new UserTask { Status = TaskState.Done, Deadline = _futureDate },
                new UserTask { Status = TaskState.Done, Deadline = _futureDate }
            };

            stats.Update(tasks);

            Assert.Equal(2, stats.Completed);
            Assert.Equal(0, stats.InProgress);
            Assert.Equal(0, stats.Overdue);
        }

        [Fact]
        public void Statistics_Update_WithOnlyInProgressTasks_ReturnsCorrectCounts()
        {
            var stats = new Statistics();
            var tasks = new List<UserTask>
            {
                new UserTask { Status = TaskState.InProgress, Deadline = _futureDate },
                new UserTask { Status = TaskState.InProgress, Deadline = _futureDate }
            };

            stats.Update(tasks);

            Assert.Equal(0, stats.Completed);
            Assert.Equal(2, stats.InProgress);
            Assert.Equal(0, stats.Overdue);
        }

        [Fact]
        public void Statistics_Update_WithOnlyToDoTasks_ReturnsCorrectCounts()
        {
            var stats = new Statistics();
            var tasks = new List<UserTask>
            {
                new UserTask { Status = TaskState.ToDo, Deadline = _futureDate },
                new UserTask { Status = TaskState.ToDo, Deadline = _futureDate },
                new UserTask { Status = TaskState.ToDo, Deadline = _futureDate },
                new UserTask { Status = TaskState.ToDo, Deadline = _futureDate }
            };

            stats.Update(tasks);

            Assert.Equal(0, stats.Completed);
            Assert.Equal(0, stats.InProgress);
            Assert.Equal(0, stats.Overdue);
        }

        [Fact]
        public void Statistics_Update_WithMixedTasks_ReturnsCorrectCounts()
        {
            var stats = new Statistics();
            var tasks = new List<UserTask>
            {
                new UserTask { Status = TaskState.Done, Deadline = _futureDate },
                new UserTask { Status = TaskState.InProgress, Deadline = _futureDate },
                new UserTask { Status = TaskState.ToDo, Deadline = _futureDate },
                new UserTask { Status = TaskState.Done, Deadline = _futureDate },
                new UserTask { Status = TaskState.InProgress, Deadline = _futureDate }
            };

            stats.Update(tasks);

            Assert.Equal(2, stats.Completed);
            Assert.Equal(2, stats.InProgress);
            Assert.Equal(0, stats.Overdue);
        }

        [Fact]
        public void Statistics_Update_WithOverdueTasks_ReturnsCorrectCounts()
        {
            var stats = new Statistics();
            var tasks = new List<UserTask>
            {
                new UserTask { Status = TaskState.ToDo, Deadline = _pastDate },      // Просрочена
                new UserTask { Status = TaskState.ToDo, Deadline = _pastDate },      // Просрочена
                new UserTask { Status = TaskState.InProgress, Deadline = _pastDate }, // Просрочена
                new UserTask { Status = TaskState.Done, Deadline = _pastDate },       // Выполнена (не просрочена)
                new UserTask { Status = TaskState.ToDo, Deadline = _futureDate }      // Не просрочена
            };

            stats.Update(tasks);

            Assert.Equal(1, stats.Completed);
            Assert.Equal(1, stats.InProgress);
            Assert.Equal(3, stats.Overdue);
        }

        [Fact]
        public void Statistics_Update_DoesNotCountDoneTasksAsOverdue()
        {
            var stats = new Statistics();
            var tasks = new List<UserTask>
            {
                new UserTask { Status = TaskState.Done, Deadline = _pastDate }
            };

            stats.Update(tasks);

            Assert.Equal(1, stats.Completed);
            Assert.Equal(0, stats.InProgress);
            Assert.Equal(0, stats.Overdue);
        }

        [Fact]
        public void Statistics_Update_EmptyList_ReturnsZeros()
        {
            var stats = new Statistics();
            stats.Update(new List<UserTask>());

            Assert.Equal(0, stats.Completed);
            Assert.Equal(0, stats.InProgress);
            Assert.Equal(0, stats.Overdue);
        }

        [Fact]
        public void Statistics_Update_TasksWithoutDeadline_AreNotOverdue()
        {
            var stats = new Statistics();
            var tasks = new List<UserTask>
            {
                new UserTask { Status = TaskState.ToDo, Deadline = DateTime.MinValue },
                new UserTask { Status = TaskState.InProgress, Deadline = DateTime.MinValue }
            };

            stats.Update(tasks);

            Assert.Equal(0, stats.Completed);
            Assert.Equal(1, stats.InProgress);
            Assert.Equal(0, stats.Overdue);
        }
    }
}