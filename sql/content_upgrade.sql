ALTER PROCEDURE [dbo].[ListPagedFilteredEvents]
	@publishFlag bit,
	@languageId int,
	@fromRecordIndex int,
	@toRecordIndex int,
	@sortByColumn varchar(255) = NULL,
	@sortOrder varchar(4) = NULL,
	@categoryIds varchar(255)  = NULL,
	@includeArticlesWithoutTranslation bit = 0,
	@changed bit = NULL,
	@fromDate datetime = null,
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
		e.id, e.changed, e.marked_for_deletion, e.begin_date, e.end_date, cbt.ucategories_fk_id
	FROM [dbo].[event] e
	INNER JOIN [dbo].[content] c ON c.id = e.content_fk_id
	INNER JOIN [dbo].[content_data_store] cds ON c.id = cds.content_fk_id
	INNER JOIN [dbo].[ucategorie_belongs_to] cbt ON cbt.fkid = e.id AND table_name = ''[dbo].[events]'' '
	IF (@includeArticlesWithoutTranslation = 0)
		SELECT @sql = @sql + '	AND cds.language_fk_id = @xlanguageId '
	
	SELECT @sql = @sql + 'WHERE publish = ' + CAST(@publishFlag AS char(1))  + ' '

	IF (SELECT COUNT (*) FROM #categoryIDs) > 0
		SELECT @sql = @sql + 'AND cbt.ucategories_fk_id IN (SELECT categoryID FROM #categoryIDs) ' 

--	SELECT @sql = @sql + 'AND e.begin_date >= [dbo].[DateOnly](getdate()) '

	IF @changed IS NOT NULL             
		SELECT @sql = @sql + ' AND e.changed = @xchanged '

	IF @fromDate IS NOT NULL             
		SELECT @sql = @sql + ' AND e.begin_date >= [dbo].[DateOnly](@xfromDate) '

	IF @sortByColumn IS NOT NULL             
		SELECT @sql = @sql + ' ORDER BY ' + @sortByColumn
	IF @sortByColumn IS NOT NULL AND @sortOrder IS NOT NULL
		SELECT @sql = @sql + ' ' + @sortOrder

	IF @debug = 1
		PRINT @sql 

	SELECT @paramlist = '@xlanguageId int,
						 @xchanged bit,
						 @xfromDate datetime'

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
		changed bit, 
		marked_for_deletion bit, 
		begin_date datetime, 
		end_date datetime, 
		ucategories_fk_id int,
		ID int IDENTITY PRIMARY KEY
	)

	INSERT #pagingTable
		EXEC sp_executesql @sql, @paramlist,     
                   @languageId, @changed, @fromDate

	DECLARE @AllRecords int
	SET @fromRecordIndex = @fromRecordIndex
	SET @AllRecords = (SELECT COUNT (ID) FROM #pagingTable)
	
	SELECT *, @AllRecords AS AllRecords
	FROM #pagingTable pT WHERE ID >= @fromRecordIndex AND ID <= @toRecordIndex
	DROP table #pagingTable

END

GO


ALTER PROCEDURE [dbo].[ListPagedFrequentQuestions]
@publishFlag bit,
	@languageId int,
	@fromRecordIndex int,
	@toRecordIndex int,
	@sortByColumn varchar(255) = NULL,
	@sortOrder varchar(4) = NULL,
	@categoryIds varchar(255)  = NULL,
	@includeArticlesWithoutTranslation bit = 0,
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
		f.id, f.changed, f.marked_for_deletion, cbt.ucategories_fk_id, cbt.idx
	FROM [dbo].[frequent_questions] f
	INNER JOIN [dbo].[content] c ON c.id = f.content_fk_id
	INNER JOIN [dbo].[content_data_store] cds ON c.id = cds.content_fk_id
	INNER JOIN [dbo].[ucategorie_belongs_to] cbt ON cbt.fkid = f.id AND table_name = ''[dbo].[faq_question]'' '
	IF (@includeArticlesWithoutTranslation = 0)
		SELECT @sql = @sql + '	AND cds.language_fk_id = @xlanguageId '
	
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
		changed bit, 
		marked_for_deletion bit, 
		ucategories_fk_id int,
		idx int,
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



