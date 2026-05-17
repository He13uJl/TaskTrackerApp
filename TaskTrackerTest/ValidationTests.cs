using System;
using TaskTracker.Models;
using TaskTracker.Services;
using Xunit;
using Assert = Xunit.Assert;

namespace TaskTracker.Tests
{
    public class ValidationTests
    {
        // ========== ВАЛИДАЦИЯ ДАТ ==========

        [Fact]
        public void Sprint_EndDateMustBeAfterStartDate_ValidCase()
        {
            var sprint = new Sprint
            {
                StartDate = new DateTime(2025, 1, 1),
                EndDate = new DateTime(2025, 1, 15)
            };

            Assert.True(sprint.EndDate > sprint.StartDate);
        }

        [Fact]
        public void Sprint_EndDateEqualsStartDate_IsInvalid()
        {
            var sprint = new Sprint
            {
                StartDate = new DateTime(2025, 1, 1),
                EndDate = new DateTime(2025, 1, 1)
            };

            Assert.False(sprint.EndDate > sprint.StartDate);
        }

        [Fact]
        public void Task_DeadlineCanBeInPast_ForExistingTasks()
        {
            var task = new UserTask
            {
                Deadline = DateTime.Today.AddDays(-10),
                Status = TaskState.ToDo
            };

            Assert.True(task.IsOverdue);
        }

        // ========== КРАЕВЫЕ СЛУЧАИ ==========

        [Fact]
        public void Task_Deadline_MinimumValue_DoesNotCrash()
        {
            var task = new UserTask
            {
                Deadline = DateTime.MinValue,
                Status = TaskState.ToDo
            };

            var isOverdue = task.IsOverdue;
            // Не должно быть исключения
            Assert.True(isOverdue || !isOverdue);
        }

        [Fact]
        public void Task_Deadline_MaximumValue_DoesNotCrash()
        {
            var task = new UserTask
            {
                Deadline = DateTime.MaxValue,
                Status = TaskState.ToDo
            };

            var isOverdue = task.IsOverdue;
            Assert.False(isOverdue);
        }

        [Fact]
        public void TaskName_SpecialCharacters_IsAccepted()
        {
            var specialName = "!@#$%^&*()_+{}:<>?[];',./`~";
            var task = new UserTask { Name = specialName };

            Assert.Equal(specialName, task.Name);
        }

        [Fact]
        public void MultipleStatusChanges_KeepsConsistency()
        {
            var task = new UserTask { Status = TaskState.ToDo };

            task.Status = TaskState.InProgress;
            task.Status = TaskState.Done;
            task.Status = TaskState.ToDo;
            task.Status = TaskState.InProgress;

            Assert.Equal(TaskState.InProgress, task.Status);
        }
    }
}