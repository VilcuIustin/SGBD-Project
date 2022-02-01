using Dapper;
using Microsoft.Data.SqlClient;
using SGBD_Project.Dtos;
using SGBD_Project.Models;

namespace SGBD_Project.Services
{
    public class WorkspaceService
    {

        private SGBDContext Context { get; }
        private IConfiguration _configuration { get; }
        public WorkspaceService(SGBDContext context, IConfiguration configuration)
        {
            Context = context;
            _configuration = configuration;
        }

        public async Task<Response<bool>> CreateWorkspaceAsync(WorkspaceCreateDto workspace, Guid userId)
        {
            var userDatabases = new Dictionary<string, Guid>();
            using (var conn = new SqlConnection((string)_configuration.GetValue(typeof(string), "ConnectionStrings")))
            {
                //retrive all workspaces from user or just the userId to check that user exists
                userDatabases = (await conn.QueryAsync<(string, Guid)>(
@"SELECT ISNULL(W.Name, '') Name, U.Id 
FROM USERS U LEFT JOIN WORKSPACES W ON U.Id = W.UserId                     
WHERE U.Id = @userId AND U.DeletedAt IS NULL;",
                   new { userId = userId })).ToDictionary(w => w.Item1, w => w.Item2);
            };

            //Now checking if there is an user with this id

            var userExists = userDatabases.TryGetValue("", out _);

            if (userExists)
                return new Response<bool>("User does not exist.", 400);

            var newDatabaseName = $"{userId}_{workspace.Name}";

            var databaseExists = userDatabases.TryGetValue(workspace.Name, out _);

            if (databaseExists)
                return new Response<bool>("Database with this name already exists.", 400);



            using (var conn = new SqlConnection((string)_configuration.GetValue(typeof(string), "ServerDbString")))
            {
                // sql server does not accept string as parameter for create database and Quotename is used against
                // sql injection []
                var rez = await conn.ExecuteAsync(
                    @"
DECLARE @sql nvarchar(max) = 'Create database ' +  QUOTENAME(@databasename);
exec (@sql);",
                    new { databasename = newDatabaseName });
                var a = 5;
                if (rez == -1)
                {
                    Context.Workspaces.Add(new Workspace
                    {
                        Name = workspace.Name,
                        Id = new Guid(),
                        UserId = userId,
                    });
                    await Context.SaveChangesAsync();
                }
            };

            return new Response<bool>(true);
        }


        public async Task<List<object>> ExecuteSql(SqlDto sqlDto, Guid userId)
        {
            var userWorkspace = await IsMyWorkspace(userId, sqlDto.WorkspaceId);
            if (userWorkspace == null)
            {
                throw new ApplicationException("Workspace does not exist");
            }
            
            using (var conn = new SqlConnection(string.Format(_configuration.GetValue(typeof(string), "ConnectionStringsForClients").ToString(), userWorkspace)))
            {
                var result = new List<object>();
                // maybe will add the set ROWCOUNT number_of_rows
                try
                {
                    var a = (await conn.QueryMultipleAsync(
$@"EXECUTE sp_executesql @sql",
                new { sql = sqlDto.Sql }));
                    while (!a.IsConsumed)
                    {
                        result.Add(a.Read());

                    }


                    return result;
                }
                catch(SqlException ex)
                {
                    return new List<object> { new object[] { new { error = ex.Message } } };
                }
              
            };

        }

        public async Task<Response<List<WorkspaceDto>>> GetMyWorkspacesAsync(Guid userId)
        {
            var databases = new List<WorkspaceDto>();
            using (var conn = new SqlConnection((string)_configuration.GetValue(typeof(string), "ConnectionStrings")))
            {
                //retrive all workspaces from user or just the userId to check that user exists
                databases = (await conn.QueryAsync<WorkspaceDto>(
@"SELECT W.Name, W.Id 
FROM USERS U INNER JOIN WORKSPACES W ON U.Id = W.UserId                     
WHERE U.Id = @userId AND U.DeletedAt IS NULL;",
                   new { userId = userId })).ToList();
            };

            return new Response<List<WorkspaceDto>>(databases);
        }


        public async Task<string> IsMyWorkspace(Guid userId, Guid workspaceId)
        {
            using (var conn = new SqlConnection((string)_configuration.GetValue(typeof(string), "ConnectionStrings")))
            {
                
                  return (await conn.QueryAsync<string>(
@"SELECT cast(W.UserId as varchar(36)) + '_' +W.Name 
FROM USERS U INNER JOIN WORKSPACES W ON U.Id = W.UserId                     
WHERE W.UserId = @userId AND W.id = @workspaceId AND U.DeletedAt IS NULL;",
                   new { userId = userId, workspaceId  = workspaceId })).FirstOrDefault();
            };
        }

    }
}
