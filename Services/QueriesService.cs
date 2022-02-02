using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SGBD_Project.Dtos;
using SGBD_Project.Models;
using SGBD_Project.Queries;
using System.Dynamic;
using static Dapper.SqlMapper;
using static Newtonsoft.Json.JsonConvert;

namespace SGBD_Project.Services
{
    public class QueriesService
    {

        private SGBDContext _context { get; }
        private IConfiguration _configuration { get; }
        private readonly WorkspaceService _workspace;
        public QueriesService(SGBDContext context, IConfiguration configuration, WorkspaceService workspace)
        {
            _context = context;
            _configuration = configuration;
            _workspace = workspace;
        }

        public async Task<bool> SaveQuery(QueriesDto query, Guid userId)
        {
            var userWorkspace = await _workspace.IsMyWorkspace(userId, query.WorkspaceId);
            if (userWorkspace == null)
            {
                throw new ApplicationException("Workspace does not exist");
            }

            var newQuery = new Script
            {
                Id = Guid.NewGuid(),
                DateCreated = DateTime.UtcNow,
                Name = query.Name,
                Query = query.Query,
                WorkspaceId = query.WorkspaceId,
            };

            await _context.AddAsync(newQuery);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CreateTable(TableCreateDto dto, Guid userId)
        {
            var userWorkspace = await _workspace.IsMyWorkspace(userId, dto.Id);
            if (userWorkspace == null)
            {
                throw new ApplicationException("Workspace does not exist");
            }
            
            var sql = @"DECLARE @sql nvarchar(max) = 'CREATE TABLE ' +QUOTENAME(@TABLENAME)+ ' ( ";
            var parameters = new DynamicParameters();
            parameters.Add($"@TABLENAME", dto.Name);
            var columnsPk = new List<string>();

            foreach (var column in dto.Columns)
            {
                var nullable = column.IsNullable ? " NULL " : " NOT NULL";
                var primaryKey = column.IsPrimary ? " PRIMARY KEY " : string.Empty;
                var columnNameVar = "column_name_" + Guid.NewGuid().ToString()
                    .Remove(23, 1)
                    .Remove(18, 1)
                    .Remove(13, 1)
                    .Remove(8, 1);

                sql = sql + $@" '+QUOTENAME(@{columnNameVar})+' {column.ColumnType} {nullable},";
                parameters.Add($"@{columnNameVar}", column.ColumnName);
                if (column.IsPrimary)
                    columnsPk.Add($"{columnNameVar}");
            }
            sql = sql.Remove(sql.Length -1) + " )'; EXEC (@sql); ";
            if(columnsPk.Count > 0)
            {
                sql = sql + "SET @sql = 'ALTER TABLE ' +QUOTENAME(@TABLENAME)+' ADD PRIMARY KEY (";
                foreach(var columns in columnsPk)
                {
                    sql = sql + $"' + QUOTENAME(@{columns}) + ',";
                }
                sql = sql.Remove(sql.Length - 1) + ")'; EXEC (@sql);";

            }
            Console.WriteLine(sql);
           
            using (var conn = new SqlConnection(string.Format(_configuration.GetValue(typeof(string), "ConnectionStringsForClients").ToString(), userWorkspace)))
            {
                var result = await conn.QueryAsync(sql, parameters);
                var a = 5;

            }
    
            return true;
        }

        public async Task<object> RepairAll(WorkspaceIdDto dto, Guid userId, int repairType)
        {
            var userWorkspace = await _workspace.IsMyWorkspace(userId, dto.Id);
            if (userWorkspace == null)
            {
                throw new ApplicationException("Workspace does not exist");
            }

            using (var conn = new SqlConnection(string.Format(_configuration.GetValue(typeof(string), "ConnectionStringsForClients").ToString(), userWorkspace)))
            {
                var result = new List<object>();

                try
                {
                    await conn.QueryAsync(RepairQueries.DropProcs);
                    await conn.QueryAsync(RepairQueries.CreateProc_CREATE_TMP_TABLE_RESULTS);
                    await conn.QueryAsync(RepairQueries.CreateProc_CREATE_SEQUENCE_NUMERIC);
                    await conn.QueryAsync(RepairQueries.CreateProc_Create_Column);
                    await conn.QueryAsync(RepairQueries.CreateProc_TABLES_WITHOUT_PK);
                    await conn.QueryAsync(RepairQueries.CreateProc_TABLES_WITH_PK_FK);
                    await conn.QueryAsync(RepairQueries.CreateProc_TABLES_WITH_PK);
                    await conn.QueryAsync(RepairQueries.CreateProc_NORMALIZARE_PK);

                    GridReader a;

                    if (repairType == 1)
                    {
                        a = await conn.QueryMultipleAsync(RepairQueries.Exec_TABLES_WITHOUT_PK);
                    }
                    else if (repairType == 2)
                    {
                        a = await conn.QueryMultipleAsync(RepairQueries.Exec_TABLES_PK);
                    }
                    else
                    {
                        a = await conn.QueryMultipleAsync("NORMALIZARE_PK");
                    }
                   
                    while (!a.IsConsumed)
                    {
                        result.Add(a.Read());
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    return new List<object> { new object[] { new { error = ex.Message } } };
                }

            };

        }
    }
}
