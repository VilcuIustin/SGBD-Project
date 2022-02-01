using System.ComponentModel.DataAnnotations;

namespace SGBD_Project.Dtos
{
    public class WorkspaceCreateDto
    {
        [Required]
        [MinLength(1)]
        public string Name { get; set; }
    }
}
