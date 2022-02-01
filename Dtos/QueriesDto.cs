using System.ComponentModel.DataAnnotations;

namespace SGBD_Project.Dtos
{
    public class QueriesDto
    {
        [Required]
        public Guid WorkspaceId { get; set; }
        [Required]
        public string Query { get; set; }
        [Required]
        [MaxLength(70)]
        public string Name { get; set; }
        
    }
}
