using ChronoDesk.Domain.Entities;
using ChronoDesk.Domain.Interfaces;

namespace ChronoDesk.Infrastructure.Persistence.Repositories;

public class ProjectRepository : Repository<Project>, IProjectRepository
{
    public ProjectRepository(ChronoDbContext context) : base(context)
    {
    }
}
