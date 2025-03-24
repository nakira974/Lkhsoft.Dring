#region

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lkhsoft.Dring.Client.Models;
using Lkhsoft.Dring.Client.Services.Multimedia;

#endregion

namespace Lkhsoft.Dring.Client.PageModels;

public partial class MainPageModel : ObservableObject, IProjectTaskPageModel
{
    private bool _isNavigatedTo;
    private bool _dataLoaded;
    private readonly ProjectRepository _projectRepository;
    private readonly TaskRepository _taskRepository;
    private readonly CategoryRepository _categoryRepository;
    private readonly ModalErrorHandler _errorHandler;
    private readonly SeedDataService _seedDataService;
    private readonly INativeLibrariesImports _nativeLibrariesImports;


    [ObservableProperty] private List<CategoryChartData> _todoCategoryData = [];

    [ObservableProperty] private List<Brush> _todoCategoryColors = [];

    [ObservableProperty] private List<ProjectTask> _tasks = [];

    [ObservableProperty] private List<Project> _projects = [];

    [ObservableProperty] private bool _isBusy;

    [ObservableProperty] private bool _isRefreshing;

    [ObservableProperty] private string _today = DateTime.Now.ToString("dddd, MMM d");

    public bool HasCompletedTasks
        => Tasks?.Any(t => t.IsCompleted) ?? false;

    public MainPageModel(SeedDataService seedDataService, ProjectRepository projectRepository,
        TaskRepository taskRepository, CategoryRepository categoryRepository, ModalErrorHandler errorHandler,
        INativeLibrariesImports nativeLibrariesImports)
    {
        _nativeLibrariesImports = nativeLibrariesImports;
        _projectRepository = projectRepository;
        _taskRepository = taskRepository;
        _categoryRepository = categoryRepository;
        _errorHandler = errorHandler;
        _seedDataService = seedDataService;
    }

    private async Task LoadData()
    {
        try
        {
            LoadNativeLibraries();
            IsBusy = true;

            Projects = await _projectRepository.ListAsync();

            var chartData = new List<CategoryChartData>();
            var chartColors = new List<Brush>();

            var categories = await _categoryRepository.ListAsync();
            foreach (var category in categories)
            {
                chartColors.Add(category.ColorBrush);

                var ps = Projects.Where(p => p.CategoryID == category.ID).ToList();
                var tasksCount = ps.SelectMany(p => p.Tasks).Count();

                chartData.Add(new CategoryChartData(category.Title, tasksCount));
            }

            TodoCategoryData = chartData;
            TodoCategoryColors = chartColors;

            Tasks = await _taskRepository.ListAsync();
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(HasCompletedTasks));
        }
    }

    private async Task InitData(SeedDataService seedDataService)
    {
        var isSeeded = Preferences.Default.ContainsKey("is_seeded");

        if (!isSeeded) await seedDataService.LoadSeedDataAsync();

        Preferences.Default.Set("is_seeded", true);
        await Refresh();
    }

    // Charger les périphériques audio
    private void LoadNativeLibraries()
    {
        _nativeLibrariesImports.Load(NativeLibraries.MultimediaStream);
    }

    [RelayCommand]
    private async Task Refresh()
    {
        try
        {
            IsRefreshing = true;
            await LoadData();
        }
        catch (Exception e)
        {
            _errorHandler.HandleError(e);
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private void NavigatedTo()
    {
        _isNavigatedTo = true;
    }

    [RelayCommand]
    private void NavigatedFrom()
    {
        _isNavigatedTo = false;
    }

    [RelayCommand]
    private async Task Appearing()
    {
        if (!_dataLoaded)
        {
            LoadNativeLibraries();
            await InitData(_seedDataService);
            _dataLoaded = true;
            await Refresh();
        }
        // This means we are being navigated to
        else if (!_isNavigatedTo)
        {
            await Refresh();
        }
    }

    [RelayCommand]
    private Task TaskCompleted(ProjectTask task)
    {
        OnPropertyChanged(nameof(HasCompletedTasks));
        return _taskRepository.SaveItemAsync(task);
    }

    [RelayCommand]
    private Task AddTask()
    {
        return Shell.Current.GoToAsync($"task");
    }

    [RelayCommand]
    private Task NavigateToProject(Project project)
    {
        return Shell.Current.GoToAsync($"project?id={project.ID}");
    }

    [RelayCommand]
    private Task NavigateToTask(ProjectTask task)
    {
        return Shell.Current.GoToAsync($"task?id={task.ID}");
    }

    [RelayCommand]
    private async Task CleanTasks()
    {
        var completedTasks = Tasks.Where(t => t.IsCompleted).ToList();
        foreach (var task in completedTasks)
        {
            await _taskRepository.DeleteItemAsync(task);
            Tasks.Remove(task);
        }

        OnPropertyChanged(nameof(HasCompletedTasks));
        Tasks = new List<ProjectTask>(Tasks);
        await AppShell.DisplayToastAsync("All cleaned up!");
    }
}