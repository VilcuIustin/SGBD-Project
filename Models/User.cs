using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SGBD_Project.Models
{
    public class User : BaseEntity
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public ICollection<Workspace> Workspaces { get; set; }

    }
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {

        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasIndex(e => e.Email).IsUnique();
        }
    }
}
