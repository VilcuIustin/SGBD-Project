using Microsoft.AspNetCore.Mvc;
using SGBD_Project.Dtos;
using SGBD_Project.Models;

namespace SGBD_Project.Services
{
    public class QueriesService
    {

        private SGBDContext _context { get; }
        private IConfiguration _configuration { get; }
        private readonly WorkspaceService _workspace;
        public QueriesService(SGBDContext context, IConfiguration configuration, WorkspaceService workspace)
        {
            _context = context;
            _configuration = configuration;
            _workspace = workspace;
        }

        public async Task<bool> SaveQuery(QueriesDto query, Guid userId)
        {
            var userWorkspace = await _workspace.IsMyWorkspace(userId, query.WorkspaceId);
            if (userWorkspace == null)
            {
                throw new ApplicationException("Workspace does not exist");
            }

            var newQuery = new Script
            {
                Id = Guid.NewGuid(),
                DateCreated = DateTime.UtcNow,
                Name = query.Name,
                Query = query.Query,
                WorkspaceId = query.WorkspaceId,
            };

            await _context.AddAsync(newQuery);
            await _context.SaveChangesAsync();
            return true;
        }

        [HttpGet]
        public async Task<ActionResult> GetQueries(FilterQueries filter)
        {
            throw new NotImplementedException();
        }


    }
}
