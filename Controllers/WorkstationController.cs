using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SGBD_Project.Dtos;
using SGBD_Project.Services;

namespace SGBD_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WorkstationController : ControllerBase
    {
        private WorkspaceService _workspace { get; set; }
        public WorkstationController(WorkspaceService workspaceService)
        {
            _workspace = workspaceService;
        }


        [HttpPost]
        public async Task<Response<bool>> CreateWorkspace(WorkspaceCreateDto workspace)
        {
            var response = await _workspace.CreateWorkspaceAsync(workspace, new Guid(User.Claims.FirstOrDefault(c => c.Type == "id").Value));
            
            if (response.Status != 200)
            {
                Response.StatusCode = response.Status;
            }

            return response;
        }

        [HttpGet]
        public async Task<Response<List<WorkspaceDto>>> GetMyWorkspaceAsync()
        {
            var response = await _workspace.GetMyWorkspacesAsync(new Guid(User.Claims.FirstOrDefault(c => c.Type == "id").Value));

            if (response.Status != 200)
            {
                Response.StatusCode = response.Status;
            }

            return response;
        }

        [HttpPost("run")]
        public async Task<IActionResult> RunSqlAsync(SqlDto sqlDto)
        {
            try
            {
                var response = await _workspace.ExecuteSql(sqlDto, new Guid(User.Claims.FirstOrDefault(c => c.Type == "id").Value));

                return Ok(response);
            }
            catch (Exception ex)
            {

                return new ObjectResult("Something went wrong");
            }
         
        }


    }
}
