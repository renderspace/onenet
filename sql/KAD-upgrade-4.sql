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

/**********************************************************************************/
/************************COMMENTS UPGRADE         *********************************/
/**********************************************************************************/

DECLARE @comments_update_title bit
SELECT @comments_update_title = 0

IF (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='dbo' AND
TABLE_NAME='comments' AND COLUMN_NAME='title') = 0
begin
	ALTER TABLE [dbo].[comments] ADD title nvarchar(255) NULL
	SELECT @comments_update_title = 1
END

if (@comments_update_title = 1)
BEGIN
	PRINT 'comments_update_title'
	UPDATE [dbo].[comments] SET title=''
	WHERE title is NULL
	
	ALTER TABLE [dbo].[comments] ALTER COLUMN title nvarchar(255) NOT NULL
END
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
	
	
-- KAD ONLY: 
GO
EXEC sp_fulltext_table 'content_data_store', 'drop'
GO
ALTER TABLE [dbo].[content_data_store] ALTER COLUMN title nvarchar(4000) NULL
GO
ALTER TABLE [dbo].[content_data_store] ALTER COLUMN subtitle nvarchar(255) NULL
GO
ALTER TABLE [dbo].[int_link] ALTER COLUMN par_link_name nvarchar(250) NULL
GO

-- updated SOF, mercator


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

GO

ALTER TABLE [dbo].[settings_list] ADD options varchar(1000) NULL
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

GO

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

-- ***************************************************************** DO NOT ADD BELOW THIS LINE. THIS FILE IS LOCKED.
-- ***************************************************************** DO NOT ADD BELOW THIS LINE. THIS FILE IS LOCKED.
-- ***************************************************************** DO NOT ADD BELOW THIS LINE. THIS FILE IS LOCKED.
-- ***************************************************************** DO NOT ADD BELOW THIS LINE. THIS FILE IS LOCKED.
-- ***************************************************************** DO NOT ADD BELOW THIS LINE. THIS FILE IS LOCKED.