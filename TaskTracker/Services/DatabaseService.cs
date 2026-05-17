using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Data;
using TaskTracker.Models;

namespace TaskTracker.Services
{
    public class DatabaseService : IDisposable
    {
        private readonly AppDbContext _context;

        public DatabaseService()
        {
            _context = new AppDbContext();
            _context.Database.EnsureCreated();
        }

        public DatabaseService(AppDbContext context)
        {
            _context = context;
        }

        // ========== СПРИНТЫ ==========
        public List<Sprint> GetAllSprints()
        {
            return _context.Sprints.Include(s => s.Tasks).ToList();
        }

        public Sprint GetSprintById(int id)
        {
            return _context.Sprints.Include(s => s.Tasks).FirstOrDefault(s => s.Id == id);
        }

        public Sprint CreateSprint(string name, DateTime start, DateTime end)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Название спринта не может быть пустым");

            if (end <= start)
                throw new ArgumentException("Дата окончания должна быть позже даты начала");

            var sprint = new Sprint
            {
                Name = name,
                StartDate = start,
                EndDate = end
            };

            _context.Sprints.Add(sprint);
            _context.SaveChanges();
            return sprint;
        }

        public void UpdateSprint(Sprint sprint)
        {
            if (sprint.EndDate <= sprint.StartDate)
                throw new ArgumentException("Дата окончания должна быть позже даты начала");

            _context.Entry(sprint).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public void DeleteSprint(int id)
        {
            var sprint = _context.Sprints.Find(id);
            if (sprint != null)
            {
                _context.Sprints.Remove(sprint);
                _context.SaveChanges();
            }
        }

        // ========== ЗАДАЧИ ==========
        public UserTask CreateTask(string name, TaskPriority priority, DateTime deadline, int sprintId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Название задачи не может быть пустым");

            var sprint = _context.Sprints.Find(sprintId);
            if (sprint == null)
                throw new ArgumentException("Спринт не найден");

            var task = new UserTask
            {
                Name = name,
                Priority = priority,
                Deadline = deadline,
                Status = TaskState.ToDo,
                SprintId = sprintId
            };

            _context.Tasks.Add(task);
            _context.SaveChanges();
            return task;
        }

        public void UpdateTaskStatus(int taskId, TaskState newStatus)
        {
            var task = _context.Tasks.Find(taskId);
            if (task == null)
                throw new ArgumentException("Задача не найдена");

            task.Status = newStatus;
            _context.SaveChanges();
        }

        public void UpdateTask(UserTask task)
        {
            _context.Entry(task).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public void DeleteTask(int id)
        {
            var task = _context.Tasks.Find(id);
            if (task != null)
            {
                _context.Tasks.Remove(task);
                _context.SaveChanges();
            }
        }

        public List<UserTask> GetTasksBySprint(int sprintId)
        {
            return _context.Tasks.Where(t => t.SprintId == sprintId).ToList();
        }

        // ========== СТАТИСТИКА ==========
        public Statistics GetStatistics(int sprintId)
        {
            var tasks = GetTasksBySprint(sprintId);
            var stats = new Statistics();
            stats.Update(tasks);
            return stats;
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}