using System.ComponentModel.DataAnnotations;

namespace SGBD_Project.Dtos
{
    public class ColumnCreateDto
    {
        [Required]
        public string ColumnName { get; set; }
        [Required]
        public string ColumnType { get; set; }
        [Required]
        public bool IsPrimary { get; set; }
        [Required]
        public bool IsNullable { get; set; }
    }
}
