using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TaskTracker.Models;
using TaskTracker.Services;

namespace TaskTracker.Views
{
    public partial class MainWindow : Window
    {
        private DatabaseService _dbService;
        private Sprint _currentSprint;
        private System.Collections.ObjectModel.ObservableCollection<Sprint> _sprints;

        public MainWindow()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            _sprints = new System.Collections.ObjectModel.ObservableCollection<Sprint>();

            NewTaskDeadlinePicker.SelectedDate = DateTime.Now;
            NewSprintStartPicker.SelectedDate = DateTime.Now;
            NewSprintEndPicker.SelectedDate = DateTime.Now.AddDays(14);

            LoadData();
        }

        private void LoadData()
        {
            _sprints = new System.Collections.ObjectModel.ObservableCollection<Sprint>(_dbService.GetAllSprints());
            SprintComboBox.ItemsSource = _sprints;

            if (_sprints.Any())
            {
                _currentSprint = _sprints.First();
                SprintComboBox.SelectedItem = _currentSprint;
            }

            RefreshKanban();
            UpdateStatistics();
            RefreshTaskSelector();
        }

        private void RefreshKanban()
        {
            if (_currentSprint == null) return;

            var tasks = _dbService.GetTasksBySprint(_currentSprint.Id);

            TodoListBox.ItemsSource = tasks.Where(t => t.Status == TaskState.ToDo).ToList();
            InProgressListBox.ItemsSource = tasks.Where(t => t.Status == TaskState.InProgress).ToList();
            DoneListBox.ItemsSource = tasks.Where(t => t.Status == TaskState.Done).ToList();
        }

        private void UpdateStatistics()
        {
            if (_currentSprint == null) return;

            var stats = _dbService.GetStatistics(_currentSprint.Id);
            var tasks = _dbService.GetTasksBySprint(_currentSprint.Id);

            StatsCompleted.Text = $"✅ Выполнено: {stats.Completed}";
            StatsInProgress.Text = $"⚙ В работе: {stats.InProgress}";
            StatsOverdue.Text = $"⚠ Просрочено: {stats.Overdue}";
            StatsTotal.Text = $"📊 Всего задач: {tasks.Count}";
        }

        private void RefreshTaskSelector()
        {
            if (_currentSprint == null)
            {
                TaskSelector.ItemsSource = null;
                return;
            }

            var tasks = _dbService.GetTasksBySprint(_currentSprint.Id);
            TaskSelector.ItemsSource = tasks;
            if (tasks.Any() && TaskSelector.SelectedItem == null)
                TaskSelector.SelectedIndex = 0;
        }

        private void SprintComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SprintComboBox.SelectedItem is Sprint sprint)
            {
                _currentSprint = sprint;
                RefreshKanban();
                UpdateStatistics();
                RefreshTaskSelector();
            }
        }

        private void TaskSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TaskSelector.SelectedItem is UserTask task)
            {
                SelectedTaskInfo.Text = $"✅ Выбрана задача: \"{task.Name}\"";
                MoveTaskButton.IsEnabled = true;
                MoveTaskButton.Background = System.Windows.Media.Brushes.Orange;
            }
            else
            {
                SelectedTaskInfo.Text = "✏️ Выберите задачу и колонку для перемещения";
                MoveTaskButton.IsEnabled = false;
                MoveTaskButton.Background = System.Windows.Media.Brushes.Gray;
            }
        }

        // ========== ПЕРЕМЕЩЕНИЕ ЗАДАЧИ (ГЛАВНЫЙ МЕТОД) ==========
        private void MoveTask_Click(object sender, RoutedEventArgs e)
        {
            if (TaskSelector.SelectedItem is not UserTask selectedTask)
            {
                MessageBox.Show("Выберите задачу для перемещения", "Внимание");
                return;
            }

            if (ColumnSelector.SelectedItem is not ComboBoxItem selectedColumn)
            {
                MessageBox.Show("Выберите колонку для перемещения", "Внимание");
                return;
            }

            TaskState newStatus = selectedColumn.Content.ToString() switch
            {
                "To Do" => TaskState.ToDo,
                "In Progress" => TaskState.InProgress,
                "Done" => TaskState.Done,
                _ => selectedTask.Status
            };

            if (selectedTask.Status == newStatus)
            {
                MessageBox.Show($"Задача уже находится в колонке {selectedColumn.Content}", "Внимание");
                return;
            }

            try
            {
                // 1. Обновляем статус в БД
                _dbService.UpdateTaskStatus(selectedTask.Id, newStatus);

                // 2. Обновляем текущую задачу в памяти
                selectedTask.Status = newStatus;

                // 3. Обновляем Канбан-доску (перерисовываем списки)
                RefreshKanban();

                // 4. Обновляем статистику
                UpdateStatistics();

                // 5. Обновляем выпадающий список задач
                RefreshTaskSelector();

                // 6. Показываем подтверждение
                MessageBox.Show($"Задача \"{selectedTask.Name}\" перемещена в {selectedColumn.Content}", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка");
            }
        }

        // ========== СОЗДАНИЕ ЗАДАЧ ==========
        private void CreateTask_Click(object sender, RoutedEventArgs e)
        {
            if (_currentSprint == null)
            {
                MessageBox.Show("Сначала создайте или выберите спринт");
                return;
            }

            string name = NewTaskNameBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(name) || name == "Название задачи")
            {
                MessageBox.Show("Введите название задачи");
                return;
            }

            if (NewTaskDeadlinePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату дедлайна");
                return;
            }

            TaskPriority priority = (NewTaskPriorityBox.SelectedItem as ComboBoxItem)?.Content.ToString() switch
            {
                "Низкий" => TaskPriority.Низкий,
                "Высокий" => TaskPriority.Высокий,
                _ => TaskPriority.Средний
            };

            try
            {
                _dbService.CreateTask(name, priority, NewTaskDeadlinePicker.SelectedDate.Value, _currentSprint.Id);
                NewTaskNameBox.Text = "Название задачи";
                RefreshKanban();
                UpdateStatistics();
                RefreshTaskSelector();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        // ========== СОЗДАНИЕ СПРИНТОВ ==========
        private void CreateSprint_Click(object sender, RoutedEventArgs e)
        {
            string name = NewSprintNameBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Введите название спринта");
                return;
            }

            if (NewSprintStartPicker.SelectedDate == null || NewSprintEndPicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите даты спринта");
                return;
            }

            if (NewSprintEndPicker.SelectedDate.Value <= NewSprintStartPicker.SelectedDate.Value)
            {
                MessageBox.Show("Дата окончания должна быть позже даты начала");
                return;
            }

            try
            {
                _dbService.CreateSprint(name, NewSprintStartPicker.SelectedDate.Value, NewSprintEndPicker.SelectedDate.Value);
                NewSprintNameBox.Text = "Спринт 1";
                LoadData();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}