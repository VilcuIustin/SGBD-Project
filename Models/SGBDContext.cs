using Microsoft.EntityFrameworkCore;

namespace SGBD_Project.Models
{
    public class SGBDContext : DbContext
    {
        public SGBDContext(DbContextOptions<SGBDContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Workspace> Workspaces { get; set; }
        public DbSet<Script> Scripts { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SGBDContext).Assembly);
        }

    }
}
