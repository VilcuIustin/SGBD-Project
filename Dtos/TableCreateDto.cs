using System.ComponentModel.DataAnnotations;

namespace SGBD_Project.Dtos
{
    public class TableCreateDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public Guid Id { get; set; }
        public List<ColumnCreateDto> Columns { get; set; }

    }
}
