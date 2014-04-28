

-- 1.7.1

if ((SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ucategories' AND COLUMN_NAME = 'is_private') = 0)
begin
	ALTER TABLE dbo.ucategories ADD
	is_private bit NOT NULL DEFAULT 0
END
GO

alter table files alter column mime_type varchar(255) not null
GO


-- 1.7.2

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'SearchCrawler' AND [name] = 'UseWebService')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'SearchCrawler', 'UseWebService', 'false', 'NORMAL')
GO

-- 1.7.3

UPDATE nform_answer SET is_fake=0
WHERE answer_type='Radio' AND is_fake=1

-- 1.7.4

IF (SELECT COUNT(id) FROM [dbo].[module] 
WHERE ([name] = 'ArticleCategoryFilter')) = 0	
	INSERT INTO [dbo].[module] ( [name], module_source )
	VALUES ( 'ArticleCategoryFilter', 'ArticleCategoryFilter.ascx')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticleCategoryFilter' AND [name] = 'RegularsList')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'ArticleCategoryFilter', 'RegularsList', '', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticleCategoryFilter' AND [name] = 'ArticleListUri')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'ArticleCategoryFilter', 'ArticleListUri', '', 'NORMAL')
GO


IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Article' AND [name] = 'ShowRegulars')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'Article', 'ShowRegulars', 'false', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[module] 
WHERE ([name] = 'ArticlesInCategory')) = 0	
	INSERT INTO [dbo].[module] ( [name], module_source )
	VALUES ( 'ArticlesInCategory', 'ArticlesInCategory.ascx')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticlesInCategory' AND [name] = 'CategoryId')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'ArticlesInCategory', 'CategoryId', '-1', 'NORMAL')
GO


IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticlesInCategory' AND [name] = 'LimitNoArticles')) = 0
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('Int', 'ArticlesInCategory', 'LimitNoArticles', '10', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticlesInCategory' AND [name] = 'SortByColumn')) = 0	
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('String', 'ArticlesInCategory', 'SortByColumn', 'display_date', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticlesInCategory' AND [name] = 'SortDescending')) = 0
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('Bool', 'ArticlesInCategory', 'SortDescending', 'false', 'NORMAL')
GO

-- 1.7.5

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Form' AND [name] = 'ShowModuleTitleWhenSubmitted')) = 0
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('Bool', 'Form', 'ShowModuleTitleWhenSubmitted', 'false', 'NORMAL')
GO

-- 1.7.6

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Article' AND [name] = 'ShowArticleCategoryDescription')) = 0		
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('Bool', 'Article', 'ShowArticleCategoryDescription', 'false', 'NORMAL')
GO

-- 1.7.7

IF (SELECT COUNT(id) FROM [dbo].[module] 
WHERE ([name] = 'Ratings')) = 0	
	INSERT INTO [dbo].[module] ( [name], module_source )
	VALUES ( 'Ratings', 'Ratings.ascx')
	
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Ratings' AND [name] = 'AuthenticatedOnly')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'Ratings', 'AuthenticatedOnly', 'false', 'NORMAL')
	
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Article' AND [name] = 'EnableRatingsProvider')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'Article', 'EnableRatingsProvider', 'false', 'NORMAL')
	
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticleCategoryFilter' AND [name] = 'ControlType')) = 0
	INSERT INTO [dbo].[settings_list] 
	([type], subsystem, [name], default_value, user_visibility, options) 
	VALUES ('Int', 'ArticleCategoryFilter', 'ControlType', '0', 'NORMAL', '1:DropDown;2:Repeater;')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Article' AND [name] = 'EnableContentIdProvider')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'Article', 'EnableContentIdProvider', 'false', 'NORMAL')
	
GO

-- 1.7.8

alter table content_data_store alter column teaser nvarchar(MAX) null
GO
alter table content_data_store alter column html nvarchar(MAX) null
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticleCategoryFilter' AND [name] = 'SortByColumn')) = 0
	INSERT INTO [dbo].[settings_list] 
	([type], subsystem, [name], default_value, user_visibility) 
	VALUES ('String', 'ArticleCategoryFilter', 'SortByColumn', 'id', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticleCategoryFilter' AND [name] = 'SortDescending')) = 0
	INSERT INTO [dbo].[settings_list] 
	([type], subsystem, [name], default_value, user_visibility) 
	VALUES ('Bool', 'ArticleCategoryFilter', 'SortDescending', 'true', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticleCategoryFilter' AND [name] = 'UniqueTranslation')) = 0
	INSERT INTO [dbo].[settings_list] 
	([type], subsystem, [name], default_value, user_visibility) 
	VALUES ('Bool', 'ArticleCategoryFilter', 'UniqueTranslation', 'false', 'NORMAL')
GO

-- 1.7.9
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Article' AND [name] = 'ShowMoreLink')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'Article', 'ShowMoreLink', 'true', 'NORMAL')
	
GO

IF (SELECT COUNT(id) FROM [dbo].[place_holder]
WHERE (place_holder_id = 'hidded')) = 0
	INSERT INTO [dbo].[place_holder] ( place_holder_class, place_holder_type, place_holder_id)
	VALUES ( '', 0, 'hidden')
GO

--1.8.0
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'FlashMovie' AND [name] = 'DisableRightClickMenu')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'FlashMovie', 'DisableRightClickMenu', 'false', 'NORMAL')
	
GO


IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'FrequentQuestions' AND [name] = 'FaqListUri')) = 0
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('String', 'FrequentQuestions', 'FaqListUri', '', 'NORMAL')
GO

--1.8.1

IF (SELECT COUNT(id) FROM [dbo].[module] 
WHERE ([name] = 'ArticleSlide')) = 0
BEGIN
	insert into module (name, module_source)
	values ('ArticleSlide', 'ArticleSlide.ascx')
END
Go

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticleSlide' AND [name] = 'SingleArticleUri')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'ArticleSlide', 'SingleArticleUri', '', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticleSlide' AND [name] = 'ArticleListUri')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'ArticleSlide', 'ArticleListUri', '', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticleSlide' AND [name] = 'DefaultImageUri')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'ArticleSlide', 'DefaultImageUri', '', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticleSlide' AND [name] = 'SortByColumn')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'ArticleSlide', 'SortByColumn', '', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticleSlide' AND [name] = 'RegularIds')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'ArticleSlide', 'RegularIds', '', 'NORMAL')
GO


IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticleSlide' AND [name] = 'SortDescending')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'ArticleSlide', 'SortDescending', 'false', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticleSlide' AND [name] = 'ThumbnailWidth')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'ArticleSlide', 'ThumbnailWidth', '100', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticleSlide' AND [name] = 'ArticleCount')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'ArticleSlide', 'ArticleCount', '10', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticleSlide' AND [name] = 'UniqueTranslation')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'ArticleSlide', 'UniqueTranslation', 'false', 'NORMAL')
GO

-- 1.8.2

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Article' AND [name] = 'ShowCommentCount')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'Article', 'ShowCommentCount', 'false', 'NORMAL')
GO

-- 1.8.3
-- CKeditor added

-- 1.8.4

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

	SELECT @sql = '	SELECT DISTINCT cds.title, cds.subtitle, cds.teaser, cds.html, c.principal_created_by, c.date_created, 
				c.principal_modified_by, c.date_modified, c.votes, c.score, a.id, a.publish, a.content_fk_id,
				a.display_date, a.marked_for_deletion, a.changed, '

	IF (@publishFlag = 0 )
		SET @sql = @sql + ' ( select count(a2.id) FROM [dbo].[article] a2 WHERE a2.id=a.id AND a2.publish=1) countPublished, '
	ELSE
		SET @sql = @sql + ' 1, '

	SELECT @sql = @sql + ' ( select count(ct.id) FROM [dbo].[comments] ct WHERE ct.content_fk_id=a.content_fk_id AND ct.publish=a.publish) commentCount '

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
		SELECT @sql = @sql + ' AND ra.regular_fk_id IN (SELECT regularID FROM #regularIDs) ' 

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
		teaser nvarchar(max),
		html nvarchar(max),
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
		commentCount int,		
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

-- 1.8.5 redirects table

CREATE TABLE [dbo].[redirects](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[from_link] [varchar](256) NOT NULL,
	[to_link] [varchar](256) NOT NULL,
 CONSTRAINT [PK_redirects] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

-- 1.8.6 increase of value from varchar 255 to 4000
ALTER TABLE [dbo].[module_settings] ALTER COLUMN value varchar(MAX) NULL
ALTER TABLE [dbo].[pages_settings] ALTER COLUMN value varchar(MAX) NULL

GO

ALTER PROCEDURE [dbo].[ChangeModuleInstanceSetting]
	@Id int,
	@PublishFlag int,
	@Name varchar(255),
	@Value varchar(MAX),
	@SubSystem varchar(255)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @SettingId int
	SELECT @SettingId = id FROM settings_list WHERE [name] = @Name AND subsystem=@SubSystem

	DECLARE @Exists int
	SELECT @Exists = COUNT(module_instance_fk_pages_fk_publish) FROM [dbo].[module_settings]
		WHERE module_instance_fk_id = @Id AND module_instance_fk_pages_fk_publish = @PublishFlag 
			AND settings_list_fk_id = @SettingId

	IF (@Exists > 0)
	BEGIN
		UPDATE [dbo].[module_settings] SET [value] = @Value
		WHERE module_instance_fk_id = @Id AND module_instance_fk_pages_fk_publish = @PublishFlag 
			 AND settings_list_fk_id = @SettingId
	END
	ELSE
	BEGIN
		INSERT INTO [dbo].[module_settings]
		(module_instance_fk_id,  module_instance_fk_pages_fk_publish, settings_list_fk_id, [value])
		VALUES
		(@Id, @PublishFlag, @SettingId, @Value)
	END
END
GO

ALTER PROCEDURE [dbo].[ChangeWebSiteSetting] 
	@Id int,
	@Name varchar(255),
	@Value varchar(MAX)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @SettingId int
	SELECT @SettingId = id FROM settings_list WHERE [name] = @Name

	DECLARE @Exists int
	SELECT @Exists = COUNT(settings_list_fk_id) FROM [dbo].[web_site_settings]
		WHERE web_site_fk_id = @Id AND settings_list_fk_id = @SettingId

	IF (@Exists > 0)
	BEGIN
		UPDATE [dbo].[web_site_settings] SET [value] = @Value
		WHERE web_site_fk_id = @Id AND settings_list_fk_id = @SettingId
	END
	ELSE
	BEGIN
		INSERT INTO [dbo].[web_site_settings]
		(web_site_fk_id,  settings_list_fk_id, [value])
		VALUES
		(@Id, @SettingId, @Value)
	END
END
GO

ALTER PROCEDURE [dbo].[ChangePageSetting] 
	@Id int,
	@PublishFlag int,
	@Name varchar(255),
	@Value varchar(MAX)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @SettingId int
	SELECT @SettingId = id FROM settings_list WHERE [name] = @Name and subsystem='Page'

	DECLARE @Exists int
	SELECT @Exists = COUNT(settings_list_fk_id) FROM pages_settings 
		WHERE pages_fk_id = @Id AND pages_fk_publish = @PublishFlag AND settings_list_fk_id = @SettingId

	IF (@Exists > 0)
	BEGIN
		UPDATE [dbo].[pages_settings] SET [value] = @Value
		WHERE pages_fk_id = @Id AND pages_fk_publish = @PublishFlag AND settings_list_fk_id = @SettingId
	END
	ELSE
	BEGIN
		INSERT INTO [dbo].[pages_settings]
		(pages_fk_id,  pages_fk_publish, settings_list_fk_id, [value])
		VALUES
		(@Id, @PublishFlag, @SettingId, @Value)
	END
END


-- 1.8.7 Flasmovie upgrades

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'FlashMovie' AND [name] = 'AllowFullScreen')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'FlashMovie', 'AllowFullScreen', 'false', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'FlashMovie' AND [name] = 'AllowScriptAccess')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'FlashMovie', 'AllowScriptAccess', 'false', 'NORMAL')
GO


-- 1.8.7

CREATE TABLE [dbo].[event_location](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[lcid] [int] NOT NULL,
	[name] [nvarchar](255) NOT NULL,
	[additional_info] [ntext] NULL,
 CONSTRAINT [PK_event_location] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO

if ((SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'event' AND COLUMN_NAME = 'event_location_fk_id') = 0)
begin
	ALTER TABLE dbo.event ADD event_location_fk_id int NULL
END
GO

ALTER TABLE [dbo].[event]  WITH CHECK ADD  CONSTRAINT [FK_event_event_location] FOREIGN KEY([event_location_fk_id])
REFERENCES [dbo].[event_location] ([id])
GO

ALTER TABLE [dbo].[event] CHECK CONSTRAINT [FK_event_event_location]
GO

ALTER PROCEDURE [dbo].[ChangeEvent]
	@publishFlag bit,
	@changed bit,
	@contentId int,
	@markedForDeletion bit,
	@begin_date datetime,
	@end_date datetime=null,
	@locationId int=null,
	@id int=null output
AS

	IF ( @id is null )
	BEGIN
		EXEC [dbo].[nextval] 'event', @id out		

		INSERT INTO [dbo].[event] ( id, publish, changed, content_fk_id, marked_for_deletion, begin_date, end_date, event_location_fk_id)
		VALUES (@id, @publishFlag, @changed, @contentId, @markedForDeletion, @begin_date, @end_date, @locationId)
	END
	ELSE
	BEGIN
		DECLARE @Exists int
		SELECT @Exists = count(*) FROM [dbo].[event] WHERE id=@id AND publish=@publishFlag

		IF ( @Exists = 0 )
		BEGIN
			INSERT INTO [dbo].[event] ( id, publish, changed, content_fk_id, marked_for_deletion, begin_date, end_date, event_location_fk_id)
			VALUES (@id, @publishFlag, @changed, @contentId, @markedForDeletion, @begin_date, @end_date, @locationId)
		END
		ELSE
		BEGIN
			UPDATE [dbo].[event]
			SET changed=@changed, content_fk_id=@contentID, marked_for_deletion=@markedForDeletion, begin_date = @begin_date, end_date = @end_date, event_location_fk_id=@locationId
			WHERE id=@id AND publish=@publishFlag
		END
	END

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
	@locationIdFilter int = null,
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
	
	SELECT @sql = @sql + ', e.event_location_fk_id location_id, el.lcid location_lcid, el.name location_name '
	
	SELECT @sql = @sql + ' FROM [dbo].[event] e
		INNER JOIN [dbo].[content] c ON c.id=e.content_fk_id
		INNER JOIN [dbo].[ucategorie_belongs_to] cbt ON cbt.fkid = e.id AND table_name = ''[dbo].[events]'' 
	'
	
	IF (@showUntranslated = 0)
		SELECT @sql = @sql + '	INNER '
	ELSE	
		SELECT @sql = @sql + '	LEFT '

	SELECT @sql = @sql + ' JOIN [dbo].[content_data_store] cds ON cds.content_fk_id=c.id AND cds.language_fk_id=@xlanguageId '
	
	SELECT @sql = @sql + ' LEFT JOIN [dbo].[event_location] el ON el.id=e.event_location_fk_id '

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

	IF @locationIdFilter IS NOT NULL
		SELECT @sql = @sql + ' AND e.event_location_fk_id = @xlocationIdFilter '
		
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
						 @xfilterSubTitle nvarchar(255),
						 @xlocationIdFilter int'

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
		location_id int, 
		location_lcid int, 
		location_name nvarchar(255),
		ID int IDENTITY PRIMARY KEY
	)

	INSERT #pagingTable
		EXEC sp_executesql @sql, @paramlist,     
                   @languageId, @changed, @fromDate, @toDate, @filterSubTitle, @locationIdFilter

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








IF (SELECT COUNT(id) FROM [dbo].[module] 
WHERE ([name] = 'EventLocations')) = 0	
	INSERT INTO [dbo].[module] ( [name], module_source )
	VALUES ( 'EventLocations', 'EventLocations.ascx')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'EventLocations' AND [name] = 'ShowModuleTitle')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'EventLocations', 'ShowModuleTitle', 'true', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'EventLocations' AND [name] = 'SortDescending')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'EventLocations', 'SortDescending', 'true', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'EventLocations' AND [name] = 'SortByColumn')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'EventLocations', 'SortByColumn', '[name]', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'EventLocations' AND [name] = 'SingleLocationUri')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'EventLocations', 'SingleLocationUri', '', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'EventLocations' AND [name] = 'LocationLcid')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'EventLocations', 'LocationLcid', '1060', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[module] 
WHERE ([name] = 'EventLocationsMap')) = 0	
	INSERT INTO [dbo].[module] ( [name], module_source )
	VALUES ( 'EventLocationsMap', 'EventLocationsMap.ascx')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'EventLocationsMap' AND [name] = 'ShowModuleTitle')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'EventLocationsMap', 'ShowModuleTitle', 'true', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'EventLocationsMap' AND [name] = 'SortDescending')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'EventLocationsMap', 'SortDescending', 'true', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'EventLocationsMap' AND [name] = 'SortByColumn')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'EventLocationsMap', 'SortByColumn', '[name]', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'EventLocationsMap' AND [name] = 'SingleLocationUri')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'EventLocationsMap', 'SingleLocationUri', '', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'EventLocationsMap' AND [name] = 'LocationLcid')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Int', 'EventLocationsMap', 'LocationLcid', '1060', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'EventLocationsMap' AND [name] = 'MapImageUri')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'EventLocationsMap', 'MapImageUri', '', 'NORMAL')
GO

-- 1.8.9

IF (SELECT COUNT(id) FROM [dbo].[module] 
WHERE ([name] = 'AuthorInfo')) = 0	
	INSERT INTO [dbo].[module] ( [name], module_source )
	VALUES ( 'AuthorInfo', 'AuthorInfo.ascx')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Article' AND [name] = 'EnableAuthorInfoProvider')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'Article', 'EnableAuthorInfoProvider', 'false', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'AuthorInfo' AND [name] = 'ShowDescription')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'AuthorInfo', 'ShowDescription', 'true', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'AuthorInfo' AND [name] = 'ShowImage')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'AuthorInfo', 'ShowImage', 'true', 'NORMAL')
GO



CREATE TABLE [dbo].[author_info](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](255) NOT NULL,
	[title] [nvarchar](255) NULL,
	[description] [ntext] NOT NULL,
	[image_uri] [nvarchar](255) NULL,
	[profile_uri] [nvarchar](255) NULL,	
 CONSTRAINT [PK_author_info] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

CREATE TABLE [dbo].[author_info_has_content](
	[content_fk_id] [int] NOT NULL,
	[author_info_fk_id] [int] NOT NULL,
 CONSTRAINT [PK_author_info_has_content] PRIMARY KEY CLUSTERED 
(
	[content_fk_id] ASC,
	[author_info_fk_id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[author_info_has_content]  WITH CHECK ADD  CONSTRAINT [FK_author_info_has_content_author_info] FOREIGN KEY([author_info_fk_id])
REFERENCES [dbo].[author_info] ([id])
GO

ALTER TABLE [dbo].[author_info_has_content] CHECK CONSTRAINT [FK_author_info_has_content_author_info]
GO

ALTER TABLE [dbo].[author_info_has_content]  WITH CHECK ADD  CONSTRAINT [FK_author_info_has_content_content] FOREIGN KEY([content_fk_id])
REFERENCES [dbo].[content] ([id])
GO

ALTER TABLE [dbo].[author_info_has_content] CHECK CONSTRAINT [FK_author_info_has_content_content]
GO

-- 1.9.0.0

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Events' AND [name] = 'EnableEventIdProvider')) = 0		
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('Bool', 'Events', 'EnableEventIdProvider', 'false', 'NORMAL')	
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Events' AND [name] = 'ShowTeaserImage')) = 0		
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('Bool', 'Events', 'ShowTeaserImage', 'false', 'NORMAL')	
GO

-- 1.9.0.1

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'FolderDocumentList' AND [name] = 'SortDescending')) = 0		
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('Bool', 'FolderDocumentList', 'SortDescending', 'false', 'NORMAL')	
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'FolderDocumentList' AND [name] = 'SortByColumn')) = 0		
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility, options)
	VALUES ('String', 'FolderDocumentList', 'SortByColumn', 'f.id', 'NORMAL', 'f.id;f.name;title;teaser;f.created')	
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Article' AND [name] = 'EnableLeadImageProvider')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'Article', 'EnableLeadImageProvider', 'false', 'NORMAL')
	
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Events' AND [name] = 'EnableLeadImageProvider')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'Events', 'EnableLeadImageProvider', 'false', 'NORMAL')
	
GO

-- 1.9.0.3	
	IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Article' AND [name] = 'EnablePageNameProvider')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'Article', 'EnablePageNameProvider', 'true', 'NORMAL')
	
GO	

	IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Article' AND [name] = 'EnableArticleIdProvider')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'Article', 'EnableArticleIdProvider', 'false', 'NORMAL')
	
GO	

---- 1.9.04

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
	@locationIdFilter int = null,
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
	
	SELECT @sql = @sql + ', e.event_location_fk_id location_id, el.lcid location_lcid, el.name location_name '
	
	SELECT @sql = @sql + ' FROM [dbo].[event] e
		INNER JOIN [dbo].[content] c ON c.id=e.content_fk_id
		INNER JOIN [dbo].[ucategorie_belongs_to] cbt ON cbt.fkid = e.id AND table_name = ''[dbo].[events]'' 
	'
	
	IF (@showUntranslated = 0)
		SELECT @sql = @sql + '	INNER '
	ELSE	
		SELECT @sql = @sql + '	LEFT '

	SELECT @sql = @sql + ' JOIN [dbo].[content_data_store] cds ON cds.content_fk_id=c.id AND cds.language_fk_id=@xlanguageId '
	
	SELECT @sql = @sql + ' LEFT JOIN [dbo].[event_location] el ON el.id=e.event_location_fk_id '

	SELECT @sql = @sql + ' WHERE publish = ' + CAST(@publishFlag AS char(1))  + ' '

	IF (SELECT COUNT (*) FROM #categoryIDs) > 0
		SELECT @sql = @sql + 'AND cbt.ucategories_fk_id IN (SELECT categoryID FROM #categoryIDs) ' 

	IF @changed IS NOT NULL             
		SELECT @sql = @sql + ' AND e.changed = @xchanged '

	IF @fromDate IS NOT NULL             
		SELECT @sql = @sql + ' AND (YEAR(e.begin_date) = YEAR(@xfromDate) AND MONTH(e.begin_date) = MONTH(@xfromDate) AND DAY(e.begin_date) >= DAY(@xfromDate)) '

	IF @toDate IS NOT NULL             
		SELECT @sql = @sql + ' AND (YEAR(e.begin_date) = YEAR(@xtoDate) AND MONTH(e.begin_date) = MONTH(@xtoDate) AND DAY(e.begin_date) <= DAY(@xtoDate)) '

	IF @filterSubTitle IS NOT NULL
		SELECT @sql = @sql + ' AND subtitle LIKE @xfilterSubTitle '

	IF @locationIdFilter IS NOT NULL
		SELECT @sql = @sql + ' AND e.event_location_fk_id = @xlocationIdFilter '
		
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
						 @xfilterSubTitle nvarchar(255),
						 @xlocationIdFilter int'

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
		location_id int, 
		location_lcid int, 
		location_name nvarchar(255),
		ID int IDENTITY PRIMARY KEY
	)

	INSERT #pagingTable
		EXEC sp_executesql @sql, @paramlist,     
                   @languageId, @changed, @fromDate, @toDate, @filterSubTitle, @locationIdFilter

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

--- 1.9.5

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'WebSite' AND [name] = 'DefaultOgImage')) = 0	
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('String', 'WebSite', 'DefaultOgImage', '', 'NORMAL')
GO


IF (SELECT COUNT(id) FROM [dbo].[module] 
WHERE ([name] = 'ArticlesDropDownFilter')) = 0	
	INSERT INTO [dbo].[module] ( [name], module_source )
	VALUES ( 'ArticlesDropDownFilter', 'ArticlesDropDownFilter.ascx')
GO


IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticlesDropDownFilter' AND [name] = 'ShowModuleTitle')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'ArticlesDropDownFilter', 'ShowModuleTitle', 'true', 'NORMAL')
	
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticlesDropDownFilter' AND [name] = 'ArticleListUri')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'ArticlesDropDownFilter', 'ArticleListUri', '', 'NORMAL')
	
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticlesDropDownFilter' AND [name] = 'RegularsList')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'ArticlesDropDownFilter', 'RegularsList', '', 'NORMAL')
	
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticlesDropDownFilter' AND [name] = 'RssLink')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'ArticlesDropDownFilter', 'RssLink', '', 'NORMAL')
	
GO

--- 1.9.6

IF (SELECT COUNT(id) FROM [dbo].[module] 
WHERE ([name] = 'NajdiSearch')) = 0	
	INSERT INTO [dbo].[module] ( [name], module_source )
	VALUES ( 'NajdiSearch', 'NajdiSearch.ascx')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'NajdiSearch' AND [name] = 'SearchUri')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'NajdiSearch', 'SearchUri', '', 'NORMAL')

GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticlesDropDownFilter' AND [name] = 'UniqueTranslation')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'ArticlesDropDownFilter', 'UniqueTranslation', 'true', 'NORMAL')
	
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'NajdiSearch' AND [name] = 'SearchDomain')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'NajdiSearch', 'SearchDomain', '', 'NORMAL')
	
GO


IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'IFrame' AND [name] = 'PassGetParameters')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'IFrame', 'PassGetParameters', 'false', 'NORMAL')
	
GO

-- 1.9.7
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'WebSite' AND [name] = 'Headline')) = 0
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('String', 'WebSite', 'Headline', '', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'WebSite' AND [name] = 'CondensedPageTitle')) = 0
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('Bool', 'WebSite', 'CondensedPageTitle', 'false', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'WebSite' AND [name] = 'MaxPageTitleDepth')) = 0
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('Int', 'WebSite', 'MaxPageTitleDepth', '6', 'NORMAL')
GO

--- 1.9.8

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Page' AND [name] = 'OgTitle')) = 0	
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('String', 'Page', 'OgTitle', '', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticleSlide' AND [name] = 'ShowTeaser')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'ArticleSlide', 'ShowTeaser', 'false', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticleSlide' AND [name] = 'ShowMoreLink')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'ArticleSlide', 'ShowMoreLink', 'false', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticlesInCategory' AND [name] = 'SingleArticleUri')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'ArticlesInCategory', 'SingleArticleUri', '', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticlesInCategory' AND [name] = 'ArticleListUri')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('String', 'ArticlesInCategory', 'ArticleListUri', '', 'NORMAL')
GO
--- 1.9.9
	IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Article' AND [name] = 'EnableDefaultArticleIdProvider')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'Article', 'EnableDefaultArticleIdProvider', 'false', 'NORMAL')
	
GO	

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticleSlide' AND [name] = 'ShowTeaserImage')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'ArticleSlide', 'ShowTeaserImage', 'true', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'ArticleSlide' AND [name] = 'ShowSubTitle')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'ArticleSlide', 'ShowSubTitle', 'false', 'NORMAL')
GO

--- 1.9.91


IF (SELECT COUNT(id) FROM [dbo].[module] 
WHERE ([name] = 'LightboxWrapper')) = 0	
	INSERT INTO [dbo].[module] ( [name], module_source )
	VALUES ( 'LightboxWrapper', 'LightboxWrapper.ascx')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'LightboxWrapper' AND [name] = 'LightboxInstanceId')) = 0
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('Int', 'LightboxWrapper', 'LightboxInstanceId', '0', 'NORMAL')
GO


--- 2.0.0

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Page' AND [name] = 'PageOgDescription')) = 0	
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('String', 'Page', 'PageOgDescription', '', 'NORMAL')
GO

--- 2.0.2
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'SocialBookmark' AND [name] = 'ShowFacebook')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'SocialBookmark', 'ShowFacebook', 'true', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'SocialBookmark' AND [name] = 'ShowTwitter')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'SocialBookmark', 'ShowTwitter', 'true', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'SocialBookmark' AND [name] = 'ShowMySpace')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'SocialBookmark', 'ShowMySpace', 'true', 'NORMAL')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'SocialBookmark' AND [name] = 'ShowDelicious')) = 0
	insert into [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	values ('Bool', 'SocialBookmark', 'ShowDelicious', 'true', 'NORMAL')
GO

--- 2.0.3


ALTER TABLE dbo.redirects ADD
	created datetime NOT NULL CONSTRAINT DF_redirects_created DEFAULT getdate(),
	is_shortener bit NOT NULL CONSTRAINT DF_redirects_is_shortener DEFAULT 0
GO

--- 2.0.4

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'Page' AND [name] = 'PageOgImage')) = 0	
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('String', 'Page', 'PageOgImage', '', 'NORMAL')
GO

-- 2.0.5
IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'WebSite' AND [name] = 'CustomHeadJs')) = 0	
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('String', 'WebSite', 'CustomHeadJs', '', 'MULTILINE')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'WebSite' AND [name] = 'CustomBodyJs')) = 0	
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('String', 'WebSite', 'CustomBodyJs', '', 'MULTILINE')
GO

-- 2.0.6

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'WebSite' AND [name] = 'GAEnableCookieConsent')) = 0	
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('Bool', 'WebSite', 'GAEnableCookieConsent', '', 'true')
GO

-- 2.0.7

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'WebSite' AND [name] = 'GAConsentDefaultDisabled')) = 1	
	DELETE FROM [dbo].[settings_list] WHERE subsystem = 'WebSite' AND [name] = 'GAConsentDefaultDisabled'
GO

-- 2.0.7.2

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
        allow_modify_in_submission,
		completion_redirect
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
		completion_redirect nvarchar(255),
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

if ((SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'nform' AND COLUMN_NAME = 'completion_redirect') = 0)
begin
	ALTER TABLE dbo.nform ADD
	completion_redirect nvarchar (255) NULL
END
GO

if ((SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'nform_section' AND COLUMN_NAME = 'on_client_click') = 0)
begin
	ALTER TABLE dbo.nform_section ADD
	on_client_click nvarchar (255) NULL
END
GO

-- 2.0.7.3

IF (SELECT COUNT(id) FROM [dbo].[module] 
WHERE ([name] = 'NavigatePages')) = 0	
	INSERT INTO [dbo].[module] ( [name], module_source )
	VALUES ( 'NavigatePages', 'NavigatePages.ascx')
GO

IF (SELECT COUNT(id) FROM [dbo].[settings_list]
WHERE (subsystem = 'NavigatePages' AND [name] = 'IsPrevNavigation')) = 0	
	INSERT INTO [dbo].[settings_list]
	([type], subsystem, [name], default_value, user_visibility)
	VALUES ('Bool', 'NavigatePages', 'IsPrevNavigation', 'false', 'NORMAL')
GO