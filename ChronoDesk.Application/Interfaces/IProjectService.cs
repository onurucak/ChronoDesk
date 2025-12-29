using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChronoDesk.Domain.Entities;

namespace ChronoDesk.Application.Interfaces;

public interface IProjectService
{
    Task<IEnumerable<Project>> GetAllProjectsAsync();
    Task<Project?> GetProjectByIdAsync(Guid id);
    Task<Project> CreateProjectAsync(string name, string description);
    Task UpdateProjectAsync(Project project);
    Task ArchiveProjectAsync(Guid id);
}
