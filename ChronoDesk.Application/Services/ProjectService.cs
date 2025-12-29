using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChronoDesk.Application.Interfaces;
using ChronoDesk.Domain.Entities;
using ChronoDesk.Domain.Interfaces;

namespace ChronoDesk.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;

    public ProjectService(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<IEnumerable<Project>> GetAllProjectsAsync()
    {
        return await _projectRepository.GetAllAsync();
    }

    public async Task<Project?> GetProjectByIdAsync(Guid id)
    {
        return await _projectRepository.GetByIdAsync(id);
    }

    public async Task<Project> CreateProjectAsync(string name, string description)
    {
        var project = new Project
        {
            Name = name,
            Description = description,
            IsArchived = false
        };
        await _projectRepository.AddAsync(project);
        return project;
    }

    public async Task UpdateProjectAsync(Guid id, string name, string description)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project != null)
        {
            project.Name = name;
            project.Description = description;
            project.UpdatedAt = DateTime.UtcNow;
            await _projectRepository.UpdateAsync(project);
        }
    }

    public async Task UpdateProjectAsync(Project project)
    {
        project.UpdatedAt = DateTime.UtcNow;
        await _projectRepository.UpdateAsync(project);
    }

    public async Task ArchiveProjectAsync(Guid id)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project != null)
        {
            project.IsArchived = true;
            project.UpdatedAt = DateTime.UtcNow;
            await _projectRepository.UpdateAsync(project);
        }
    }
}
