namespace SGBD_Project.Dtos
{
    public class WorkspaceDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string DatabaseName  => Id.ToString() + "_" + Name;
    }
}
