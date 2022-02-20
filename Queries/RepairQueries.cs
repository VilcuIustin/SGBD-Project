namespace SGBD_Project.Queries
{
    public static class RepairQueries
    {
		public static string DropProcs =>
			@"DROP PROC IF EXISTS NORMALIZARE_PK, Create_Column, TABLES_WITHOUT_PK, CREATE_TMP_TABLE_RESULTS, CREATE_SEQUENCE_NUMERIC, TABLES_WITH_PK_FK, TABLES_WITH_PK;
";
		public static string CreateProc_CREATE_TMP_TABLE_RESULTS =>
			@"
CREATE PROC CREATE_TMP_TABLE_RESULTS
AS
	DROP TABLE IF EXISTS ##results;
	CREATE TABLE ##results(message NVARCHAR(MAX));
";
		public static string CreateProc_CREATE_SEQUENCE_NUMERIC =>
			@"CREATE PROC CREATE_SEQUENCE_NUMERIC 
@tableName nvarchar(max),
@sequenceName nvarchar(max) OUTPUT
AS
	DECLARE @sql nvarchar(max);
	DECLARE @id nvarchar(36) = cast(NEWID() as  nvarchar(36));
	SET @id = STUFF(STUFF(STUFF(STUFF(@id, 9, 1, '' ), 13, 1, ''), 17, 1, ''), 21, 1, '');
	SET @sequenceName = @tableName +'_' + @id;
	SET @sql = N'CREATE SEQUENCE '+QUOTENAME(@sequenceName)+'
		AS BIGINT
		START WITH 1
		INCREMENT BY 1;'
	exec(@sql);
	insert into ##results (message) values('Created sequence '+@sequenceName +' for table '+ @tableName);
";
		public static string CreateProc_Create_Column =>
			@"CREATE PROC Create_Column
	@tableId int,
	@mode tinyint,						-- 0 = pk , 1 = column for pk, 2 = column for fk
	@columnName nvarchar(max) output
AS
	DECLARE @tableName sysname = OBJECT_NAME(@tableId);
	DECLARE @pkName nvarchar(38);
	DECLARE @sql nvarchar(max);
	DECLARE @sequenceName nvarchar(max);
	DECLARE @id nvarchar(36) = cast(NEWID() as  nvarchar(36));
	SET @id = STUFF(STUFF(STUFF(STUFF(@id, 9, 1, '' ), 13, 1, ''), 17, 1, ''), 21, 1, ''); --scoate unu sau mai multe caractere si il inlocuieste cu un alt string
	
	IF @mode = 0 BEGIN
		EXEC CREATE_SEQUENCE_NUMERIC @tableName = @tableName, @sequenceName = @sequenceName OUTPUT;
		SET @pkName = 'PK_ID_' + @id;
		SET @sql = 'ALTER TABLE ' + QUOTENAME(@tableName) + ' ADD ' + QUOTENAME(@pkName) + ' INT NOT NULL PRIMARY KEY DEFAULT (NEXT VALUE FOR '+QUOTENAME(@sequenceName)+')';

	END
	ELSE BEGIN
		IF @mode = 1 BEGIN
			EXEC CREATE_SEQUENCE_NUMERIC @tableName = @tableName, @sequenceName = @sequenceName OUTPUT;
			SET @pkName = 'PK_ID_' + @id;
			SET @sql = 'ALTER TABLE ' + @tableName + ' ADD ' + QUOTENAME(@pkName) + ' INT NOT NULL DEFAULT (NEXT VALUE FOR '+QUOTENAME(@sequenceName)+')';
			SET @columnName = @pkName;
			END

		ELSE BEGIN
			EXEC CREATE_SEQUENCE_NUMERIC @tableName = @tableName, @sequenceName = @sequenceName OUTPUT;
			SET @pkName = 'FK_ID_' + @id;
			SET @sql = 'ALTER TABLE ' + @tableName + ' ADD ' + QUOTENAME(@pkName) + ' INT ';
			SET @columnName = @pkName;
		END;
	END;
	
	EXEC (@sql);
	INSERT INTO ##results (message) VALUES('Created column for ' + @pkName + ' in table '+ @tableName);
";
		public static string CreateProc_TABLES_WITHOUT_PK =>
			@"
CREATE PROC TABLES_WITHOUT_PK
AS
DECLARE @tableName nvarchar(max);
DECLARE @tableId int;
DECLARE @tables Table(object_id int, COLUMN_NAME nvarchar(max))
DECLARE @columnName NVARCHAR;

INSERT INTO @tables select t.object_id, t.name 
from sys.tables t 
where not exists(select 1 from sys.indexes i where i.object_id = t.object_id and i.is_primary_key = 1)

	WHILE EXISTS(SELECT * FROM @tables)
	BEGIN
		SELECT TOP 1 @tableName = T.COLUMN_NAME, @tableId = T.object_id FROM @tables T;
	    EXECUTE Create_Column @mode = 0, @tableId = @tableId, @columnName = @columnName;
		INSERT INTO ##results (message) VALUES('Table without pk '+ @tableName);
		delete from @tables where COLUMN_NAME = @tableName;
	END
";
		public static string CreateProc_TABLES_WITH_PK_FK =>
			@"
CREATE PROC TABLES_WITH_PK_FK
AS
	DECLARE @refObj nvarchar(max);
	DECLARE @ParObj nvarchar(max);
	DECLARE @sql nvarchar(max);
	DECLARE @pkName nvarchar(max);
	DECLARE @fkName nvarchar(max);
	DECLARE @numberResults int;
	DECLARE @fkNameConstraint nvarchar(max);
	DECLARE @column_name_ref nvarchar(max);
	DECLARE @column_name_par nvarchar(max);
	DECLARE @newColumn nvarchar(max);

	DROP TABLE IF EXISTS #FkDetails;
	CREATE TABLE #FkDetails ( TYPE_REF sysname,
		referenced_object_id int,
		column_name_ref sysname,
		fk_name nvarchar(max),
		parent_object_id int,
		column_name_par sysname)
		--create index IX_FkDetails on #FkDetails (referenced_object_id);

	 INSERT INTO #FkDetails SELECT TY.name TYPE_REF, fkc.referenced_object_id, c.name column_name_ref,fk.name fk_name, fkc.parent_object_id, cc.name column_name_par

	 FROM SYS.foreign_keys fk JOIN SYS.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
	 JOIN SYS.columns c ON C.object_id = FKC.referenced_object_id and c.column_id = fkc.referenced_column_id
	 JOIN SYS.columns cc ON CC.object_id = FKC.parent_object_id and cc.column_id = fkc.parent_column_id
	 JOIN SYS.types ty ON TY.user_type_id = C.system_type_id
	 order by fkc.referenced_object_id ;
	 --select * from #FkDetails;
	while exists(select f.referenced_object_id from #FkDetails f)
	begin
		select TOP(1) @refObj = f.referenced_object_id, @fkNameConstraint = f.fk_name, @ParObj = f.parent_object_id from #FkDetails f;

		DROP TABLE IF EXISTS #det;
		select * into #det from #FkDetails f where referenced_object_id = @refObj;
		select @numberResults = count(*) from #det;
		
		if @numberResults = 1
		begin
			print @refObj
			if( (select coalesce((select TOP(1) 1 
			from #det 
			where TYPE_REF not in ('smallint', 'int', 'bigint', 'tinyint')), 0)) = 0)
			begin
				delete from #FkDetails where referenced_object_id = @refObj;
				CONTINUE;
			end;
			
		end;
		else
		begin
			print @refObj
				if((select count(*) from #det where TYPE_REF in ('smallint', 'int', 'bigint', 'tinyint')) <> @numberResults)
				begin
					delete from #FkDetails where referenced_object_id = @refObj;
					CONTINUE;
				end;
		end

		EXEC Create_Column @tableId = @refObj, @mode = 1, @columnName = @pkName OUTPUT;
		EXEC Create_Column @tableId = @ParObj, @mode = 2, @columnName = @fkName OUTPUT;

		SET @sql = 'UPDATE '+QUOTENAME(OBJECT_NAME(@ParObj))+ ' SET ' + QUOTENAME(@fkName) + ' = ' + QUOTENAME(@pkName) 
		+ ' FROM '+QUOTENAME(OBJECT_NAME(@ParObj))+ ' JOIN '+ QUOTENAME(OBJECT_NAME(@refObj)) + ' ON ';

		while exists (select * from #det)
		begin
			SELECT TOP(1) @column_name_ref = column_name_ref, @column_name_par = column_name_par FROM #det;
			SET @sql = @sql + QUOTENAME(OBJECT_NAME(@ParObj)) +'.'+ QUOTENAME(@column_name_par) 
			+ ' = ' + QUOTENAME(OBJECT_NAME(@refObj))+ '.' + QUOTENAME(@column_name_ref) + ' AND ';
			DELETE FROM #det WHERE column_name_ref = @column_name_ref and column_name_par = @column_name_par;
		end
		SET @sql = @sql + '1 = 1';
		
		print @sql;
		EXEC (@sql);
	
		SET @sql = 'ALTER TABLE '+ QUOTENAME(OBJECT_NAME(@ParObj)) + ' DROP CONSTRAINT '+ QUOTENAME(@fkNameConstraint);
		EXEC (@sql);

		SET @sql = 'ALTER TABLE ' + QUOTENAME(OBJECT_NAME(@refObj)) + ' DROP CONSTRAINT '+ (SELECT name  
			FROM sys.key_constraints  
			WHERE type = 'PK' AND parent_object_id = @refObj);

		EXEC (@sql);
		insert into ##results (message) values('Removed pk '+QUOTENAME(@fkNameConstraint) +' for table '+ QUOTENAME(OBJECT_NAME(@ParObj)));
		insert into ##results (message) values('Removed pk for table '+ QUOTENAME(OBJECT_NAME(@refObj)));

		SET @sql = 'ALTER TABLE ' + QUOTENAME(OBJECT_NAME(@refObj)) + ' ADD PRIMARY KEY ('+ QUOTENAME(@pkName) + '); '+
		'ALTER TABLE '+ QUOTENAME(OBJECT_NAME(@ParObj)) + ' ALTER COLUMN '+ QUOTENAME(@fkName) 
		+ ' INT NOT NULL; ALTER TABLE '
		+ QUOTENAME(OBJECT_NAME(@ParObj)) + ' ADD FOREIGN KEY ('+ QUOTENAME(@fkName)
		+') REFERENCES '+QUOTENAME(OBJECT_NAME(@refObj))+'('+QUOTENAME(@pkName)+');';

		PRINT @sql;
		EXEC (@sql);
		insert into ##results (message) values('Add pk '+QUOTENAME(@pkName) +' for table '+ QUOTENAME(OBJECT_NAME(@refObj)));
		insert into ##results (message) values('Add fk ' + QUOTENAME(@fkName) +' for table '+ QUOTENAME(OBJECT_NAME(@ParObj)));
		delete from #FkDetails where referenced_object_id = @refObj;
	end
";
		public static string CreateProc_TABLES_WITH_PK =>
			@"CREATE PROC TABLES_WITH_PK
AS
DECLARE @TABLEID int;
DECLARE @ROWS int;
DECLARE @pkName NVARCHAR(MAX);
DECLARE @sql NVARCHAR(MAX);
DECLARE @oldPkName NVARCHAR(MAX);

select c.name, i.object_id table_id, ty.name type_REF, i.name pk_constraint
into #tableDet
from sys.foreign_key_columns fkc right join sys.index_columns ic
on fkc.referenced_object_id = ic.object_id and fkc.referenced_column_id = ic.column_id
join sys.columns c on c.object_id = ic.object_id and c.column_id = ic.column_id
JOIN sys.types ty on ty.user_type_id = c.user_type_id
join sys.indexes i on ic.object_id = i.object_id
where fkc.parent_object_id is null and isnull(I.is_primary_key, 0) = 1;

while exists(select * from #tableDet)
begin
	
	select TOP(1) @TABLEID = t.table_id, @oldPkName = T.pk_constraint FROM #tableDet t;
	SELECT @ROWS = COUNT(table_id) from #tableDet T WHERE @TABLEID = t.table_id ;
	if @ROWS = 1
		begin
			if( (select coalesce((select TOP(1) 1 
			from #tableDet T 
			where @TABLEID = t.table_id and TYPE_REF not in ('smallint', 'int', 'bigint', 'tinyint')), 0)) = 0)
			begin
				delete from #tableDet where table_id = @TABLEID;
				CONTINUE;
			end;
		end;
		else
		begin
			DROP TABLE IF EXISTS #det;
			SELECT * INTO #DET FROM #tableDet t WHERE @TABLEID = t.table_id;

				if((select count(*) from #det where TYPE_REF in ('smallint', 'int', 'bigint', 'tinyint')) <> @ROWS)
				begin
					delete from #tableDet where table_id = @TABLEID;
					DROP TABLE #det;
					CONTINUE;
				end;
		end;
		print 'PROCESARE '+OBJECT_NAME(@TABLEID);
		EXEC Create_Column @tableId=@TABLEID, @mode = 1, @columnName = @pkName OUTPUT;
		SET @sql = 'ALTER TABLE '+ QUOTENAME(OBJECT_NAME(@TABLEID)) + ' DROP CONSTRAINT '+ QUOTENAME(@oldPkName) +'; ALTER TABLE '
		+ QUOTENAME(OBJECT_NAME(@TABLEID)) + ' ADD PRIMARY KEY ('+QUOTENAME(@pkName)+');';
		
		EXEC (@sql);
		insert into ##results (message) values('Removed pk '+QUOTENAME(@oldPkName) +' for table '+ QUOTENAME(OBJECT_NAME(@TABLEID)));
		insert into ##results (message) values('Created pk '+QUOTENAME(@pkName) +' for table '+ QUOTENAME(OBJECT_NAME(@TABLEID)));
	delete from #tableDet where table_id = @TABLEID;
end;

";
		public static string CreateProc_NORMALIZARE_PK =>
			@"CREATE PROC NORMALIZARE_PK
AS
	--BEGIN TRANSACTION;
	EXEC CREATE_TMP_TABLE_RESULTS;
	EXEC TABLES_WITHOUT_PK;
	EXEC TABLES_WITH_PK_FK;
	EXEC TABLES_WITH_PK;
	select * from ##results;
	--ROLLBACK;
";
		public static string Exec_TABLES_WITHOUT_PK =>
			@"
EXEC CREATE_TMP_TABLE_RESULTS;
EXEC TABLES_WITHOUT_PK;
select * from ##results;
";
		public static string Exec_TABLES_PK =>
			@"
			EXEC CREATE_TMP_TABLE_RESULTS;
			EXEC TABLES_WITH_PK_FK;
			EXEC TABLES_WITH_PK;
			select * from ##results;";
	}
}
