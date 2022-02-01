using System.ComponentModel.DataAnnotations;

namespace SGBD_Project.Dtos
{
    public class SqlDto
    {
        [Required]
        public string Sql { get; set; }
        [Required]
        public Guid WorkspaceId { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int RowNumbers { get; set; } = 10;

    }
}
