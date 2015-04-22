
--select * from module_instance mi 	INNER JOIN module m ON m.id = mi.module_fk_id 	where mi.pages_fk_publish = 0 AND m.name = 'Article'


DECLARE @pageId int
DECLARE @placeholderId int
DECLARE @moduleId int
DECLARE @order int
DECLARE @moduleInstanceID int
DECLARE @moduleMode nvarchar(255)
DECLARE @value nvarchar(max)




DECLARE db_cursor CURSOR FOR  
	select mi.pages_fk_id, mi.place_holder_fk_id, mi.id, mi.idx from module_instance mi
	INNER JOIN module m ON m.id = mi.module_fk_id
	where mi.pages_fk_publish = 0 AND m.name = 'Article'

OPEN db_cursor   
FETCH NEXT FROM db_cursor INTO @pageId, @placeholderId, @moduleInstanceID, @order

WHILE @@FETCH_STATUS = 0   
BEGIN   
		

       -- @moduleId = SELECT id FROM module WHERE name LIKE 'ArticleList'
	   SELECT @value = ms.[value] FROM [dbo].[module_settings] ms 
		 INNER JOIN settings_list sl ON sl.id = ms.settings_list_fk_id
	   WHERE ms.module_instance_fk_id = @moduleInstanceID AND ms.module_instance_fk_pages_fk_publish = 0  AND sl.name = 'ModuleMode'

		IF  @value = 0 
				SELECT @moduleId = id FROM module WHERE name = 'ArticleSingle' 
		ELSE 
			SELECT @moduleId = id FROM module WHERE name = 'ArticleList'

	   exec [dbo].[ChangeModuleInstance] @Id  = -100, @PageID = @pageId, @PublishFlag = 0, @ModuleId = @moduleId, @Order = @order, @PersistFrom = 0, @PersistTo = 0, @PersistOrder = 66, @PlaceHolderID = @placeholderId, @Changed = 1, @PendingDelete = 0
	   

       FETCH NEXT FROM db_cursor INTO @pageId, @placeholderId, @moduleInstanceID, @order
END   

CLOSE db_cursor   
DEALLOCATE db_cursor