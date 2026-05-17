using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TaskTracker.Models;
using TaskTracker.Services;
using TaskTracker.ViewModels;

namespace TaskTracker.Views
{
    public partial class MainWindow : Window
    {
        private DatabaseService _dbService;
        private MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _dbService = new DatabaseService();

            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            _viewModel.DataChanged += (s, e) => UpdateStatistics();

            NewTaskDeadlinePicker.SelectedDate = DateTime.Now;
            NewSprintStartPicker.SelectedDate = DateTime.Now;
            NewSprintEndPicker.SelectedDate = DateTime.Now.AddDays(14);

            LoadData();
        }

        private void LoadData()
        {
            _viewModel.LoadData();
            if (_viewModel.Sprints.Any() && _viewModel.SelectedSprint == null)
                _viewModel.SelectedSprint = _viewModel.Sprints.First();
            UpdateStatistics();
        }

        private void UpdateStatistics()
        {
            Dispatcher.Invoke(() =>
            {
                if (_viewModel?.SelectedSprint != null)
                {
                    var stats = _dbService.GetStatistics(_viewModel.SelectedSprint.Id);
                    var tasks = _dbService.GetTasksBySprint(_viewModel.SelectedSprint.Id);

                    StatsCompleted.Text = $"✅ Выполнено: {stats.Completed}";
                    StatsInProgress.Text = $"⚙ В работе: {stats.InProgress}";
                    StatsOverdue.Text = $"⚠ Просрочено: {stats.Overdue}";
                    StatsTotal.Text = $"📊 Всего задач: {tasks.Count}";
                }
                else
                {
                    StatsCompleted.Text = "✅ Выполнено: 0";
                    StatsInProgress.Text = "⚙ В работе: 0";
                    StatsOverdue.Text = "⚠ Просрочено: 0";
                    StatsTotal.Text = "📊 Всего задач: 0";
                }
            });
        }

        private void RefreshStats_Click(object sender, RoutedEventArgs e)
        {
            _viewModel?.ForceUpdate();
            UpdateStatistics();
            RefreshStatsButton.Content = "✅ Обновлено!";
            System.Threading.Tasks.Task.Delay(1000).ContinueWith(_ =>
                Dispatcher.Invoke(() => RefreshStatsButton.Content = "🔄 Обновить статистику"));
        }

        private void Sprint_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateStatistics();

        private void MoveToInProgress_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is UserTask task)
            {
                _viewModel.MoveTaskToStatus(task, TaskState.InProgress);
                UpdateStatistics();
            }
        }

        private void MoveToToDo_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is UserTask task)
            {
                _viewModel.MoveTaskToStatus(task, TaskState.ToDo);
                UpdateStatistics();
            }
        }

        private void MoveToDone_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is UserTask task)
            {
                _viewModel.MoveTaskToStatus(task, TaskState.Done);
                UpdateStatistics();
            }
        }

        private void MoveToInProgressFromDone_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is UserTask task)
            {
                _viewModel.MoveTaskToStatus(task, TaskState.InProgress);
                UpdateStatistics();
            }
        }

        private void CreateTask_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel?.SelectedSprint == null)
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
                _dbService.CreateTask(name, priority, NewTaskDeadlinePicker.SelectedDate.Value, _viewModel.SelectedSprint.Id);
                NewTaskNameBox.Text = "Название задачи";
                _viewModel.ForceUpdate();
                UpdateStatistics();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

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
                _viewModel.LoadData();
                if (_viewModel.Sprints.Any()) _viewModel.SelectedSprint = _viewModel.Sprints.Last();
                _viewModel.ForceUpdate();
                UpdateStatistics();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}