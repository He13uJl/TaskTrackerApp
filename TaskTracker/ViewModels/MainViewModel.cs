using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using TaskTracker.Models;
using TaskTracker.Services;

namespace TaskTracker.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _dbService;
        private readonly Dispatcher _uiDispatcher;

        private ObservableCollection<Sprint> _sprints;
        private Sprint _selectedSprint;
        private ObservableCollection<UserTask> _todoTasks;
        private ObservableCollection<UserTask> _inProgressTasks;
        private ObservableCollection<UserTask> _doneTasks;

        public event Action DataChanged;

        public MainViewModel()
        {
            _dbService = new DatabaseService();
            _uiDispatcher = Dispatcher.CurrentDispatcher;
            _sprints = new ObservableCollection<Sprint>();
            _todoTasks = new ObservableCollection<UserTask>();
            _inProgressTasks = new ObservableCollection<UserTask>();
            _doneTasks = new ObservableCollection<UserTask>();
        }

        public void LoadData()
        {
            var sprints = _dbService.GetAllSprints();
            _uiDispatcher.Invoke(() => Sprints = new ObservableCollection<Sprint>(sprints));
            UpdateKanban();
        }

        public void UpdateKanban()
        {
            _uiDispatcher.Invoke(() =>
            {
                if (SelectedSprint == null)
                {
                    _todoTasks.Clear();
                    _inProgressTasks.Clear();
                    _doneTasks.Clear();
                    DataChanged?.Invoke();
                    return;
                }

                var tasks = _dbService.GetTasksBySprint(SelectedSprint.Id);

                UpdateCollection(_todoTasks, tasks.Where(t => t.Status == TaskState.ToDo));
                UpdateCollection(_inProgressTasks, tasks.Where(t => t.Status == TaskState.InProgress));
                UpdateCollection(_doneTasks, tasks.Where(t => t.Status == TaskState.Done));

                OnPropertyChanged(nameof(TodoTasks));
                OnPropertyChanged(nameof(InProgressTasks));
                OnPropertyChanged(nameof(DoneTasks));

                DataChanged?.Invoke();
            });
        }

        public void ForceUpdate()
        {
            UpdateKanban();
        }

        private void UpdateCollection(ObservableCollection<UserTask> collection, IEnumerable<UserTask> newItems)
        {
            collection.Clear();
            foreach (var item in newItems) collection.Add(item);
        }

        public ObservableCollection<Sprint> Sprints
        {
            get => _sprints;
            set { _sprints = value; OnPropertyChanged(); }
        }

        public Sprint SelectedSprint
        {
            get => _selectedSprint;
            set
            {
                _selectedSprint = value;
                OnPropertyChanged();
                ForceUpdate();
            }
        }

        public ObservableCollection<UserTask> TodoTasks
        {
            get => _todoTasks;
            set { _todoTasks = value; OnPropertyChanged(); }
        }

        public ObservableCollection<UserTask> InProgressTasks
        {
            get => _inProgressTasks;
            set { _inProgressTasks = value; OnPropertyChanged(); }
        }

        public ObservableCollection<UserTask> DoneTasks
        {
            get => _doneTasks;
            set { _doneTasks = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}