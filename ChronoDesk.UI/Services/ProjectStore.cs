using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ChronoDesk.Application.Interfaces;
using ChronoDesk.Domain.Entities;

namespace ChronoDesk.UI.Services;

public class ProjectStore
{
    private readonly IProjectService _projectService;

    public ObservableCollection<Project> Projects { get; } = new();

    public ProjectStore(IProjectService projectService)
    {
        _projectService = projectService;
    }

    public async Task LoadProjectsAsync()
    {
        var list = await _projectService.GetAllProjectsAsync();
        Projects.Clear();
        foreach (var p in list.Where(p => !p.IsArchived))
        {
            Projects.Add(p);
        }
    }

    public void Add(Project project)
    {
        if (!project.IsArchived)
        {
            Projects.Add(project);
        }
    }

    public void Update(Project project)
    {
        var existing = Projects.FirstOrDefault(p => p.Id == project.Id);
        if (existing != null)
        {
            if (project.IsArchived)
            {
                Projects.Remove(existing);
            }
            else
            {
                existing.Name = project.Name;
                existing.Description = project.Description;
            }
        }
        else if (!project.IsArchived)
        {
            Projects.Add(project);
        }
    }
}
