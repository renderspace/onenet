
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Page' AND [name] = 'MetaDescription')) = 0
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('String', 'Page', 'MetaDescription', '', 'NORMAL')

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Page' AND [name] = 'MetaKeywords')) = 0
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('String', 'Page', 'MetaKeywords', '', 'NORMAL')



IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Article' AND [name] = 'ShowTeaserImage')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'Article', 'ShowTeaserImage', 'false', 'NORMAL')

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Page' AND [name] = 'RobotsIndex')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'Page', 'RobotsIndex', 'true', 'NORMAL')
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Page' AND [name] = 'RobotsFollow')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'Page', 'RobotsFollow', 'true', 'NORMAL')

	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Article' AND [name] = 'OffSet')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'Article', 'OffSet', '0', 'NORMAL')

IF (SELECT COUNT(id) FROM [dbo].[module] 
WHERE ([name] = 'IFrame')) = 0	
	INSERT INTO [dbo].[module] ( [name], module_source )
	VALUES ( 'IFrame', 'IFrame.ascx')
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'IFrame' AND [name] = 'Src')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'IFrame', 'Src', '', 'NORMAL')
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'IFrame' AND [name] = 'Width')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'IFrame', 'Width', '0', 'NORMAL')
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'IFrame' AND [name] = 'Height')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'IFrame', 'Height', '0', 'NORMAL')
	
IF (SELECT COUNT(id) FROM [dbo].[module] 
WHERE ([name] = 'FlashMovie')) = 0	
	INSERT INTO [dbo].[module] ( [name], module_source )
	VALUES ( 'FlashMovie', 'FlashMovie.ascx')

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'FlashMovie' AND [name] = 'URL')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'FlashMovie', 'URL', '', 'NORMAL')
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'FlashMovie' AND [name] = 'BackgroundColor')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'FlashMovie', 'BackgroundColor', '#FFFFFF', 'NORMAL')

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'FlashMovie' AND [name] = 'FlashVars')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'FlashMovie', 'FlashVars', '', 'NORMAL')

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'FlashMovie' AND [name] = 'Width')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'FlashMovie', 'Width', '0', 'NORMAL')
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'FlashMovie' AND [name] = 'Height')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'FlashMovie', 'Height', '0', 'NORMAL')

IF (SELECT COUNT(id) FROM [dbo].[module] 
WHERE ([name] = 'Image')) = 0	
	INSERT INTO [dbo].[module] ( [name], module_source )
	VALUES ( 'Image', 'Image.ascx')
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Image' AND [name] = 'Src')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'Image', 'Src', '', 'NORMAL')
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Image' AND [name] = 'Width')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'Image', 'Width', '0', 'NORMAL')
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Image' AND [name] = 'Height')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'Image', 'Height', '0', 'NORMAL')

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Image' AND [name] = 'Alt')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'Image', 'Alt', '', 'NORMAL')
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'WebSite' AND [name] = 'GoogleAnalyticsCode')) = 0	
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('String', 'WebSite', 'GoogleAnalyticsCode', 'UA-xxxx-x', 'NORMAL')

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ImageGallery' AND [name] = 'PopupTemplateId')) = 0		
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'ImageGallery', 'PopupTemplateId', '-1', 'NORMAL')
	
ALTER TABLE [dbo].[module_instance]  DROP CONSTRAINT [FK_module_instance_module] 
	
ALTER TABLE [dbo].[module_instance]  WITH NOCHECK ADD  CONSTRAINT [FK_module_instance_module] FOREIGN KEY([module_fk_id])
REFERENCES [dbo].[module] ([id]) ON DELETE CASCADE ON UPDATE CASCADE

ALTER TABLE [dbo].[module_instance]  DROP CONSTRAINT [FK_module_instance_pages] 

ALTER TABLE [dbo].[module_instance]  WITH CHECK ADD  CONSTRAINT [FK_module_instance_pages] FOREIGN KEY([pages_fk_id], [pages_fk_publish])
REFERENCES [dbo].[pages] ([id], [publish]) ON DELETE CASCADE ON UPDATE CASCADE

ALTER TABLE [dbo].[module_instance]  DROP CONSTRAINT [FK_module_instance_place_holder] 

ALTER TABLE [dbo].[module_instance]  WITH CHECK ADD  CONSTRAINT [FK_module_instance_place_holder] FOREIGN KEY([place_holder_fk_id])
REFERENCES [dbo].[place_holder] ([id]) ON UPDATE CASCADE

ALTER TABLE [dbo].[int_link]  DROP CONSTRAINT [FK_int_link_pages]

ALTER TABLE [dbo].[int_link]  WITH NOCHECK ADD  CONSTRAINT [FK_int_link_pages] FOREIGN KEY([pages_fk_id], [pages_fk_publish])
REFERENCES [dbo].[pages] ([id], [publish]) ON DELETE CASCADE NOT FOR REPLICATION 

GO



-- Form Selector

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'FormSelector' AND [name] = 'FormIds')) = 0
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('String', 'FormSelector', 'FormIds', '', 'NORMAL')


IF (SELECT COUNT(id) FROM [dbo].[module] 
WHERE ([name] = 'FormSelector')) = 0	
	INSERT INTO [dbo].[module] ( [name], module_source )
	VALUES ( 'FormSelector', 'FormSelector.ascx')
	
GO

IF (SELECT COUNT(id) FROM [dbo].[language]
WHERE (id = 1279)) = 0	
	INSERT INTO [dbo].[language] (id)
	VALUES ( 1279 )

	IF (SELECT COUNT(id) FROM [dbo].[language]
WHERE (id =2074	)) = 0	
	INSERT INTO [dbo].[language] (id)
	VALUES ( 2074	 )
	
	IF (SELECT COUNT(id) FROM [dbo].[language]
WHERE (id =5146	)) = 0	
	INSERT INTO [dbo].[language] (id)
	VALUES ( 5146	 )

GO

DECLARE @contentId int

DECLARE c_ids CURSOR FOR
 SELECT content_fk_id 
	FROM ucategories  uc
	WHERE ucategorie_type = 'tree_file_folder' OR ucategorie_type = 'tree_dictionary'

OPEN c_ids
FETCH NEXT FROM c_ids INTO @contentId
	UPDATE content_data_store SET language_fk_id = 1279 WHERE content_fk_id = @contentId AND language_fk_id != 1279

WHILE @@FETCH_STATUS = 0
BEGIN
 FETCH NEXT FROM c_ids INTO @contentId
	UPDATE content_data_store SET language_fk_id = 1279 WHERE content_fk_id = @contentId AND language_fk_id != 1279
END 

CLOSE c_ids
DEALLOCATE c_ids

GO
	
IF (SELECT COUNT(id) FROM [dbo].[module] 
WHERE ([name] = 'XSLT')) = 0	
	INSERT INTO [dbo].[module] ( [name], module_source )
	VALUES ( 'XSLT', 'XSLT.ascx')
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'XSLT' AND [name] = 'XSLT_URI')) = 0		
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'XSLT', 'XSLT_URI', '', 'NORMAL')

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'XSLT' AND [name] = 'XML_URI')) = 0		
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'XSLT', 'XML_URI', '', 'NORMAL')

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'XSLT' AND [name] = 'GET_ParamsList')) = 1
	DELETE FROM [dbo].[settings_list] WHERE subsystem = 'XSLT' AND [name] = 'GET_ParamsList'

GO

ALTER TABLE dbo.pages ADD
	viewGroups varchar(255) NOT NULL DEFAULT ''

ALTER TABLE dbo.pages ADD
	editGroups varchar(255) NOT NULL DEFAULT ''

ALTER TABLE dbo.pages ADD
	requireSSL bit NOT NULL DEFAULT 0
	
GO
	
ALTER PROCEDURE [dbo].[ChangePage] 
	@Id int out,
	@ParentId int,
	@PublishFlag int,
	@Changed int,
	@Order int,
	@PendingDelete int, 
	@MenuGroup int,
	@BreakPersistance int,
	@Level int,
	@ContentId int,
	@TemplateId int,
	@WebSiteId int,
	@redirectToUrl nvarchar(255),
	@viewGroups varchar(255),
	@editGroups varchar(255),
	@requireSSL bit
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @Exists int
	SELECT @Exists = COUNT(id) FROM pages WHERE id = @Id AND publish = @PublishFlag AND web_site_fk_id = @WebSiteId

	IF (@ParentId < 1)
		SET @ParentId = NULL

	IF (@Exists > 0)
	BEGIN
		UPDATE [dbo].[pages] SET break_persistence = @BreakPersistance, [level] = @Level, 
		content_fk_id = @ContentId, template_fk_id = @TemplateId,
		pages_fk_publish = @PublishFlag, pages_fk_id = @ParentId, changed = @Changed, 
		menu_group = @MenuGroup, idx = @Order, web_site_fk_id = @WebSiteId, pending_delete = @PendingDelete,
		redirectToUrl = @redirectToUrl, viewGroups = @viewGroups, editGroups = @editGroups, requireSSL = @requireSSL
		WHERE id = @Id AND publish = @PublishFlag
	END
	ELSE
	BEGIN
		IF (@PublishFlag = 0)
		BEGIN
			EXEC nextval 'pages', @Id out
			IF (@Order = 0) 
			BEGIN
				SET @Order = @Id
			END
		END

		INSERT INTO pages 
		(id,  break_persistence, level, publish, content_fk_id, template_fk_id,
		pages_fk_publish, pages_fk_id, changed, menu_group, idx, web_site_fk_id, pending_delete,
		redirectToUrl, viewGroups, editGroups, requireSSL)
		VALUES
		(@Id, @BreakPersistance, @Level, @PublishFlag, @ContentId, @TemplateId,
		@PublishFlag, @ParentId, @Changed, @MenuGroup, @Order, @WebSiteId, @PendingDelete,
		@redirectToUrl, @viewGroups, @editGroups, @requireSSL )
	END
END

GO

ALTER PROCEDURE create_web_site
	@websiteName varchar(255),
	@defaultLanguageId int,
	@principal varchar(255) = 'unknown'
AS
BEGIN
	DECLARE @websiteId int
	DECLARE @contentId int

	INSERT INTO content (date_created, principal_created_by)
	values (getdate(), @principal)
	SET @contentId = SCOPE_IDENTITY()

	insert into content_data_store ( language_fk_id, content_fk_id, title, subtitle, teaser, html)
	values ( @defaultLanguageId, @contentId, @websiteName, '', '', '')

	insert into web_site ( content_fk_id )
	values (@contentId)
	SET @websiteId = SCOPE_IDENTITY()
	
	exec ChangeWebSiteSetting @websiteId, 'PrimaryLanguageId', @defaultLanguageId
	
	SELECT @websiteId
END
GO

	
UPDATE settings_list SET user_visibility = 'SPECIAL' WHERE subsystem = 'WebSite' AND [name] = 'PrimaryLanguageId'

GO

ALTER PROCEDURE [dbo].[ChangeEvent]
	@publishFlag bit,
	@changed bit,
	@contentId int,
	@markedForDeletion bit,
	@begin_date datetime,
	@end_date datetime=null,
	@id int=null output
AS

	IF ( @id is null )
	BEGIN
		EXEC [dbo].[nextval] 'event', @id out		

		INSERT INTO [dbo].[event] ( id, publish, changed, content_fk_id, marked_for_deletion, begin_date, end_date)
		VALUES (@id, @publishFlag, @changed, @contentId, @markedForDeletion, @begin_date, @end_date)
	END
	ELSE
	BEGIN
		DECLARE @Exists int
		SELECT @Exists = count(*) FROM [dbo].[event] WHERE id=@id AND publish=@publishFlag

		IF ( @Exists = 0 )
		BEGIN
			INSERT INTO [dbo].[event] ( id, publish, changed, content_fk_id, marked_for_deletion, begin_date, end_date)
			VALUES (@id, @publishFlag, @changed, @contentId, @markedForDeletion, @begin_date, @end_date)
		END
		ELSE
		BEGIN
			UPDATE [dbo].[event]
			SET changed=@changed, content_fk_id=@contentID, marked_for_deletion=@markedForDeletion, begin_date = @begin_date, end_date = @end_date
			WHERE id=@id AND publish=@publishFlag
		END
	END



GO

create  function DateOnly(@DateTime DateTime)
-- Returns @DateTime at midnight; i.e., it removes the time portion of a DateTime value.
returns datetime
as
    begin
		return dateadd(dd,0, datediff(dd,0,@DateTime))
    end
go


------------------------------------------------------------------------------------------------------------------------------------------

IF (SELECT COUNT(id) FROM [dbo].[module] 
WHERE ([name] = 'Events')) = 0	
	INSERT INTO [dbo].[module] ( [name], module_source )
	VALUES ( 'Events', 'Events.ascx')
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Events' AND [name] = 'ShowTitle')) = 0		
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('Bool', 'Events', 'ShowTitle', 'true', 'NORMAL')

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Events' AND [name] = 'ShowSubTitle')) = 0		
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('Bool', 'Events', 'ShowSubTitle', 'false', 'NORMAL')
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Events' AND [name] = 'ShowTeaser')) = 0		
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('Bool', 'Events', 'ShowTeaser', 'false', 'NORMAL')

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Events' AND [name] = 'ShowHtml')) = 0		
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('Bool', 'Events', 'ShowHtml', 'false', 'NORMAL')
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Events' AND [name] = 'ShowModuleTitle')) = 0		
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'Events', 'ShowModuleTitle', 'false', 'NORMAL')
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Events' AND [name] = 'CategoryList')) = 0		
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'Events', 'CategoryList', '', 'NORMAL')

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Events' AND [name] = 'RecordsPerPage')) = 0		
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'Events', 'RecordsPerPage', '10', 'NORMAL')
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Events' AND [name] = 'ShowPager')) = 0			
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('Bool', 'Events', 'ShowPager', 'false', 'NORMAL')
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Events' AND [name] = 'SingleEventUri')) = 0		
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('String', 'Events', 'SingleEventUri', '', 'NORMAL')
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Events' AND [name] = 'ModuleMode')) = 0		
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('Int', 'Events', 'ModuleMode', '1', 'NORMAL')

	-------------
	
ALTER TABLE [dbo].[content] ADD votes int NOT NULL DEFAULT 0
ALTER TABLE [dbo].[content] ADD score float NULL

GO
	
ALTER PROCEDURE [dbo].[ChangeContent]
      @title nvarchar(4000),
      @subtitle nvarchar(255),
      @teaser ntext,
      @html ntext,
      @languageID int,
      @principal varchar(255),
      @contentID int = NULL,
	  @votes int = 0,
	  @score float = NULL
AS
      SET @contentID = (SELECT id FROM content WHERE id = @contentID)
      IF (
		(@contentID IS NOT NULL) AND 
		((SELECT idd 
			FROM content_data_store 
			WHERE language_fk_id = @languageID 
			AND content_fk_id = @contentID) IS NOT NULL))
      BEGIN
           UPDATE content_data_store SET  title = @title, subtitle = @subtitle, teaser = @teaser, html = @html WHERE content_fk_id = @contentID AND language_fk_id = @languageID
           UPDATE content SET date_modified = getdate(),  principal_modified_by = @principal where id = @contentID
      END
      ELSE IF (@contentID IS NOT NULL) 
		BEGIN
			INSERT INTO content_data_store (title, subtitle, teaser, html, language_fk_id, content_fk_id)
				VALUES (@title, @subtitle, @teaser, @html, @languageID, @contentID)
			UPDATE content SET date_modified = getdate(),  principal_modified_by = @principal where id = @contentID
		END
	  ELSE 
      BEGIN
           IF (@contentID IS NULL)
           BEGIN
                INSERT INTO content (date_created, principal_created_by) VALUES (getdate(), @principal)
                SET @contentID = @@IDENTITY
           END
           INSERT INTO content_data_store (title, subtitle, teaser, html, language_fk_id, content_fk_id)
           VALUES (@title, @subtitle, @teaser, @html, @languageID, @contentID)
      END
	  
	  IF (@score IS NOT NULL)
			   UPDATE content SET votes = @votes, score = @score

      SELECT @contentID AS content_id
	  
GO
	
CREATE PROCEDURE [dbo].[Vote]
  @votedScore int,
  @contentId int
AS	
	IF (@votedScore > 5 OR @votedScore < 1)
		RAISERROR(50001, 10, 1, N'voted score OUT OF RANGE')
	
      SELECT @contentId = id FROM [dbo].[content] WHERE id =  @contentId

	  IF (@contentId IS NOT NULL)
		BEGIN
			BEGIN TRANSACTION
				UPDATE [dbo].[content]  
					SET 
						votes = votes + 1, 
						score = ((votes * ISNULL(score, 0)) + @votedScore) / (votes + 1)
					WHERE id = @contentId
			COMMIT
			SELECT score, votes FROM [dbo].[content] WHERE id =  @contentId
		END
GO


---------------------------------------------------------------------------------------------

IF (SELECT COUNT(id) FROM [dbo].[module] 
WHERE ([name] = 'FolderGallery')) = 0	
	INSERT INTO [dbo].[module] ( [name], module_source )
	VALUES ( 'FolderGallery', 'FolderGallery.ascx')
	
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'FolderGallery' AND [name] = 'ImageWidth')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'FolderGallery', 'ImageWidth', '150', 'NORMAL')

GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'FolderGallery' AND [name] = 'ImageHeight')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'FolderGallery', 'ImageHeight', '0', 'NORMAL')

GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'FolderGallery' AND [name] = 'FolderId')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'FolderGallery', 'FolderId', '0', 'NORMAL')

GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'FolderGallery' AND [name] = 'RecordsPerPage')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'FolderGallery', 'RecordsPerPage', '10', 'NORMAL')
	
GO
	
----------------------------------------------------------------------------------------------------------------------------------

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
	WHERE (subsystem = 'Article' AND [name] = 'ModuleSource')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'Article', 'ModuleSource', '', 'NORMAL')
	
	
GO


	
ALTER PROCEDURE [dbo].[ListPagedArticles]
	@publishFlag bit,
	@languageId int,
	@offSet int = 0,
	@fromRecordIndex int,
	@toRecordIndex int,
	@sortByColumn varchar(255) = NULL,
	@sortOrder varchar(4) = NULL,
	@regularIds varchar(255)  = NULL,
	@dateFrom datetime  = NULL,
	@dateTo datetime = NULL,
	@showUntranslated bit = 0,
	@changed bit = null,
	@debug     bit          = 0
AS
BEGIN	
	CREATE TABLE #regularIDs (regularID int NOT NULL)
	CREATE TABLE #numbers (listpos int NOT NULL, number  int NOT NULL)
	EXEC [dbo].[Acc_IntList2Table] @regularIds
	INSERT #regularIDs  SELECT number FROM #numbers
	DROP TABLE #numbers

	DECLARE @sql        nvarchar(4000),
			@paramlist  nvarchar(4000)

	SELECT @sql =
    '	
		SELECT	cds.title, cds.subtitle, cds.teaser, cds.html, c.principal_created_by, c.date_created, 
				c.principal_modified_by, c.date_modified, c.votes, c.score, a.id, a.publish, a.content_fk_id,
				a.display_date, a.marked_for_deletion, a.changed, ( select count(a2.id) FROM [dbo].[article] a2 WHERE a2.id=a.id AND a2.publish=1) countPublished
		FROM [dbo].[article] a
		INNER JOIN [dbo].[content] c ON c.id=a.content_fk_id
		INNER JOIN [dbo].[regular_has_articles] ra ON ra.article_fk_id=a.id and ra.article_fk_publish=a.publish
	'

	IF (@showUntranslated = 0)
		SELECT @sql = @sql + '	INNER '
	ELSE	
		SELECT @sql = @sql + '	LEFT '

	SELECT @sql = @sql + ' JOIN [dbo].[content_data_store] cds ON cds.content_fk_id=c.id AND cds.language_fk_id=@xlanguageId '

	SELECT @sql = @sql + ' WHERE a.publish = ' + CAST(@publishFlag AS char(1))

	IF (SELECT COUNT (*) FROM #regularIDs) > 0
		SELECT @sql = @sql + 'AND ra.regular_fk_id IN (SELECT regularID FROM #regularIDs) ' 

	IF @dateFrom IS NOT NULL             
		SELECT @sql = @sql + ' AND a.display_date >= @xdateFrom '

	IF @dateTo IS NOT NULL             
		SELECT @sql = @sql + ' AND a.display_date <= @xdateTo '

	IF @changed IS NOT NULL
		SELECT @sql = @sql + ' AND a.changed = @xchanged '

	IF @sortByColumn IS NOT NULL             
		SELECT @sql = @sql + ' ORDER BY ' + @sortByColumn
	IF @sortByColumn IS NOT NULL AND @sortOrder IS NOT NULL
		SELECT @sql = @sql + ' ' + @sortOrder

	IF @debug = 1
		PRINT @sql 

	SELECT @paramlist = '@xlanguageId int,
						 @xdateFrom  datetime,
						 @xdateTo    datetime,
						 @xchanged   bit'

	CREATE TABLE #pagingTable
	(
		title nvarchar(4000),
		subtitle nvarchar(255),
		teaser ntext,
		html ntext,
		principal_created_by varchar(255),
		date_created datetime,
		principal_modified_by varchar(255),
		date_modified datetime,
		votes int,
		score float,
		article_id int,
		publish bit,
		content_id int,
		display_date datetime, 
		marked_for_deletion bit, 
		changed bit, 
		countPublished int,		
		ID int IDENTITY PRIMARY KEY,
	)

	INSERT #pagingTable
		EXEC sp_executesql @sql, @paramlist,     
                   @languageId, @dateFrom, @dateTo, @changed

	DECLARE @AllRecords int
	SET @fromRecordIndex = @fromRecordIndex + @offSet
	SET @AllRecords = (SELECT COUNT (ID) FROM #pagingTable)
	
	SELECT *, @AllRecords
	FROM #pagingTable pT 
	WHERE pT.ID between @fromRecordIndex AND @toRecordIndex 

	DROP table #pagingTable

END

GO

------------------------------------------------------------------------------------------------------------------

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Article' AND [name] = 'UniqueTranslation')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'Article', 'UniqueTranslation', 'false', 'NORMAL')
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Article' AND [name] = 'TeaserImageWidth')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'Article', 'TeaserImageWidth', '150', 'NORMAL')
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Image' AND [name] = 'href')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'Image', 'href', '', 'NORMAL')
	
GO
	
ALTER PROCEDURE [dbo].[ListPagedPages]
	@webSiteId int,
	@publishFlag bit,
	@languageId int,
	@fromRecordIndex int,
	@toRecordIndex int,
	@sortByColumn varchar(255) = NULL,
	@sortOrder varchar(4) = NULL,
	@changed bit = null,
	@debug     bit          = 0
AS
BEGIN	

	set ANSI_NULLS ON
	set QUOTED_IDENTIFIER ON

	DECLARE @sql        nvarchar(4000),
			@paramlist  nvarchar(4000)

	SELECT @sql =
    'SELECT cds.title, cds.subtitle, cds.teaser, cds.html, c.principal_created_by, c.date_created, 
            c.principal_modified_by, c.date_modified, c.votes, c.score, c.id ContentId, p.id PageId, p.pages_fk_id, p.menu_group, p.idx, 
			t.id TemplateId, t.name, level, pending_delete, changed, 
            break_persistence, web_site_fk_id, redirectToUrl, [viewGroups], [editGroups], [requireSSL]
     FROM [dbo].[pages] p	
     INNER JOIN [dbo].[content_data_store] cds ON cds.content_fk_id = p.content_fk_id AND cds.language_fk_id = @xlanguageId
     INNER JOIN [dbo].[content] c ON c.id = p.content_fk_id
     INNER JOIN [dbo].[template] t ON t.id = p.template_fk_id AND template_type = ''3'' '

	SELECT @sql = @sql + 'WHERE p.publish = @xpublishFlag AND p.web_site_fk_id=@xwebSiteId'

	IF @changed IS NOT NULL
		SELECT @sql = @sql + ' AND p.changed = @xchanged '

	IF @sortByColumn IS NOT NULL             
		SELECT @sql = @sql + ' ORDER BY ' + @sortByColumn
	IF @sortByColumn IS NOT NULL AND @sortOrder IS NOT NULL
		SELECT @sql = @sql + ' ' + @sortOrder

	IF @debug = 1
		PRINT @sql 

	SELECT @paramlist = '@xlanguageId int,
						@xwebSiteId int,
						@xpublishFlag bit,
						@xchanged bit'

	CREATE TABLE #pagingTable
	(
		ID int IDENTITY PRIMARY KEY,
		Title nvarchar(4000), 
		SubTitle nvarchar(255),
        Teaser ntext,
        Html ntext,
        PrincipalCreated varchar(255),
        DateCreated datetime,
        PrincipalModified varchar(255),
		DateModified datetime,
		votes int,
		score float,
		ContentId int,
		PageId int,
		ParentId int,
		MenuGroup int,
		Idx int, 
		TemplateID int,
		Template varchar(255), 
		[Level] int, 
		PendingDelete bit,
		Changed bit, 
		BreakPersistance bit,
		WebSiteId int,
		RedirectToUrl nvarchar(255), 
		[viewGroups] varchar(255), 
		[editGroups] varchar(255), 
		[requireSSL] bit
	)

	INSERT #pagingTable
		EXEC sp_executesql @sql, @paramlist,     
                   @languageId, @webSiteId, @publishFlag, @changed

	SELECT Title, SubTitle, Teaser, Html, PrincipalCreated, DateCreated, 
            PrincipalModified, DateModified, votes, score, ContentId, PageId, ParentId, MenuGroup, Idx, 
			TemplateId, Template, [Level], PendingDelete, Changed, 
            BreakPersistance, WebSiteId, RedirectToUrl, [viewGroups], [editGroups], [requireSSL]
	FROM #pagingTable pT 
	WHERE pT.ID >= @fromRecordIndex AND pT.ID <= @toRecordIndex

	SELECT COUNT (ID) FROM #pagingTable

	DROP table #pagingTable

END

GO

	
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'FolderGallery' AND [name] = 'RequireDescription')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'FolderGallery', 'RequireDescription', 'false', 'NORMAL')
	
	
	IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'FolderGallery' AND [name] = 'EnableVote')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'FolderGallery', 'EnableVote', 'false', 'NORMAL')
	
GO


-------------------------------------------------------------------------------------------------------------------------------

IF (SELECT COUNT(id) FROM [dbo].[module] 
WHERE ([name] = 'Comments')) = 0	
	INSERT INTO [dbo].[module] ( [name], module_source )
	VALUES ( 'Comments', 'Comments.ascx')
	
GO

	IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Comments' AND [name] = 'UseCaptcha')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'Comments', 'UseCaptcha', 'false', 'NORMAL')
	
GO

	IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Comments' AND [name] = 'EnableModeration')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'Comments', 'EnableModeration', 'false', 'NORMAL')
	
GO

	IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Article' AND [name] = 'EnableCommentProvider')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'Article', 'EnableCommentProvider', 'false', 'NORMAL')
	
GO

	IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Comments' AND [name] = 'SendCommentsToAddress')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'Comments', 'SendCommentsToAddress', '', 'NORMAL')
	
GO

	IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Comments' AND [name] = 'SendCommentsFromAddress')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'Comments', 'SendCommentsFromAddress', '', 'NORMAL')
	
GO

	IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Comments' AND [name] = 'RecordsPerPage')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'Comments', 'RecordsPerPage', '10', 'NORMAL')
	
GO

	IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Comments' AND [name] = 'SortByColumn')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'Comments', 'SortByColumn', '', 'NORMAL')
	
GO

-------------------------------------------------------------------------------------------------------------------------

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_comments_content]') AND parent_object_id = OBJECT_ID(N'[dbo].[comments]'))
ALTER TABLE [dbo].[comments] DROP CONSTRAINT [FK_comments_content]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[comments]') AND type in (N'U'))
DROP TABLE [dbo].[comments]
GO

CREATE TABLE [dbo].[comments](
	[id] [int] NOT NULL,
	[publish] [bit] NOT NULL CONSTRAINT [DF_comments_published]  DEFAULT ((0)),
	[changed] [bit] NOT NULL,
	[content_fk_id] [int] NOT NULL,
	[comment] [ntext] COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[name] [nvarchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[email] [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[commented_at] [datetime] NOT NULL CONSTRAINT [DF_comments_commented_at]  DEFAULT (getdate()),
	[marked_for_deletion] [bit] NOT NULL
 CONSTRAINT [PK_comments] PRIMARY KEY CLUSTERED 
(
	[id] ASC,
	[publish] ASC
)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
ALTER TABLE [dbo].[comments]  WITH CHECK ADD  CONSTRAINT [FK_comments_content] FOREIGN KEY([content_fk_id])
REFERENCES [dbo].[content] ([id])
GO

INSERT INTO [dbo].[sequences] (seq, sequence_id)
VALUES ( 'comments', 0)

GO


CREATE PROCEDURE [dbo].[ChangeComment]
	@id int out,
	@publish bit,
	@changed bit,
	@markedForDeletion bit,
	@contentId int,
	@comment ntext,
	@email varchar(255),
	@name nvarchar(255)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @Exists int
	SELECT @Exists = COUNT(id) FROM comments WHERE id = @id AND publish = @publish

	IF (@Exists > 0)
	BEGIN
		UPDATE [dbo].[comments] 
			SET	comment=@comment, 
				[name]=@name,
				email=@email,
				changed=@changed,
				marked_for_deletion=@markedForDeletion
			WHERE id=@id AND publish=@publish
	END
	ELSE
	BEGIN
		IF (@publish = 0)
		BEGIN
			EXEC nextval 'comments', @id out
		END

		INSERT INTO [dbo].[comments] 
		(id, publish, content_fk_id, comment, email, [name], changed, marked_for_deletion, commented_at) 
		VALUES 
		(@id, @publish, @contentId, @comment, @email, @name, @changed, @markedForDeletion, getdate());

	END
END

GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Comments' AND [name] = 'UniqueTranslation')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'Comments', 'UniqueTranslation', 'false', 'NORMAL')

GO

CREATE PROCEDURE [dbo].[ListPagedForms] 
@sortBy varchar(255), 
@sortDirection varchar(255), 
@fromRecordIndex int, 
@toRecordIndex int,
@debug int=0
AS
	DECLARE @sql        nvarchar(4000),
			@paramlist  nvarchar(4000)

	SELECT @sql =
    'SELECT f.id, f.content_fk_id, f.form_type, f.send_to, 
		(SELECT COUNT(id) FROM [dbo].[nform_submission] WHERE nform_fk_id=f.id) cnt,
        (SELECT MIN(finished) FROM [dbo].[nform_submission] WHERE nform_fk_id=f.id) first_submission_date,
        (SELECT MAX(finished) FROM [dbo].[nform_submission] WHERE nform_fk_id=f.id) last_submission_date,
        allow_multiple_submissions,
        allow_modify_in_submission
     FROM [dbo].[nform] f'

	IF @sortBy IS NOT NULL             
		SELECT @sql = @sql + ' ORDER BY ' + @sortBy
	IF @sortBy IS NOT NULL AND @sortDirection IS NOT NULL
		SELECT @sql = @sql + ' ' + @sortDirection

	IF @debug = 1
		PRINT @sql 

	SELECT @paramlist = ''

	CREATE TABLE #pagingTable
	(
		TempID int IDENTITY PRIMARY KEY,
		id int,
		content_fk_id int,
		form_type varchar(255),
		send_to nvarchar(1000),
		submission_count int,
		first_submission_date datetime,
		last_submission_date datetime,
		allow_multiple_submissions bit,
        allow_modify_in_submission bit
	)

	INSERT #pagingTable
		EXEC sp_executesql @sql, @paramlist

	SELECT id, form_type, content_fk_id, send_to, submission_count, first_submission_date, last_submission_date, allow_multiple_submissions, allow_modify_in_submission
	FROM
		#pagingTable
	WHERE
		TempID >= @fromRecordIndex AND TempID <= @toRecordIndex

	select count(*) from #pagingTable

	DROP table #pagingTable
	
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[nform_get_paged]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[nform_get_paged]

GO

if ((SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'nform' AND COLUMN_NAME = 'send_from') = 1)
begin
	ALTER TABLE [dbo].[nform] DROP COLUMN send_from
END

GO

DECLARE @settingListId INT
SELECT @settingListId=id FROM settings_list WHERE [name]='SubscriptionEmailSender'
DELETE FROM module_settings WHERE settings_list_fk_id=@settingListId
DELETE FROM settings_list WHERE id=@settingListId
SELECT @settingListId=id FROM settings_list WHERE [name]='SendCommentsFromAddress'
DELETE FROM module_settings WHERE settings_list_fk_id=@settingListId
DELETE FROM settings_list WHERE id=@settingListId

GO

------------

IF (SELECT COUNT(id) FROM [dbo].[module] 
WHERE ([name] = 'FlashFolderGallery')) = 0	
	INSERT INTO [dbo].[module] ( [name], module_source )
	VALUES ( 'FlashFolderGallery', 'FlashFolderGallery.ascx')
	
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'FlashFolderGallery' AND [name] = 'FolderId')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'FlashFolderGallery', 'FolderId', '0', 'NORMAL')

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'FlashFolderGallery' AND [name] = 'textColor')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'FlashFolderGallery', 'textColor', '000000', 'NORMAL')
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'FlashFolderGallery' AND [name] = 'frameColor')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'FlashFolderGallery', 'frameColor', '333333', 'NORMAL')
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'FlashFolderGallery' AND [name] = 'backColor')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'FlashFolderGallery', 'backColor', 'AAAAAA', 'NORMAL')
	
IF (SELECT COUNT(id) FROM [dbo].[module] 
WHERE ([name] = 'SearchCrawler')) = 0	
	INSERT INTO [dbo].[module] ( [name], module_source )
	VALUES ( 'SearchCrawler', 'SearchCrawler.ascx')	

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'SearchCrawler' AND [name] = 'RecordsPerPage')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'SearchCrawler', 'RecordsPerPage', '10', 'NORMAL')
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'SearchCrawler' AND [name] = 'WebSiteIdsString')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'SearchCrawler', 'WebSiteIdsString', '', 'NORMAL')

GO	

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ListArticleSearchResults]') AND type in (N'P', N'PC'))
DROP PROCEDURE ListArticleSearchResults
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ListAllSearchResults]') AND type in (N'P', N'PC'))
DROP PROCEDURE ListAllSearchResults
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ListTextContentSearchResults]') AND type in (N'P', N'PC'))
DROP PROCEDURE ListTextContentSearchResults
GO

--

DECLARE @settingListId INT
SELECT @settingListId=id FROM settings_list WHERE [name]='SearchThisModuleInstance' AND subsystem='TextContent'
DELETE FROM module_settings WHERE settings_list_fk_id=@settingListId
DELETE FROM settings_list WHERE id=@settingListId


SELECT @settingListId=id FROM settings_list WHERE [name]='SearchThisModuleInstance' AND subsystem='Article'
DELETE FROM module_settings WHERE settings_list_fk_id=@settingListId
DELETE FROM settings_list WHERE id=@settingListId

SELECT @settingListId=id FROM settings_list WHERE [name]='RecordsPerPage' AND subsystem='Search'
DELETE FROM module_settings WHERE settings_list_fk_id=@settingListId
DELETE FROM settings_list WHERE id=@settingListId

SELECT @settingListId=id FROM settings_list WHERE [name]='ShowDisplayDate' AND subsystem='Search'
DELETE FROM module_settings WHERE settings_list_fk_id=@settingListId
DELETE FROM settings_list WHERE id=@settingListId

SELECT @settingListId=id FROM settings_list WHERE [name]='ShowExtraPagerText' AND subsystem='Search'
DELETE FROM module_settings WHERE settings_list_fk_id=@settingListId
DELETE FROM settings_list WHERE id=@settingListId

SELECT @settingListId=id FROM settings_list WHERE [name]='ResultsPageUri' AND subsystem='Search'
DELETE FROM module_settings WHERE settings_list_fk_id=@settingListId
DELETE FROM settings_list WHERE id=@settingListId

DELETE FROM [dbo].[module] WHERE [name] = 'Search'

	
-----------------------------------

if ((SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ucategorie_belongs_to' AND COLUMN_NAME = 'idx') = 0)
BEGIN
ALTER TABLE ucategorie_belongs_to ADD idx int NULL
END
GO
update ucategorie_belongs_to SET idx=id
GO
ALTER TABLE [dbo].[ucategorie_belongs_to] ALTER COLUMN [idx] int NOT NULL
GO
CREATE PROCEDURE [dbo].[ChangeUCategorieBelongsTo]
@categoryId int,
@tableName varchar(255),
@fkId int,
@idx int=null,
@id int=null
AS
BEGIN
	IF @id is NULL
	BEGIN
		INSERT INTO [dbo].[ucategorie_belongs_to]
		(ucategories_fk_id, fkid, table_name, idx)
		VALUES 
		(@categoryID, @fkId, @tableName, -1)	

		SET @id=SCOPE_IDENTITY()
		UPDATE [dbo].[ucategorie_belongs_to] SET idx=@id WHERE id=@id 		
	END
	ELSE
	BEGIN
		IF (@idx IS NOT NULL)
		BEGIN
			UPDATE [dbo].[ucategorie_belongs_to]
			SET idx=@idx
			WHERE id=@id
		END
	END
END
GO

UPDATE ucategorie_belongs_to SET table_name = '[dbo].[faq_question]' WHERE ucategories_fk_id IN
	(SELECT id FROM ucategories WHERE ucategorie_type LIKE '%faq%')
	AND table_name IS NULL
GO

SELECT @settingListId=id FROM settings_list WHERE [name]='ShowNumberOfQuestions' AND subsystem='FrequentQuestions'
DELETE FROM module_settings WHERE settings_list_fk_id=@settingListId
DELETE FROM settings_list WHERE id=@settingListId

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'FrequentQuestions' AND [name] = 'RecordsPerPage')) = 0
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('Int', 'FrequentQuestions', 'RecordsPerPage', '10', 'NORMAL')

GO


ALTER PROCEDURE [dbo].[ListPagedComments]
	@fromRecordIndex int,
	@toRecordIndex int,
	@sortByColumn varchar(255) = NULL,
	@sortOrder varchar(4) = NULL,
	@changed bit=null,
	@publish bit=null,
	@contentId int=null,
	@languageId int=null,
	@debug bit=0
AS
BEGIN	
	DECLARE @sql        nvarchar(4000),
			@paramlist  nvarchar(4000)

	SELECT @sql =
    '	SELECT c.content_fk_id, c.email, c.[name], c.comment, c.commented_at, c.id, c.publish, c.changed, c.marked_for_deletion, 
			( select count(c2.id) FROM [dbo].[comments] c2 WHERE c2.id=c.id AND c2.publish=1) countPublished '

	if @languageId IS NOT NULL
		SELECT @sql = @sql + ', cds.title '
	ELSE
		SELECT @sql = @sql + ', '''' '
	
	SELECT @sql = @sql + ' FROM [dbo].[comments] c '

	if @languageId IS NOT NULL
		SELECT @sql = @sql + ' INNER JOIN [dbo].[content_data_store] cds ON cds.content_fk_id=c.content_fk_id AND cds.language_fk_id=@xlanguageId '        

	SELECT @sql = @sql + 'WHERE 1=1'

	IF @contentId IS NOT NULL             
		SELECT @sql = @sql + ' AND content_fk_id = @xcontentId '

	IF @changed IS NOT NULL             
		SELECT @sql = @sql + ' AND changed = @xchanged '

	IF @publish IS NOT NULL             
		SELECT @sql = @sql + ' AND publish = @xpublish '

	IF @sortByColumn IS NOT NULL             
		SELECT @sql = @sql + ' ORDER BY ' + @sortByColumn
	IF @sortByColumn IS NOT NULL AND @sortOrder IS NOT NULL
		SELECT @sql = @sql + ' ' + @sortOrder

	IF @debug = 1
		PRINT @sql 

	SELECT @paramlist = '@xcontentId int,
						 @xpublish bit,
						 @xchanged bit,
						 @xlanguageId int'

	CREATE TABLE #pagingTable
	(
		TempID int IDENTITY PRIMARY KEY,
		content_fk_id int, 
		email varchar(255), 
		[name] nvarchar(255), 
		comment ntext, 
		commented_at datetime,
		commentId int,
		publish bit,
		changed bit,
		marked_for_deletion bit,
		countPublished int,
		title nvarchar(4000)
	)

	INSERT #pagingTable
		EXEC sp_executesql @sql, @paramlist,     
                   @contentId, @publish, @changed, @languageId

	SELECT
	content_fk_id, email, [name], comment, commented_at, commentId, publish, changed, marked_for_deletion, countPublished, title
	FROM
		#pagingTable
	WHERE 
		TempID >= @fromRecordIndex AND TempID <= @toRecordIndex

	select count(*) from #pagingTable

	drop table #pagingTable

END

GO

CREATE PROCEDURE [dbo].[SwapOrderOfCategorizedItems]
	@categoryId int,
	@fk1Id int,
	@fk2Id int
AS
BEGIN	
	DECLARE @order1  int,
			@order2  int

	BEGIN TRANSACTION

		SELECT TOP(1) @order1 = idx
			FROM ucategorie_belongs_to 
			WHERE fkid = @fk1Id AND  ucategories_fk_id = @categoryId
		SELECT TOP(1) @order2 = idx 
			FROM ucategorie_belongs_to 
			WHERE fkid = @fk2Id AND  ucategories_fk_id = @categoryId

		IF @order1 IS NOT NULL AND @order2 IS NOT NULL
		BEGIN
			UPDATE ucategorie_belongs_to SET idx = @order2
				WHERE fkid = @fk1Id AND  ucategories_fk_id = @categoryId

			UPDATE ucategorie_belongs_to SET idx = @order1
				WHERE fkid = @fk2Id AND  ucategories_fk_id = @categoryId
		END		

		IF @@ERROR <> 0
		BEGIN
			ROLLBACK

			RAISERROR ('Error in deleting employees in DeleteDepartment.', 16, 1)
			RETURN
		END

	COMMIT TRANSACTION

END

GO


IF (SELECT COUNT(id) FROM [dbo].[module] 
WHERE ([name] = 'FolderDocumentList')) = 0	
	INSERT INTO [dbo].[module] ( [name], module_source )
	VALUES ( 'FolderDocumentList', 'FolderDocumentList.ascx')
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'FolderDocumentList' AND [name] = 'FolderId')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'FolderDocumentList', 'FolderId', '0', 'NORMAL')
	
SELECT @settingListId=id FROM settings_list WHERE [name]='WebSiteTitle' AND subsystem='WebSite'
DELETE FROM web_site_settings WHERE settings_list_fk_id=@settingListId
DELETE FROM settings_list WHERE id=@settingListId
SELECT @settingListId=id FROM settings_list WHERE [name]='PrintCssFile' AND subsystem='WebSite'
DELETE FROM web_site_settings WHERE settings_list_fk_id=@settingListId
DELETE FROM settings_list WHERE id=@settingListId
SELECT @settingListId=id FROM settings_list WHERE [name]='ScreenCssFile' AND subsystem='WebSite'
DELETE FROM web_site_settings WHERE settings_list_fk_id=@settingListId
DELETE FROM settings_list WHERE id=@settingListId

----------------

DECLARE @Id int

DECLARE c_ids CURSOR FOR
 SELECT cbt.id 
	FROM ucategories  uc
	INNER JOIN [dbo].[ucategorie_belongs_to] cbt ON cbt.ucategories_fk_id = uc.id
	WHERE ucategorie_type = 'tree_file_folder' 

OPEN c_ids
FETCH NEXT FROM c_ids INTO @Id
	UPDATE [ucategorie_belongs_to] SET table_name = '[dbo].[files]' WHERE id = @Id

WHILE @@FETCH_STATUS = 0
BEGIN
 FETCH NEXT FROM c_ids INTO @Id
	UPDATE [ucategorie_belongs_to] SET table_name = '[dbo].[files]' WHERE id = @Id
END 

CLOSE c_ids
DEALLOCATE c_ids


---------------

CREATE TABLE [dbo].[publisher](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[subsystem] [varchar](255) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[fkid] [int] NOT NULL,
	[scheduled_at] [datetime] NOT NULL,
	[published_at] [datetime] NULL,
	[table] [varchar](255) NOT NULL,
 CONSTRAINT [PK_publisher] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE PROCEDURE [dbo].[ListPagedPublisherData]
	@published bit,
	@fromRecordIndex int,
	@toRecordIndex int,
	@sortByColumn varchar(255) = NULL,
	@sortOrder varchar(4) = NULL,
	@show_pending bit = 0,
	@debug bit = 0
AS
BEGIN	
	
	DECLARE @sql        nvarchar(4000),
			@paramlist  nvarchar(4000)

	SELECT @sql =
    '	SELECT id, subsystem, fkid, scheduled_at, published_at, [table]
		FROM [dbo].[publisher]'

	IF (@published = 0)
		SELECT @sql = @sql + ' WHERE published_at IS NULL'
	ELSE
		SELECT @sql = @sql + ' WHERE published_at IS NOT NULL'

	IF (@show_pending != 0 AND @published = 0)
		SELECT @sql = @sql + ' AND scheduled_at < getdate() '

	IF @sortByColumn IS NOT NULL             
		SELECT @sql = @sql + ' ORDER BY ' + @sortByColumn
	IF @sortByColumn IS NOT NULL AND @sortOrder IS NOT NULL
		SELECT @sql = @sql + ' ' + @sortOrder

	IF @debug = 1
		PRINT @sql 

	SELECT @paramlist = ''

	CREATE TABLE #pagingTable
	(
		id int, 
		subsystem varchar(255), 
		fkid int, 
		scheduled_at datetime, 
		published_at datetime,
		[table] varchar(255),
		TempID int IDENTITY PRIMARY KEY
	)

	INSERT #pagingTable
		EXEC sp_executesql @sql, @paramlist

	DECLARE @AllRecords int
	SET @fromRecordIndex = @fromRecordIndex
	SET @AllRecords = (SELECT COUNT (ID) FROM #pagingTable)
	
	SELECT *, @AllRecords AS AllRecords
	FROM #pagingTable pT WHERE TempID between @fromRecordIndex AND @toRecordIndex
	DROP table #pagingTable

END

GO

ALTER PROCEDURE [dbo].[ListPagedComments]
	@fromRecordIndex int,
	@toRecordIndex int,
	@sortByColumn varchar(50)=null,
	@sortOrder varchar(50)=null,
	@changed bit=null,
	@publish bit=null,
	@contentId int=null,
	@languageId int=null,
	@debug bit=0
AS
BEGIN	
	DECLARE @sql        nvarchar(4000),
			@paramlist  nvarchar(4000)

	SELECT @sql =
    '	SELECT c.content_fk_id, c.email, c.[name], c.comment, c.commented_at, c.id, c.publish, c.changed, c.marked_for_deletion, ( select count(c2.id) FROM [dbo].[comments] c2 WHERE c2.id=c.id AND c2.publish=1) countPublished '

	if @languageId IS NOT NULL
		SELECT @sql = @sql + ', cds.title '
	ELSE
		SELECT @sql = @sql + ', '''' '
	
	SELECT @sql = @sql + ' FROM [dbo].[comments] c '

	if @languageId IS NOT NULL
		SELECT @sql = @sql + ' LEFT JOIN [dbo].[content_data_store] cds ON cds.content_fk_id=c.content_fk_id AND cds.language_fk_id=@xlanguageId '        

	SELECT @sql = @sql + 'WHERE 1=1'

	IF @contentId IS NOT NULL             
		SELECT @sql = @sql + ' AND content_fk_id = @xcontentId '

	IF @changed IS NOT NULL             
		SELECT @sql = @sql + ' AND changed = @xchanged '

	IF @publish IS NOT NULL             
		SELECT @sql = @sql + ' AND publish = @xpublish '

	IF @sortByColumn IS NOT NULL             
		SELECT @sql = @sql + ' ORDER BY ' + @sortByColumn
	IF @sortByColumn IS NOT NULL AND @sortOrder IS NOT NULL
		SELECT @sql = @sql + ' ' + @sortOrder

	IF @debug = 1
		PRINT @sql 

	SELECT @paramlist = '@xcontentId int,
						 @xpublish bit,
						 @xchanged bit,
						 @xlanguageId int'

	CREATE TABLE #pagingTable
	(
		TempID int IDENTITY PRIMARY KEY,
		content_fk_id int, 
		email varchar(255), 
		[name] nvarchar(255), 
		comment ntext, 
		commented_at datetime,
		commentId int,
		publish bit,
		changed bit,
		marked_for_deletion bit,
		countPublished int,
		title nvarchar(4000)
	)

	INSERT #pagingTable
		EXEC sp_executesql @sql, @paramlist,     
                   @contentId, @publish, @changed, @languageId

	SELECT
	content_fk_id, email, [name], comment, commented_at, commentId, publish, changed, marked_for_deletion, countPublished, title
	FROM
		#pagingTable
	WHERE 
		TempID >= @fromRecordIndex AND TempID <= @toRecordIndex

	select count(*) from #pagingTable

	drop table #pagingTable

END

GO



ALTER PROCEDURE [dbo].[ListPagedForms] 
@fromRecordIndex int, 
@toRecordIndex int,
@includeFormsWithoutTranslation bit,
@languageId int,
@sortBy varchar(255)=null, 
@sortDirection varchar(255)=null, 
@debug int=0
AS
	DECLARE @sql        nvarchar(4000),
			@paramlist  nvarchar(4000)

	SELECT @sql =
    'SELECT cds.title, cds.subtitle, cds.teaser, cds.html, c.principal_created_by, 
			c.date_created, c.principal_modified_by, c.date_modified, c.votes, c.score, 
			f.content_fk_id, f.id, f.form_type, f.send_to, 
		(SELECT COUNT(id) FROM [dbo].[nform_submission] WHERE nform_fk_id=f.id) cnt,
        (SELECT MIN(finished) FROM [dbo].[nform_submission] WHERE nform_fk_id=f.id) first_submission_date,
        (SELECT MAX(finished) FROM [dbo].[nform_submission] WHERE nform_fk_id=f.id) last_submission_date,
        allow_multiple_submissions,
        allow_modify_in_submission
     FROM [dbo].[nform] f 
	 INNER JOIN [dbo].[content] c ON c.id=f.content_fk_id '

	if (@includeFormsWithoutTranslation = 1)
		SELECT @sql = @sql + ' LEFT '
	ELSE
		SELECT @sql = @sql + ' INNER '

	SELECT @sql = @sql + ' JOIN [dbo].[content_data_store] cds ON cds.content_fk_id=f.content_fk_id AND cds.language_fk_id=@xlanguageId '

	IF @sortBy IS NOT NULL             
		SELECT @sql = @sql + ' ORDER BY ' + @sortBy
	IF @sortBy IS NOT NULL AND @sortDirection IS NOT NULL
		SELECT @sql = @sql + ' ' + @sortDirection

	IF @debug = 1
		PRINT @sql 

	SELECT @paramlist = '@xlanguageId int'

	CREATE TABLE #pagingTable
	(
		title nvarchar(4000),
		subtitle nvarchar(255),
		teaser ntext,
		html ntext,
		principal_created_by varchar(255),
		date_created datetime,
		principal_modified_by varchar(255),
		date_modified datetime,
		votes int,
		score float,
		content_fk_id int,
		id int,
		form_type varchar(255),
		send_to nvarchar(1000),
		submission_count int,
		first_submission_date datetime,
		last_submission_date datetime,
		allow_multiple_submissions bit,
        allow_modify_in_submission bit,
		TempID int IDENTITY PRIMARY KEY
	)

	INSERT #pagingTable
		EXEC sp_executesql @sql, @paramlist, @languageId

	DECLARE @AllRecords int
	SET @AllRecords = (SELECT COUNT (ID) FROM #pagingTable)

	SELECT *, @AllRecords
	FROM
		#pagingTable
	WHERE
		TempID >= @fromRecordIndex AND TempID <= @toRecordIndex

	DROP table #pagingTable
	

GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Page' AND [name] = 'SortableTables')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'Page', 'SortableTables', 'false', 'NORMAL')
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Login' AND [name] = 'ModuleMode')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'Login', 'ModuleMode', '0', 'NORMAL')
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Login' AND [name] = 'MembershipProvider')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'Login', 'MembershipProvider', 'SSOMembershipProvider', 'NORMAL')
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Form' AND [name] = 'UniqueTranslation')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'Form', 'UniqueTranslation', 'false', 'NORMAL')
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'FolderDocumentList' AND [name] = 'UniqueTranslation')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'FolderDocumentList', 'UniqueTranslation', 'false', 'NORMAL')

GO


IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Events' AND [name] = 'ShowTeaserImage')) = 0		
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('Bool', 'Events', 'ShowTeaserImage', 'false', 'NORMAL')

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Events' AND [name] = 'TeaserImageWidth')) = 0		
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('Int', 'Events', 'TeaserImageWidth', '150', 'NORMAL')
	
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Events' AND [name] = 'ShowTimesOnly')) = 0		
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('Bool', 'Events', 'ShowTimesOnly', 'false', 'NORMAL')	
	
	
ALTER PROCEDURE [dbo].[ListPagedArticles]
	@publishFlag bit,
	@languageId int,
	@offSet int = 0,
	@fromRecordIndex int,
	@toRecordIndex int,
	@sortByColumn varchar(255) = NULL,
	@sortOrder varchar(4) = NULL,
	@regularIds varchar(255)  = NULL,
	@dateFrom datetime  = NULL,
	@dateTo datetime = NULL,
	@showUntranslated bit = 0,
	@changed bit = null,
	@debug     bit          = 0
AS
BEGIN	
	CREATE TABLE #regularIDs (regularID int NOT NULL)
	CREATE TABLE #numbers (listpos int NOT NULL, number  int NOT NULL)
	EXEC [dbo].[Acc_IntList2Table] @regularIds
	INSERT #regularIDs  SELECT number FROM #numbers
	DROP TABLE #numbers

	DECLARE @sql        nvarchar(4000),
			@paramlist  nvarchar(4000)

	SELECT @sql = '	SELECT	cds.title, cds.subtitle, cds.teaser, cds.html, c.principal_created_by, c.date_created, 
				c.principal_modified_by, c.date_modified, c.votes, c.score, a.id, a.publish, a.content_fk_id,
				a.display_date, a.marked_for_deletion, a.changed, '

	IF (@publishFlag = 0 )
		SET @sql = @sql + ' ( select count(a2.id) FROM [dbo].[article] a2 WHERE a2.id=a.id AND a2.publish=1) countPublished '
	ELSE
		SET @sql = @sql + ' 1 '

	SET @sql = @sql + ' FROM [dbo].[article] a
		INNER JOIN [dbo].[content] c ON c.id=a.content_fk_id
		INNER JOIN [dbo].[regular_has_articles] ra ON ra.article_fk_id=a.id and ra.article_fk_publish=a.publish
	'

	IF (@showUntranslated = 0)
		SELECT @sql = @sql + '	INNER '
	ELSE	
		SELECT @sql = @sql + '	LEFT '

	SELECT @sql = @sql + ' JOIN [dbo].[content_data_store] cds ON cds.content_fk_id=c.id AND cds.language_fk_id=@xlanguageId '

	SELECT @sql = @sql + ' WHERE a.publish = ' + CAST(@publishFlag AS char(1))

	IF (SELECT COUNT (*) FROM #regularIDs) > 0
		SELECT @sql = @sql + 'AND ra.regular_fk_id IN (SELECT regularID FROM #regularIDs) ' 

	IF @dateFrom IS NOT NULL             
		SELECT @sql = @sql + ' AND a.display_date >= @xdateFrom '

	IF @dateTo IS NOT NULL             
		SELECT @sql = @sql + ' AND a.display_date <= @xdateTo '

	IF @changed IS NOT NULL
		SELECT @sql = @sql + ' AND a.changed = @xchanged '

	IF @sortByColumn IS NOT NULL             
		SELECT @sql = @sql + ' ORDER BY ' + @sortByColumn
	IF @sortByColumn IS NOT NULL AND @sortOrder IS NOT NULL
		SELECT @sql = @sql + ' ' + @sortOrder

	IF @debug = 1
		PRINT @sql 

	SELECT @paramlist = '@xlanguageId int,
						 @xdateFrom  datetime,
						 @xdateTo    datetime,
						 @xchanged   bit'

	CREATE TABLE #pagingTable
	(
		title nvarchar(4000),
		subtitle nvarchar(255),
		teaser ntext,
		html ntext,
		principal_created_by varchar(255),
		date_created datetime,
		principal_modified_by varchar(255),
		date_modified datetime,
		votes int,
		score float,
		article_id int,
		publish bit,
		content_id int,
		display_date datetime, 
		marked_for_deletion bit, 
		changed bit, 
		countPublished int,		
		ID int IDENTITY PRIMARY KEY,
	)

	INSERT #pagingTable
		EXEC sp_executesql @sql, @paramlist,     
                   @languageId, @dateFrom, @dateTo, @changed

	DECLARE @AllRecords int
	SET @fromRecordIndex = @fromRecordIndex + @offSet
	SET @AllRecords = (SELECT COUNT (ID) FROM #pagingTable)
	
	SELECT *, @AllRecords
	FROM #pagingTable pT 
	WHERE pT.ID between @fromRecordIndex AND @toRecordIndex 

	DROP table #pagingTable

END

GO


/**********************************************************************************/
/************************COMMENTS UPGRADE         *********************************/
/**********************************************************************************/
ALTER TABLE [dbo].[comments] ADD title nvarchar(255) NULL
GO

UPDATE [dbo].[comments] SET title=''
WHERE title is NULL
GO

ALTER TABLE [dbo].[comments] ALTER COLUMN title nvarchar(255) NOT NULL
GO

ALTER PROCEDURE [dbo].[ChangeComment]
	@id int out,
	@publish bit,
	@changed bit,
	@markedForDeletion bit,
	@contentId int,
	@comment ntext,
	@email varchar(255),
	@name nvarchar(255),
	@title nvarchar(255),
	@commentedAt datetime=null
AS
BEGIN
	SET NOCOUNT ON

	if @commentedAt is NULL
		SET @commentedAt = getdate()

	DECLARE @Exists int
	SELECT @Exists = COUNT(id) FROM comments WHERE id = @id AND publish = @publish

	IF (@Exists > 0)
	BEGIN
		UPDATE [dbo].[comments] 
			SET	comment=@comment, 
				[name]=@name,
				title=@title,
				email=@email,
				changed=@changed,
				marked_for_deletion=@markedForDeletion
			WHERE id=@id AND publish=@publish
	END
	ELSE
	BEGIN
		IF (@publish = 0)
		BEGIN
			EXEC nextval 'comments', @id out
		END

		INSERT INTO [dbo].[comments] 
		(id, publish, content_fk_id, comment, email, [name], changed, marked_for_deletion, commented_at, title) 
		VALUES 
		(@id, @publish, @contentId, @comment, @email, @name, @changed, @markedForDeletion, @commentedAt, @title);
	END
END
GO

ALTER PROCEDURE [dbo].[ListPagedComments]
	@fromRecordIndex int,
	@toRecordIndex int,
	@sortByColumn varchar(50)=null,
	@sortOrder varchar(50)=null,
	@changed bit=null,
	@publish bit=null,
	@contentId int=null,
	@debug bit=0
AS
BEGIN	
	DECLARE @sql        nvarchar(4000),
			@paramlist  nvarchar(4000)

	SELECT @sql =
    '	
		SELECT c.content_fk_id, c.email, c.[name], c.comment, c.commented_at, c.id, c.publish, c.changed, c.marked_for_deletion, ( select count(c2.id) FROM [dbo].[comments] c2 WHERE c2.id=c.id AND c2.publish=1) countPublished, c.title
		FROM [dbo].[comments] c 
		WHERE 1=1
	'

	IF @contentId IS NOT NULL             
		SELECT @sql = @sql + ' AND content_fk_id = @xcontentId '

	IF @changed IS NOT NULL             
		SELECT @sql = @sql + ' AND changed = @xchanged '

	IF @publish IS NOT NULL             
		SELECT @sql = @sql + ' AND publish = @xpublish '

	IF @sortByColumn IS NOT NULL             
		SELECT @sql = @sql + ' ORDER BY ' + @sortByColumn
	IF @sortByColumn IS NOT NULL AND @sortOrder IS NOT NULL
		SELECT @sql = @sql + ' ' + @sortOrder

	IF @debug = 1
		PRINT @sql 

	SELECT @paramlist = '@xcontentId int,
						 @xpublish bit,
						 @xchanged bit'

	CREATE TABLE #pagingTable
	(
		TempID int IDENTITY PRIMARY KEY,
		content_fk_id int, 
		email varchar(255), 
		[name] nvarchar(255), 
		comment ntext, 
		commented_at datetime,
		commentId int,
		publish bit,
		changed bit,
		marked_for_deletion bit,
		countPublished int,
		title nvarchar(255)
	)

	INSERT #pagingTable
		EXEC sp_executesql @sql, @paramlist,     
                   @contentId, @publish, @changed

	SELECT
	content_fk_id, email, [name], comment, commented_at, commentId, publish, changed, marked_for_deletion, countPublished, title
	FROM
		#pagingTable
	WHERE 
		TempID >= @fromRecordIndex AND TempID <= @toRecordIndex

	select count(*) from #pagingTable

	drop table #pagingTable

END
GO
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Events' AND [name] = 'NoOfDayToShow')) = 0		
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'Events', 'NoOfDayToShow', '7', 'NORMAL')

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'NewsSubscription' AND [name] = 'UniqueTranslation')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'NewsSubscription', 'UniqueTranslation', 'false', 'NORMAL')
	
GO

ALTER TABLE [dbo].[settings_list] ADD options varchar(1000) NULL
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'WebSite' AND [name] = 'RSSChannels')) = 0		
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('String', 'WebSite', 'RSSChannels', '', 'NORMAL')
GO

UPDATE [dbo].[settings_list] SET options = 'display_date;title;subtitle' WHERE subsystem = 'Article' AND [name] = 'SortByColumn' 
UPDATE [dbo].[settings_list] SET options = '0:Full;1:List' WHERE subsystem = 'Article' AND [name] = 'ModuleMode' 





IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'WebSite' AND [name] = 'RSSChannels')) = 0		
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('String', 'WebSite', 'RSSChannels', '', 'NORMAL')
GO

IF (SELECT Count(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='rss_feeds') = 0
BEGIN
	CREATE TABLE [dbo].[rss_feeds](
		[id] [int] IDENTITY(1,1) NOT NULL,
		[title] [nvarchar](255) NOT NULL,
		[description] [nvarchar](4000) NOT NULL,
		[type] [varchar](255) NOT NULL,
		[link_to_list] [varchar](255) NOT NULL,
		[link_to_single] [varchar](255) NOT NULL,
		[categories] [varchar](255) NULL,
		[language_fk_id] [int] NOT NULL,
	 CONSTRAINT [PK_rss_feeds] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[rss_feeds]  WITH CHECK ADD  CONSTRAINT [FK_rss_feeds_language] FOREIGN KEY([language_fk_id])
	REFERENCES [dbo].[language] ([id])
END
GO

IF EXISTS (select * from syscomments where id = object_id ('dbo.ListPagedRssFeeds'))
BEGIN
	drop procedure dbo.ListPagedRssFeeds 
END
GO

CREATE PROCEDURE [dbo].[ListPagedRssFeeds]
	@fromRecordIndex int,
	@toRecordIndex int,
	@languageId int,
	@sortByColumn varchar(50)=null,
	@sortOrder varchar(50)=null,
	@debug bit=0
AS
BEGIN	
	DECLARE @sql        nvarchar(4000),
			@paramlist  nvarchar(4000)

	SELECT @sql =
    '	
		SELECT rf.id, rf.title, rf.description, rf.type, rf.link_to_list, rf.link_to_single, rf.categories, rf.language_fk_id
		FROM [dbo].[rss_feeds] rf
		WHERE 1=1
	'

	SELECT @sql = @sql + ' AND rf.language_fk_id= @xlanguageId '


	IF @sortByColumn IS NOT NULL             
		SELECT @sql = @sql + ' ORDER BY ' + @sortByColumn
	IF @sortByColumn IS NOT NULL AND @sortOrder IS NOT NULL
		SELECT @sql = @sql + ' ' + @sortOrder

	IF (@debug = 1)
		PRINT @sql 

	SELECT @paramlist = '@xlanguageId int'

	CREATE TABLE #pagingTable
	(
		TempID int IDENTITY PRIMARY KEY,
		id int, 
		title nvarchar(255), 
		description nvarchar(4000), 
		[type] nvarchar(255), 
		link_to_list nvarchar(255),
		link_to_single nvarchar(255),
		categories nvarchar(255),
		language_fk_id int
	)

	INSERT #pagingTable
		EXEC sp_executesql @sql, @paramlist,     
                   @languageId

	SELECT id, title, description, [type], link_to_list, link_to_single, categories, language_fk_id
	FROM
		#pagingTable
	WHERE 
		TempID >= @fromRecordIndex AND TempID <= @toRecordIndex

	select count(*) from #pagingTable

	drop table #pagingTable
END

GO

ALTER PROCEDURE [dbo].[Acc_CharList2Table]
                    @list      ntext,
                    @delimiter char(1) = N',' AS

   DECLARE @pos      int,
           @textpos  int,
           @chunklen smallint,
           @tmpstr   nvarchar(4000),
           @leftover nvarchar(4000),
           @tmpval   nvarchar(4000)

   SET NOCOUNT ON

   SELECT @textpos = 1, @leftover = ''
   WHILE @textpos <= datalength(@list) / 2
   BEGIN
      SELECT @chunklen = 4000 - datalength(@leftover) / 2
      SELECT @tmpstr = @leftover + substring(@list, @textpos, @chunklen)
      SELECT @textpos = @textpos + @chunklen

      SELECT @pos = charindex(@delimiter, @tmpstr)

      WHILE @pos > 0
      BEGIN
         SELECT @tmpval = left(@tmpstr, @pos - 1)
         SELECT @tmpval = ltrim(rtrim(@tmpval))

         if ( len(@tmpval) > 0)
	         INSERT #strings(str) VALUES (@tmpval)
         SELECT @tmpstr = substring(@tmpstr, @pos + 1, len(@tmpstr))
         SELECT @pos = charindex(@delimiter, @tmpstr)
      END

      SELECT @leftover = @tmpstr
   END

   if ( len(@leftover) > 0)
	   INSERT #strings(str) VALUES(ltrim(rtrim(@leftover)))

GO


UPDATE [dbo].[settings_list] SET options = '1:CategoryList;2:SingleFaq;3:FaqList' WHERE subsystem = 'FrequentQuestions' AND [name] = 'ModuleMode'

GO

CREATE PROCEDURE [dbo].[settings_list_delete_setting]
	@subSystem varchar(255),
	@settingName varchar(255)
AS
BEGIN
	DECLARE @settingsListId int

	SELECT @settingsListId=id FROM settings_list
	WHERE [subsystem]=@subSystem AND [name]=@settingName

	IF (@settingsListId IS NOT NULL)
	BEGIN
		DELETE FROM web_site_settings WHERE settings_list_fk_id=@settingsListId
		DELETE FROM pages_settings WHERE settings_list_fk_id=@settingsListId
		DELETE FROM module_settings WHERE settings_list_fk_id=@settingsListId
		DELETE FROM settings_list WHERE id=@settingsListId		
	END
END
GO

settings_list_delete_setting 'TextContent', 'EnableImagePopup'
GO

ALTER PROCEDURE [dbo].[ListPagedNewsletterSubscriptions] 
@newsletterID int,
@fromRecordIndex int,
@toRecordIndex int,
@subscriptionType int,
@sortBy varchar(255)=null,
@sortOrder varchar(255)=null
AS

	DECLARE @sql        nvarchar(4000),
			@paramlist  nvarchar(4000)

	SELECT @sql = '	SELECT ns.id, newsletter_fk_id, email_fk_id, date_subscribed, date_unsubscribed, hash, date_confirmed, ip_confirmed, e.email 
					FROM [dbo].[newsletter_subscription] ns 
					INNER JOIN [dbo].[email] e ON e.id=ns.email_fk_id 
					WHERE ns.newsletter_fk_id=@xnewsletterID '

    if (@subscriptionType = 1)
	BEGIN
		SET @sql = @sql + ' AND date_subscribed IS NOT null AND date_confirmed IS null AND date_unsubscribed IS null'
	END
        ELSE IF (@subscriptionType = 2)
	BEGIN
		SET @sql = @sql + ' AND date_subscribed IS NOT null AND date_confirmed IS NOT null AND date_unsubscribed IS null'
	END
        ELSE IF (@subscriptionType = 3)
	BEGIN
		SET @sql = @sql + ' AND date_subscribed IS NOT null AND date_confirmed IS NOT null AND date_unsubscribed IS NOT null'
	END

	IF @sortBy IS NOT NULL             
		SELECT @sql = @sql + ' ORDER BY ' + @sortBy
	IF @sortBy IS NOT NULL AND @sortOrder IS NOT NULL
		SELECT @sql = @sql + ' ' + @sortOrder

	SELECT @paramlist = '@xnewsletterID int'

	CREATE TABLE #pagingTable 
	(	
		TempID int IDENTITY primary key, 
		id int, 
		newsletter_fk_id int, 	
		email_fk_id int, 
		date_subscribed datetime, 
		date_unsubscribed datetime, 
		hash varchar(10), 
		date_confirmed datetime, 
		ip_confirmed varchar(15), 
		email varchar(255)
	)

	INSERT #pagingTable
		EXEC sp_executesql @sql, @paramlist, @newsletterID

	SELECT
	 id, newsletter_fk_id, email_fk_id, date_subscribed, date_unsubscribed, hash, date_confirmed, ip_confirmed, email
	FROM
		#pagingTable
	WHERE
		TempID between @fromRecordIndex AND @toRecordIndex

	SELECT Count(*) SubCount from #pagingTable

	drop table #pagingTable

GO

ALTER PROCEDURE [dbo].[ListPagedFilteredEvents]
	@publishFlag bit,
	@languageId int,
	@fromRecordIndex int,
	@toRecordIndex int,
	@showUntranslated bit = 0,
	@sortByColumn varchar(255) = NULL,
	@sortOrder varchar(4) = NULL,
	@categoryIds varchar(255)  = NULL,
	@changed bit = NULL,
	@fromDate datetime = null,
	@toDate datetime = null,
	@filterSubTitle nvarchar(255) = null,
	@debug     bit          = 0
AS
BEGIN	
	CREATE TABLE #categoryIDs (categoryID int NOT NULL)
	CREATE TABLE #numbers (listpos int NOT NULL, number  int NOT NULL)
	EXEC [dbo].[Acc_IntList2Table] @categoryIds
	INSERT #categoryIDs  SELECT number FROM #numbers
	DROP TABLE #numbers

	DECLARE @sql        nvarchar(4000),
			@paramlist  nvarchar(4000)

	SELECT @sql = '	SELECT cds.title, cds.subtitle, cds.teaser, cds.html, c.principal_created_by, c.date_created, 
				c.principal_modified_by, c.date_modified, c.votes, c.score, e.id, e.publish, e.content_fk_id,
				e.begin_date, e.end_date, e.marked_for_deletion, e.changed, '
	IF (@publishFlag = 0)
		SELECT @sql = @sql + ' ( select count(e2.id) FROM [dbo].[event] e2 WHERE e2.id=e.id AND e2.publish=1) countPublished '
	ELSE
		SELECT @sql = @sql + ' 1 '
	
	SELECT @sql = @sql + ' FROM [dbo].[event] e
		INNER JOIN [dbo].[content] c ON c.id=e.content_fk_id
		INNER JOIN [dbo].[ucategorie_belongs_to] cbt ON cbt.fkid = e.id AND table_name = ''[dbo].[events]'' 
	'
	
	IF (@showUntranslated = 0)
		SELECT @sql = @sql + '	INNER '
	ELSE	
		SELECT @sql = @sql + '	LEFT '

	SELECT @sql = @sql + ' JOIN [dbo].[content_data_store] cds ON cds.content_fk_id=c.id AND cds.language_fk_id=@xlanguageId '

	SELECT @sql = @sql + ' WHERE publish = ' + CAST(@publishFlag AS char(1))  + ' '

	IF (SELECT COUNT (*) FROM #categoryIDs) > 0
		SELECT @sql = @sql + 'AND cbt.ucategories_fk_id IN (SELECT categoryID FROM #categoryIDs) ' 

	IF @changed IS NOT NULL             
		SELECT @sql = @sql + ' AND e.changed = @xchanged '

	IF @fromDate IS NOT NULL             
		SELECT @sql = @sql + ' AND e.begin_date >= @xfromDate '

	IF @toDate IS NOT NULL             
		SELECT @sql = @sql + ' AND e.begin_date <= @xtoDate '

	IF @filterSubTitle IS NOT NULL
		SELECT @sql = @sql + ' AND subtitle LIKE @xfilterSubTitle '

	IF @sortByColumn IS NOT NULL             
		SELECT @sql = @sql + ' ORDER BY ' + @sortByColumn
	IF @sortByColumn IS NOT NULL AND @sortOrder IS NOT NULL
		SELECT @sql = @sql + ' ' + @sortOrder

	IF @debug = 1
		PRINT @sql 

	SELECT @paramlist = '@xlanguageId int,
						 @xchanged bit,
						 @xfromDate datetime,
						 @xtoDate datetime,
						 @xfilterSubTitle nvarchar(255)'

	SET @filterSubTitle = '%' + @filterSubTitle + '%'

	CREATE TABLE #pagingTable
	(
		title nvarchar(4000),
		subtitle nvarchar(255),
		teaser ntext,
		html ntext,
		principal_created_by varchar(255),
		date_created datetime,
		principal_modified_by varchar(255),
		date_modified datetime,
		votes int,
		score float,
		event_id int,
		publish bit,
		content_id int,
		begin_date datetime, 
		end_date datetime, 
		marked_for_deletion bit, 
		changed bit, 
		countPublished int,
		ID int IDENTITY PRIMARY KEY
	)

	INSERT #pagingTable
		EXEC sp_executesql @sql, @paramlist,     
                   @languageId, @changed, @fromDate, @toDate, @filterSubTitle

	DECLARE @AllRecords int
	SET @AllRecords = (SELECT COUNT (ID) FROM #pagingTable)

	if (@toRecordIndex = 0)
	BEGIN
		SELECT *, @AllRecords
		FROM #pagingTable pT
	END
	ELSE
	BEGIN
		SELECT *, @AllRecords
		FROM #pagingTable pT
		WHERE pT.ID between @fromRecordIndex AND @toRecordIndex
	END

	DROP table #pagingTable

END

GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Directory' AND [name] = 'ExcludeMenuGroupsList')) = 0		
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('String', 'Directory', 'ExcludeMenuGroupsList', '98', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'TextContent' AND [name] = 'ImageTemplate')) = 0		
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('Int', 'TextContent', 'ImageTemplate', '0', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Article' AND [name] = 'ImageTemplate')) = 0		
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('Int', 'Article', 'ImageTemplate', '0', 'NORMAL')
GO

if ((SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'template' AND COLUMN_NAME = 'content') = 0)
BEGIN
	ALTER TABLE dbo.template ADD content  varchar(4000) NULL
END

settings_list_delete_setting 'TextContent', 'ImagePopupWidth'
GO

settings_list_delete_setting 'TextContent', 'ImagePopupHeight'
GO

settings_list_delete_setting 'Article', 'ImagePopupWidth'
GO

settings_list_delete_setting 'Article', 'ImagePopupHeight'
GO

settings_list_delete_setting 'Article', 'EnableImagePopup'
GO

settings_list_delete_setting 'TextContent', 'EnableImagePopup'
GO


-- **************************************************************************************************************************************
CREATE TABLE [dbo].[content_data_store_audit](
	[guid] [varchar](36) NOT NULL,
	[content_fk_id] [int] NOT NULL,
	[language_fk_id] [int] NOT NULL,
	[title] [nvarchar](4000) NULL,
	[subtitle] [nvarchar](255) NULL,
	[teaser] [ntext] NULL,
	[html] [ntext] NULL,
	[principal_saved_by] [varchar](255) NULL,
	[date_saved] [datetime] NULL CONSTRAINT [DF_content_data_store_audit_date_saved]  DEFAULT (getdate()),
 CONSTRAINT [PK_content_data_store_audit] PRIMARY KEY CLUSTERED 
(
	[guid] ASC
)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

CREATE NONCLUSTERED INDEX IX_content_data_store_audit ON dbo.[content_data_store_audit] ( content_fk_id, language_fk_id )
GO

IF (SELECT COUNT(guid) FROM [dbo].[content_data_store_audit]) = 0		
BEGIN
	DECLARE @Idd INT
	DECLARE ContentCursor CURSOR FOR SELECT idd FROM content_data_store ORDER by idd

	OPEN ContentCursor
	FETCH NEXT FROM ContentCursor INTO @Idd

	WHILE @@FETCH_STATUS = 0
	BEGIN

		INSERT INTO [dbo].[content_data_store_audit] (guid, language_fk_id, content_fk_id, title, subtitle, teaser, html, principal_saved_by)
		(SELECT newid(), language_fk_id, content_fk_id, title, subtitle, teaser, html, ISNULL(principal_modified_by, principal_created_by) pricipal
		FROM [dbo].[content_data_store] cds
		INNER JOIN [dbo].[content] c ON cds.content_fk_id=c.id
		WHERE idd=@Idd)

		FETCH NEXT FROM ContentCursor INTO @Idd
	END

	CLOSE ContentCursor
	DEALLOCATE ContentCursor
END
GO

CREATE NONCLUSTERED INDEX IX_content_data_store_audit_date_saved ON dbo.[content_data_store_audit] ( date_saved )
GO


IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'FlashFolderGallery' AND [name] = 'thumbnailColumns')) = 0		
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'FlashFolderGallery', 'thumbnailColumns', '3', 'NORMAL')
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'FlashFolderGallery' AND [name] = 'thumbnailRows')) = 0		
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'FlashFolderGallery', 'thumbnailRows', '3', 'NORMAL')
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'FlashFolderGallery' AND [name] = 'navPosition')) = 0		
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'FlashFolderGallery', 'navPosition', 'left', 'NORMAL')
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'FlashFolderGallery' AND [name] = 'imageWidth')) = 0		
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'FlashFolderGallery', 'imageWidth', '600', 'NORMAL')
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'FlashFolderGallery' AND [name] = 'imageThumbWidth')) = 0		
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'FlashFolderGallery', 'imageThumbWidth', '50', 'NORMAL')
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'WebSite' AND [name] = 'MenuGroupList')) = 0	
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('String', 'WebSite', 'MenuGroupList', '', 'NORMAL')
GO

-- ********************************************
UPDATE [dbo].[settings_list]
SET [type]='String'
WHERE [name]='NewsletterId' AND subsystem='NewsSubscription'

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Form' AND [name] = 'ShowValidationSummary')) = 0
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('Bool', 'Form', 'ShowValidationSummary', 'false', 'NORMAL')
	
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[faq_question]') AND type in (N'U'))
DROP TABLE [dbo].[faq_question]
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'NewsSubscription' AND [name] = 'ShowModuleTitle')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'NewsSubscription', 'ShowModuleTitle', 'true', 'NORMAL')
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_comments_content]') AND parent_object_id = OBJECT_ID(N'[dbo].[comments]'))
BEGIN
	ALTER TABLE [dbo].[comments]  DROP CONSTRAINT [FK_comments_content] 
END
ALTER TABLE [dbo].[comments]  WITH NOCHECK ADD  CONSTRAINT [FK_comments_content] FOREIGN KEY([content_fk_id])
REFERENCES [dbo].[content] ([id]) ON DELETE CASCADE
GO

ALTER PROCEDURE [dbo].[ListPagedFrequentQuestions]
@publishFlag bit,
	@languageId int,
	@fromRecordIndex int,
	@toRecordIndex int,
	@sortByColumn varchar(255) = NULL,
	@sortOrder varchar(4) = NULL,
	@categoryIds varchar(255)  = NULL,
	@showUntranslated bit = 0,
	@changed bit = NULL,
	@debug     bit          = 0
AS
BEGIN	
	CREATE TABLE #categoryIDs (categoryID int NOT NULL)
	CREATE TABLE #numbers (listpos int NOT NULL, number  int NOT NULL)
	EXEC [dbo].[Acc_IntList2Table] @categoryIds
	INSERT #categoryIDs  SELECT number FROM #numbers
	DROP TABLE #numbers

	DECLARE @sql        nvarchar(4000),
			@paramlist  nvarchar(4000)

	SELECT @sql =
    'SELECT cds.title, cds.subtitle, cds.teaser, cds.html, c.principal_created_by, c.date_created, 
		c.principal_modified_by, c.date_modified, c.votes, c.score,
		f.id, f.publish, f.changed, f.content_fk_id, f.marked_for_deletion, 
		( select count(fq2.id) FROM [dbo].[frequent_questions] fq2 WHERE fq2.id=f.id AND fq2.publish=1) countPublished,
		cbt.idx, cbt.ucategories_fk_id
	FROM [dbo].[frequent_questions] f
	INNER JOIN [dbo].[content] c ON c.id = f.content_fk_id '
	
	IF (@showUntranslated = 0)
		SELECT @sql = @sql + '	INNER '
	ELSE	
		SELECT @sql = @sql + '	LEFT '

	SELECT @sql = @sql + '	JOIN [dbo].[content_data_store] cds ON c.id = cds.content_fk_id 
		AND cds.language_fk_id = @xlanguageId
INNER JOIN [dbo].[ucategorie_belongs_to] cbt ON cbt.fkid = f.id AND table_name = ''[dbo].[faq_question]'' '

	SELECT @sql = @sql + 'WHERE publish = ' + CAST(@publishFlag AS char(1))  + ' '

	IF (SELECT COUNT (*) FROM #categoryIDs) > 0
		SELECT @sql = @sql + 'AND cbt.ucategories_fk_id IN (SELECT categoryID FROM #categoryIDs) ' 

--	SELECT @sql = @sql + 'AND e.begin_date >= [dbo].[DateOnly](getdate()) '

	IF @changed IS NOT NULL             
		SELECT @sql = @sql + ' AND f.changed = @xchanged '

	IF @sortByColumn IS NOT NULL             
		SELECT @sql = @sql + ' ORDER BY ' + @sortByColumn
	IF @sortByColumn IS NOT NULL AND @sortOrder IS NOT NULL
		SELECT @sql = @sql + ' ' + @sortOrder

	IF @debug = 1
		PRINT @sql 

	SELECT @paramlist = '@xlanguageId int,
						 @xchanged bit'

	CREATE TABLE #pagingTable
	(
		title nvarchar(4000),
		subtitle nvarchar(255),
		teaser ntext,
		html ntext,
		principal_created_by varchar(255),
		date_created datetime,
		principal_modified_by varchar(255),
		date_modified datetime,
		votes int,
		score float,		
		faq_id int,
		publish bit,
		changed bit,
		content_fk_id int, 
		marked_for_deletion bit, 
		countPublished int,
		idx int,
		ucategories_fk_id int,
		ID int IDENTITY PRIMARY KEY
	)

	INSERT #pagingTable
		EXEC sp_executesql @sql, @paramlist,     
                   @languageId, @changed

	DECLARE @AllRecords int
	SET @fromRecordIndex = @fromRecordIndex
	SET @AllRecords = (SELECT COUNT (ID) FROM #pagingTable)
	
	SELECT *, @AllRecords AS AllRecords
	FROM #pagingTable pT WHERE ID >= @fromRecordIndex AND ID <= @toRecordIndex
	DROP table #pagingTable

END
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[DeletePrefixedSPs]') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
BEGIN
	DROP PROCEDURE [dbo].[DeletePrefixedSPs]
END
GO

CREATE PROCEDURE [dbo].[DeletePrefixedSPs]
@prefix VARCHAR(100)
AS

	IF (len(@prefix) < 3 )
		RAISERROR('Prefix too short.', 16, 1)
	
	DECLARE @spname varchar(100)
	DECLARE @occurance int

	DECLARE GETSPNAME CURSOR FOR
	SELECT 	sysobjects.name
	FROM  sysobjects
	WHERE sysobjects.type = 'P' AND sysobjects.category=0
	ORDER BY sysobjects.name

	OPEN GETSPNAME 
	FETCH  NEXT FROM GETSPNAME into @spname
	WHILE @@FETCH_STATUS=0
	BEGIN
		SET @occurance = (SELECT CHARINDEX(@prefix,@spname))
		IF @occurance > 0
		BEGIN
			IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(@spname) AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
			BEGIN
				EXEC('DROP PROCEDURE [dbo].[' + @spname + ']')
				PRINT 'Dropped SP:' + @spname
			END
		END
		FETCH  NEXT FROM GETSPNAME into @spname
	END

	CLOSE GETSPNAME
	DEALLOCATE GETSPNAME
GO

IF (SELECT COUNT(id) FROM [dbo].[module] 
WHERE ([name] = 'FlashMovie')) = 0	
	INSERT INTO [dbo].[module] ( [name], module_source )
	VALUES ( 'FlashMovie', 'FlashMovie.ascx')

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'FlashMovie' AND [name] = 'Transparent')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Boolean', 'FlashMovie', 'Transparent', 'false', 'NORMAL')

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'FlashMovie' AND [name] = 'DimensionInPercent')) = 0
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('Bool', 'FlashMovie', 'DimensionInPercent', 'false', 'NORMAL')
