using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using ChronoDesk.Application.Interfaces;
using ChronoDesk.Domain.Entities;

namespace ChronoDesk.UI.ViewModels;

public class ProjectListViewModel : ViewModelBase
{
    private readonly IProjectService _projectService;

    private ObservableCollection<Project> _projects = new();
    public ObservableCollection<Project> Projects
    {
        get => _projects;
        set => SetField(ref _projects, value);
    }

    private string _newProjectName = string.Empty;
    public string NewProjectName
    {
        get => _newProjectName;
        set
        {
            SetField(ref _newProjectName, value);
            ((RelayCommand)CreateProjectCommand).RaiseCanExecuteChanged();
        }
    }

    private string _newProjectDescription = string.Empty;
    public string NewProjectDescription
    {
        get => _newProjectDescription;
        set => SetField(ref _newProjectDescription, value);
    }

    public ICommand CreateProjectCommand { get; }
    public ICommand ArchiveProjectCommand { get; }

    public ProjectListViewModel(IProjectService projectService)
    {
        _projectService = projectService;
        CreateProjectCommand = new RelayCommand(async _ => await CreateProjectAsync(), _ => !string.IsNullOrWhiteSpace(NewProjectName));
        ArchiveProjectCommand = new RelayCommand(async p => await ArchiveProjectAsync(p));

        LoadProjectsAsync();
    }

    private async void LoadProjectsAsync()
    {
        var list = await _projectService.GetAllProjectsAsync();
        Projects = new ObservableCollection<Project>(list);
    }

    private async Task CreateProjectAsync()
    {
        await _projectService.CreateProjectAsync(NewProjectName, NewProjectDescription);
        NewProjectName = string.Empty;
        NewProjectDescription = string.Empty;
        LoadProjectsAsync();
    }

    private async Task ArchiveProjectAsync(object? parameter)
    {
        if (parameter is Project p)
        {
            await _projectService.ArchiveProjectAsync(p.Id);
            LoadProjectsAsync();
        }
    }
}
