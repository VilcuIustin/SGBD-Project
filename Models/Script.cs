using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SGBD_Project.Models
{
    public class Script : BaseEntity
    {
        public string Name { get; set; }
        public string Query { get; set; }
        public DateTime DateCreated { get; set; }
        public Guid WorkspaceId { get; set; }
        public virtual Workspace Workspace { get; set; }

    }

    public class ScriptConfiguration : IEntityTypeConfiguration<Script>
    {

        public void Configure(EntityTypeBuilder<Script> builder)
        {
            builder.HasIndex(e => e.DateCreated);
        }
    }
}
