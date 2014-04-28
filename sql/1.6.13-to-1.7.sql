
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'WebSite' AND [name] = 'MenuGroupList')) = 0	
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('String', 'WebSite', 'MenuGroupList', '', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[module] 
WHERE ([name] = 'ArticleSearch')) = 0	
	INSERT INTO [dbo].[module] ( [name], module_source )
	VALUES ( 'ArticleSearch', 'ArticleSearch.ascx')
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticleSearch' AND [name] = 'RegularsList')) = 0
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('String', 'ArticleSearch', 'RegularsList', '', 'NORMAL')

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticleSearch' AND [name] = 'LimitNoArticles')) = 0
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('Int', 'ArticleSearch', 'LimitNoArticles', '10', 'NORMAL')

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticleSearch' AND [name] = 'SortByColumn')) = 0
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('String', 'ArticleSearch', 'SortByColumn', 'display_date', 'NORMAL')

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticleSearch' AND [name] = 'SingleArticleUri')) = 0
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('String', 'ArticleSearch', 'SingleArticleUri', '', 'NORMAL')

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticleSearch' AND [name] = 'SortDescending')) = 0
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('Bool', 'ArticleSearch', 'SortDescending', 'false', 'NORMAL')

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticleSearch' AND [name] = 'ShowPager')) = 0
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('Bool', 'ArticleSearch', 'ShowPager', 'false', 'NORMAL')
	
UPDATE [dbo].[settings_list]
SET [type]='String'
WHERE [name]='NewsletterId' AND subsystem='NewsSubscription'

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Form' AND [name] = 'ShowValidationSummary')) = 0
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('Bool', 'Form', 'ShowValidationSummary', 'false', 'NORMAL')
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[faq_question]') AND type in (N'U'))
DROP TABLE [dbo].[faq_question]
GO	

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Form' AND [name] = 'ShowSectionTitle')) = 0
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('Bool', 'Form', 'ShowSectionTitle', 'true', 'NORMAL')
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

exec settings_list_delete_setting 'ImageGallery', 'ImageWidth'
exec settings_list_delete_setting 'ImageGallery', 'ImageHeight'
exec settings_list_delete_setting 'ImageGallery', 'PopupTemplateId'


insert into [dbo].[settings_list]
([type], subsystem, [name], default_value, user_visibility)
values ('Int', 'ImageGallery', 'ImageTemplate', '-1', 'NORMAL')


-- 1.6.17
-- changed dictionary.. improved UX, removed tree organization

DELETE FROM ucategorie_belongs_to WHERE id IN (
	SELECT ct.id FROM ucategories c
	INNER JOIN ucategorie_belongs_to ct ON ct.ucategories_fk_id = c.id
	WHERE ucategorie_type = 'tree_dictionary'
)

DELETE FROM ucategories WHERE ucategorie_type = 'tree_dictionary'

-- 1.6.18
-- extended Image module with new setting 

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Image' AND [name] = 'OpenInNewWindow')) = 0	
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('Bool', 'Image', 'OpenInNewWindow', 'false', 'NORMAL')
GO

-- 1.6.19
-- changed existing Login module (probably not used on Mercator) removed a ModuleMode setting for login module
-- added PasswordRecovery module
-- added ChangePassword module
-- big changes when fixing publishing for multilanguage and timed publish
-- added UserRegistration module
-- added special content module

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Login' AND [name] = 'PasswordRecoveryPage')) = 0	
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('String', 'Login', 'PasswordRecoveryPage', '', 'NORMAL')
GO

exec settings_list_delete_setting 'Login', 'ModuleMode'

IF (SELECT COUNT(id) FROM [dbo].[module] 
WHERE ([name] = 'PasswordRecovery')) = 0	
	INSERT INTO [dbo].[module] ( [name], module_source )
	VALUES ( 'PasswordRecovery', 'PasswordRecovery.ascx')

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'PasswordRecovery' AND [name] = 'ChangePasswordPage')) = 0	
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('String', 'PasswordRecovery', 'ChangePasswordPage', '', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[module] 
WHERE ([name] = 'ChangePassword')) = 0	
	INSERT INTO [dbo].[module] ( [name], module_source )
	VALUES ( 'ChangePassword', 'ChangePassword.ascx')

IF (SELECT COUNT(id) FROM [dbo].[module] 
WHERE ([name] = 'UserRegistration')) = 0	
	INSERT INTO [dbo].[module] ( [name], module_source )
	VALUES ( 'UserRegistration', 'UserRegistration.ascx')

IF (SELECT COUNT(id) FROM [dbo].[module] 
WHERE ([name] = 'SpecialContent')) = 0	
	INSERT INTO [dbo].[module] ( [name], module_source )
	VALUES ( 'SpecialContent', 'SpecialContent.ascx')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'SpecialContent' AND [name] = 'ContentId')) = 0	
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('Int', 'SpecialContent', 'ContentId', '-1', 'SPECIAL')
GO

-- 1.6.20 
-- Changes to login module

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Login' AND [name] = 'DestinationPageUrl')) = 0	
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('String', 'Login', 'DestinationPageUrl', '', 'NORMAL')
GO

-- 1.6.21
-- Removed "change primary language" from structure.aspx due to bugs and unusability.

-- 1.6.22
-- Added XHTML validator

-- 1.6.23
-- added width and height to flashfoldergallery

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'FlashFolderGallery' AND [name] = 'Width')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'FlashFolderGallery', 'Width', '500', 'NORMAL')
	
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'FlashFolderGallery' AND [name] = 'Height')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'FlashFolderGallery', 'Height', '500', 'NORMAL')
	
-- 1.6.24
-- flashmovie now takes parameters as param1:value;param2:value

-- 1.6.25
-- Image handler now does fixed aspect ratio resize only

-- 1.6.26
-- FolderGallery is using imagetemplate now

exec settings_list_delete_setting 'FolderGallery', 'ImageWidth'
exec settings_list_delete_setting 'FolderGallery', 'ImageHeight'

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'FolderGallery' AND [name] = 'ImageTemplate')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'FolderGallery', 'ImageTemplate', '-1', 'NORMAL')
	
-- missing from previous upgrades, non-esential updates for galery

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


-- 1.6.27
-- Added Galleriffic module
-- Added Socialbookmark  module
-- keyword == meaning bug fixed
-- added purple color for non-translation
-- translation of magic-button is now obligatory

IF (SELECT COUNT(id) FROM [dbo].[module] 
WHERE ([name] = 'Galleriffic')) = 0	
	INSERT INTO [dbo].[module] ( [name], module_source )
	VALUES ( 'Galleriffic', 'Galleriffic.ascx')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Galleriffic' AND [name] = 'FolderId')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'Galleriffic', 'FolderId', '0', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Galleriffic' AND [name] = 'RecordsPerPage')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'Galleriffic', 'RecordsPerPage', '9', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Galleriffic' AND [name] = 'EnableDownloadOriginal')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'Galleriffic', 'EnableDownloadOriginal', 'true', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Galleriffic' AND [name] = 'SlideShowDelay')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'Galleriffic', 'SlideShowDelay', '3', 'NORMAL')
GO


DECLARE @moduleName varchar(256)
SET @moduleName = 'SocialBookmark'

IF NOT EXISTS (SELECT id FROM [dbo].[module] WHERE [name] = @moduleName)
	INSERT INTO [dbo].[module] ( [name], [module_source] ) VALUES (@moduleName, @moduleName + '.ascx')

IF NOT EXISTS (SELECT id FROM [dbo].[settings_list] WHERE (subsystem = @moduleName AND [name] = 'MySpaceLocation'))
	INSERT INTO [dbo].[settings_list] ([type], subsystem, [name], default_value, user_visibility, options) VALUES ('Int', @moduleName, 'MySpaceLocation', '0', 'NORMAL', '1:Blog;2:Bulletin;3:About;4:Meet;5:Interests;6:Music;7:Movies;8:Television;9:Books;10:Heroes')

-- 1.6.28
-- Added ArticleDateFilter module
-- modified Article module to accept query param adfd (e.g. adfd=2009-12) and combine in function with ArticleDataFilter
-- added cacheLockingArticleGet to BArticle
-- For Galleriffic removed Height settings and renamed widths to ImageMax and ThumbMax
IF (SELECT COUNT(id) FROM [dbo].[module] 
WHERE ([name] = 'ArticleDateFilter')) = 0	
	INSERT INTO [dbo].[module] ( [name], module_source )
	VALUES ( 'ArticleDateFilter', 'ArticleDateFilter.ascx')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticleDateFilter' AND [name] = 'ShowArticleCount')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'ArticleDateFilter', 'ShowArticleCount', 'false', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticleDateFilter' AND [name] = 'ArticleListUri')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'ArticleDateFilter', 'ArticleListUri', '', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticleDateFilter' AND [name] = 'RegularsList')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'ArticleDateFilter', 'RegularsList', '', 'NORMAL')
GO

-- Gallerific fixes as mentioned above in description for 1.6.28
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Galleriffic' AND [name] = 'ImageWidth')) = 1
	UPDATE [dbo].[settings_list]
	SET [name]='ImageMax'
	WHERE subsystem='Galleriffic' AND [name]='ImageWidth'
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Galleriffic' AND [name] = 'ThumbWidth')) = 1
	UPDATE [dbo].[settings_list]
	SET [name]='ThumbMax'
	WHERE subsystem='Galleriffic' AND [name]='ThumbWidth'
GO

settings_list_delete_setting 'Galleriffic', 'ImageHeight'
GO
settings_list_delete_setting 'Galleriffic', 'ThumbHeight'
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Galleriffic' AND [name] = 'ImageMax')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'Galleriffic', 'ImageMax', '400', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Galleriffic' AND [name] = 'ThumbMax')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'Galleriffic', 'ThumbMax', '100', 'NORMAL')
GO

-- 1.6.29
-- new feature SQL running engine


-- 1.6.30 change size of mime_type field.
ALTER TABLE [dbo].[files] ALTER COLUMN mime_type [varchar](255) NOT NULL
GO

-- 1.6.31 textcontentmodule insatnce added caching

-- 1.6.32 FlashMovie setting Transparent (WMODE)
IF (SELECT COUNT(id) FROM [dbo].[module] 
WHERE ([name] = 'FlashMovie')) = 0	
	INSERT INTO [dbo].[module] ( [name], module_source )
	VALUES ( 'FlashMovie', 'FlashMovie.ascx')

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'FlashMovie' AND [name] = 'Transparent')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'FlashMovie', 'Transparent', 'false', 'NORMAL')
	
-- 1.6.33
-- added XHTML validator changes

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Comments' AND [name] = 'AuthenticatedOnly')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'Comments', 'AuthenticatedOnly', 'false', 'NORMAL')
	
GO

--- optional fix for gallerific

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Galleriffic' AND [name] = 'ImageMax')) > 1
	exec settings_list_delete_setting 'Galleriffic', 'ImageMax'
GO
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Galleriffic' AND [name] = 'ThumbMax')) > 1
	exec settings_list_delete_setting 'Galleriffic', 'ThumbMax'
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Galleriffic' AND [name] = 'ImageMax')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'Galleriffic', 'ImageMax', '400', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Galleriffic' AND [name] = 'ThumbMax')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'Galleriffic', 'ThumbMax', '100', 'NORMAL')
GO

-- 1.6.34 FlashMovie setting DimensionInPercent
IF (SELECT COUNT(id) FROM [dbo].[module] 
WHERE ([name] = 'FlashMovie')) = 0	
	INSERT INTO [dbo].[module] ( [name], module_source )
	VALUES ( 'FlashMovie', 'FlashMovie.ascx')

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'FlashMovie' AND [name] = 'DimensionInPercent')) = 0
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('Bool', 'FlashMovie', 'DimensionInPercent', 'false', 'NORMAL')

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Events' AND [name] = 'OffSet')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'Events', 'OffSet', '0', 'NORMAL')	
