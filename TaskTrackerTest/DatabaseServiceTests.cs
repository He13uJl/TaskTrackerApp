using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Data;
using TaskTracker.Models;
using TaskTracker.Services;
using Xunit;
using Assert = Xunit.Assert;

namespace TaskTracker.Tests
{
    public class DatabaseServiceTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly DatabaseService _service;

        public DatabaseServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new AppDbContext(options);
            _service = new DatabaseService(_context);
        }

        // ========== ТЕСТЫ СПРИНТОВ ==========

        [Fact]
        public void CreateSprint_ValidData_ReturnsSprint()
        {
            var sprint = _service.CreateSprint("Спринт 1", DateTime.Now, DateTime.Now.AddDays(14));

            Assert.NotNull(sprint);
            Assert.Equal("Спринт 1", sprint.Name);
            Assert.NotEqual(0, sprint.Id);
        }

        [Fact]
        public void CreateSprint_EmptyName_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() =>
                _service.CreateSprint("", DateTime.Now, DateTime.Now.AddDays(14)));
        }

        [Fact]
        public void CreateSprint_EndDateBeforeStart_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() =>
                _service.CreateSprint("Спринт", DateTime.Now.AddDays(14), DateTime.Now));
        }

        [Fact]
        public void CreateSprint_EndDateEqualsStart_ThrowsException()
        {
            var now = DateTime.Now;
            Assert.Throws<ArgumentException>(() =>
                _service.CreateSprint("Спринт", now, now));
        }

        [Fact]
        public void GetAllSprints_ReturnsAllSprints()
        {
            _service.CreateSprint("Спринт 1", DateTime.Now, DateTime.Now.AddDays(7));
            _service.CreateSprint("Спринт 2", DateTime.Now, DateTime.Now.AddDays(14));

            var sprints = _service.GetAllSprints();

            Assert.Equal(2, sprints.Count);
        }

        [Fact]
        public void GetSprintById_ReturnsCorrectSprint()
        {
            var sprint = _service.CreateSprint("Спринт 1", DateTime.Now, DateTime.Now.AddDays(7));

            var found = _service.GetSprintById(sprint.Id);

            Assert.NotNull(found);
            Assert.Equal(sprint.Id, found.Id);
            Assert.Equal("Спринт 1", found.Name);
        }

        [Fact]
        public void GetSprintById_NotFound_ReturnsNull()
        {
            var found = _service.GetSprintById(999);
            Assert.Null(found);
        }

        [Fact]
        public void DeleteSprint_RemovesSprint()
        {
            var sprint = _service.CreateSprint("Спринт", DateTime.Now, DateTime.Now.AddDays(7));
            _service.DeleteSprint(sprint.Id);

            var sprints = _service.GetAllSprints();
            Assert.DoesNotContain(sprints, s => s.Id == sprint.Id);
        }

        // ========== ТЕСТЫ ЗАДАЧ ==========

        [Fact]
        public void CreateTask_ValidData_ReturnsUserTask()
        {
            var sprint = _service.CreateSprint("Спринт", DateTime.Now, DateTime.Now.AddDays(14));
            var task = _service.CreateTask("Задача 1", TaskPriority.Высокий, DateTime.Now.AddDays(7), sprint.Id);

            Assert.NotNull(task);
            Assert.Equal("Задача 1", task.Name);
            Assert.Equal(TaskState.ToDo, task.Status);
            Assert.Equal(TaskPriority.Высокий, task.Priority);
        }

        [Fact]
        public void CreateTask_EmptyName_ThrowsException()
        {
            var sprint = _service.CreateSprint("Спринт", DateTime.Now, DateTime.Now.AddDays(14));

            Assert.Throws<ArgumentException>(() =>
                _service.CreateTask("", TaskPriority.Средний, DateTime.Now.AddDays(7), sprint.Id));
        }

        [Fact]
        public void CreateTask_SprintNotFound_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() =>
                _service.CreateTask("Задача", TaskPriority.Средний, DateTime.Now, 999));
        }

        [Fact]
        public void GetTasksBySprint_ReturnsCorrectTasks()
        {
            var sprint = _service.CreateSprint("Спринт", DateTime.Now, DateTime.Now.AddDays(14));
            _service.CreateTask("Задача 1", TaskPriority.Средний, DateTime.Now.AddDays(7), sprint.Id);
            _service.CreateTask("Задача 2", TaskPriority.Высокий, DateTime.Now.AddDays(5), sprint.Id);

            var tasks = _service.GetTasksBySprint(sprint.Id);

            Assert.Equal(2, tasks.Count);
        }

        // ========== ТЕСТЫ СМЕНЫ СТАТУСА ==========

        [Fact]
        public void UpdateTaskStatus_ValidTask_ChangesStatus()
        {
            var sprint = _service.CreateSprint("Спринт", DateTime.Now, DateTime.Now.AddDays(14));
            var task = _service.CreateTask("Задача", TaskPriority.Низкий, DateTime.Now.AddDays(7), sprint.Id);

            _service.UpdateTaskStatus(task.Id, TaskState.InProgress);
            var updatedTask = _service.GetTasksBySprint(sprint.Id).First();

            Assert.Equal(TaskState.InProgress, updatedTask.Status);
        }

        [Fact]
        public void UpdateTaskStatus_ToDoToInProgress_Works()
        {
            var sprint = _service.CreateSprint("Спринт", DateTime.Now, DateTime.Now.AddDays(14));
            var task = _service.CreateTask("Задача", TaskPriority.Средний, DateTime.Now.AddDays(7), sprint.Id);

            _service.UpdateTaskStatus(task.Id, TaskState.InProgress);
            var updated = _service.GetTasksBySprint(sprint.Id).First();

            Assert.Equal(TaskState.InProgress, updated.Status);
        }

        [Fact]
        public void UpdateTaskStatus_InProgressToDone_Works()
        {
            var sprint = _service.CreateSprint("Спринт", DateTime.Now, DateTime.Now.AddDays(14));
            var task = _service.CreateTask("Задача", TaskPriority.Средний, DateTime.Now.AddDays(7), sprint.Id);
            _service.UpdateTaskStatus(task.Id, TaskState.InProgress);
            _service.UpdateTaskStatus(task.Id, TaskState.Done);

            var updated = _service.GetTasksBySprint(sprint.Id).First();

            Assert.Equal(TaskState.Done, updated.Status);
        }

        [Fact]
        public void UpdateTaskStatus_TaskNotFound_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() =>
                _service.UpdateTaskStatus(999, TaskState.Done));
        }

        // ========== ТЕСТЫ СТАТИСТИКИ ==========

        [Fact]
        public void GetStatistics_ReturnsCorrectCounts()
        {
            var sprint = _service.CreateSprint("Спринт", DateTime.Now, DateTime.Now.AddDays(14));
            _service.CreateTask("Задача 1", TaskPriority.Средний, DateTime.Now.AddDays(7), sprint.Id);
            var task2 = _service.CreateTask("Задача 2", TaskPriority.Средний, DateTime.Now.AddDays(7), sprint.Id);
            _service.UpdateTaskStatus(task2.Id, TaskState.Done);

            var stats = _service.GetStatistics(sprint.Id);

            Assert.Equal(1, stats.Completed);
            Assert.Equal(0, stats.InProgress);
        }

        [Fact]
        public void GetStatistics_OverdueTask_CountsCorrectly()
        {
            var sprint = _service.CreateSprint("Спринт", DateTime.Now.AddDays(-10), DateTime.Now.AddDays(4));
            _service.CreateTask("Просрочена", TaskPriority.Высокий, DateTime.Now.AddDays(-2), sprint.Id);
            _service.CreateTask("Не просрочена", TaskPriority.Низкий, DateTime.Now.AddDays(3), sprint.Id);

            var stats = _service.GetStatistics(sprint.Id);

            Assert.Equal(1, stats.Overdue);
        }

        [Fact]
        public void GetStatistics_NoTasks_ReturnsZeros()
        {
            var sprint = _service.CreateSprint("Спринт", DateTime.Now, DateTime.Now.AddDays(14));
            var stats = _service.GetStatistics(sprint.Id);

            Assert.Equal(0, stats.Completed);
            Assert.Equal(0, stats.InProgress);
            Assert.Equal(0, stats.Overdue);
        }

        // ========== ТЕСТЫ УДАЛЕНИЯ ==========

        [Fact]
        public void DeleteTask_RemovesTask()
        {
            var sprint = _service.CreateSprint("Спринт", DateTime.Now, DateTime.Now.AddDays(14));
            var task = _service.CreateTask("Задача", TaskPriority.Средний, DateTime.Now.AddDays(7), sprint.Id);
            _service.DeleteTask(task.Id);

            var tasks = _service.GetTasksBySprint(sprint.Id);
            Assert.Empty(tasks);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}