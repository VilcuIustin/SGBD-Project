using SGBD_Project.Models;

namespace SGBD_Project.Dtos
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public List<WorkspaceDto> Workspaces { get; set; }
        public string Token { get; set; }
    }
}
