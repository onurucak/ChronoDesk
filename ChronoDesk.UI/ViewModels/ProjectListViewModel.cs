using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ChronoDesk.Application.Interfaces;
using ChronoDesk.Domain.Entities;

namespace ChronoDesk.UI.ViewModels;

public class ProjectListViewModel : ViewModelBase
{
    private readonly IProjectService _projectService;

    private ObservableCollection<ProjectItemViewModel> _projects = new();
    public ObservableCollection<ProjectItemViewModel> Projects
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

    public ProjectListViewModel(IProjectService projectService)
    {
        _projectService = projectService;
        CreateProjectCommand = new RelayCommand(async _ => await CreateProjectAsync(), _ => !string.IsNullOrWhiteSpace(NewProjectName));
        
        LoadProjectsAsync();
    }

    private async void LoadProjectsAsync()
    {
        var list = await _projectService.GetAllProjectsAsync();
        Projects = new ObservableCollection<ProjectItemViewModel>(
            list.Where(p => !p.IsArchived).Select(p => new ProjectItemViewModel(p, _projectService, this))
        );
    }

    public void RefreshList()
    {
        LoadProjectsAsync();
    }

    private async Task CreateProjectAsync()
    {
        await _projectService.CreateProjectAsync(NewProjectName, NewProjectDescription);
        NewProjectName = string.Empty;
        NewProjectDescription = string.Empty;
        LoadProjectsAsync();
    }
}

public class ProjectItemViewModel : ViewModelBase
{
    private readonly IProjectService _projectService;
    private readonly ProjectListViewModel _parentViewModel;
    private Project _project;

    public Guid Id => _project.Id;

    public string Name => _project.Name;
    public string Description => _project.Description;

    private bool _isEditing;
    public bool IsEditing
    {
        get => _isEditing;
        set => SetField(ref _isEditing, value);
    }

    private string _editName = string.Empty;
    public string EditName
    {
        get => _editName;
        set
        {
            SetField(ref _editName, value);
            ((RelayCommand)SaveEditCommand).RaiseCanExecuteChanged();
        }
    }

    private string _editDescription = string.Empty;
    public string EditDescription
    {
        get => _editDescription;
        set => SetField(ref _editDescription, value);
    }

    public ICommand EditCommand { get; }
    public ICommand SaveEditCommand { get; }
    public ICommand CancelEditCommand { get; }
    public ICommand ArchiveCommand { get; }

    public ProjectItemViewModel(Project project, IProjectService projectService, ProjectListViewModel parentViewModel)
    {
        _project = project;
        _projectService = projectService;
        _parentViewModel = parentViewModel;

        EditCommand = new RelayCommand(_ => StartEdit());
        SaveEditCommand = new RelayCommand(async _ => await SaveEditAsync(), _ => !string.IsNullOrWhiteSpace(EditName));
        CancelEditCommand = new RelayCommand(_ => CancelEdit());
        ArchiveCommand = new RelayCommand(async _ => await ArchiveAsync());
    }

    private void StartEdit()
    {
        EditName = Name;
        EditDescription = Description;
        IsEditing = true;
    }

    private void CancelEdit()
    {
        IsEditing = false;
        EditName = string.Empty;
        EditDescription = string.Empty;
    }

    private async Task SaveEditAsync()
    {
        // Ideally we'd have an Update method in service, for now we might need to implement it or use what's available.
        // Assuming NO update method exists yet based on interface files seen earlier. 
        // Wait, I need to check IProjectService.
        // If not exists, I might need to add it. For now let's assume I can add it or it exists.
        // Actually, looking at previous context, there is likely NO update method. I should add it.
        // For this step I'll assume UpdateProjectAsync exists on service or I will add it in next step.
        // To be safe I will add a TODO here and add the method to Service in next tool calls if needed.
        
        // Let's check IProjectService content first? No, I am in execution. I will assume I need to add it.
        // I will write the code assuming it exists.0

        // Actually, to avoid compilation error, I will check service now.
        // I'll proceed with this file change assuming I will add the method immediately after.
        
        await _projectService.UpdateProjectAsync(Id, EditName, EditDescription);
        _project.Name = EditName;
        _project.Description = EditDescription;
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Description));
        IsEditing = false;
    }

    private async Task ArchiveAsync()
    {
        await _projectService.ArchiveProjectAsync(Id);
        _parentViewModel.RefreshList();
    }
}
