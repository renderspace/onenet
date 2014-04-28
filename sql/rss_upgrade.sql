IF EXISTS (select * from syscomments where id = object_id ('dbo.ListArticlesForRss'))
BEGIN
	DROP PROCEDURE dbo.ListArticlesForRss 
END
GO

IF EXISTS (select * from syscomments where id = object_id ('dbo.ListArticleCategoriesForRss'))
BEGIN
	DROP PROCEDURE dbo.ListArticleCategoriesForRss 
END
GO

IF EXISTS (select * from syscomments where id = object_id ('dbo.ListEventCategoriesForRss'))
BEGIN
	DROP PROCEDURE dbo.ListEventCategoriesForRss 
END
GO

IF EXISTS (select * from syscomments where id = object_id ('dbo.ListEventsForRss'))
BEGIN
	DROP PROCEDURE dbo.ListEventsForRss 
END
GO

CREATE PROCEDURE [dbo].[ListArticlesForRss]
	@languageId int,
	@categoryIds varchar(255)  = NULL
AS
BEGIN
	CREATE TABLE #regularIDs (regularID int NOT NULL)
	CREATE TABLE #numbers (listpos int NOT NULL, number  int NOT NULL)
	EXEC [dbo].[Acc_IntList2Table] @categoryIds
	INSERT #regularIDs  SELECT number FROM #numbers
	DROP TABLE #numbers

	DECLARE @sql        nvarchar(4000),
			@paramlist  nvarchar(4000)

	SELECT @sql = '	SELECT DISTINCT Top(10) a.id, a.content_fk_id, display_date pubDate '
	SET @sql = @sql + ' FROM [dbo].[article] a
		INNER JOIN [dbo].[content] c ON c.id=a.content_fk_id
		INNER JOIN [dbo].[regular_has_articles] ra ON ra.article_fk_id=a.id and ra.article_fk_publish=1
	'

	SELECT @sql = @sql + ' INNER JOIN [dbo].[content_data_store] cds ON cds.content_fk_id=c.id AND cds.language_fk_id=@xlanguageId '
	SELECT @sql = @sql + ' WHERE a.publish = 1 ' 

	IF (SELECT COUNT (*) FROM #regularIDs) > 0
		SELECT @sql = @sql + 'AND ra.regular_fk_id IN (SELECT regularID FROM #regularIDs) ' 

	SELECT @sql = @sql + ' ORDER BY a.display_date DESC '

	SELECT @paramlist = '@xlanguageId int'

	-- temp table is required for these rss SPs because if you don't use a temp table you get the following error
	-- Msg 421, Level 16, State 1, Line 1
	-- The ntext data type cannot be selected as DISTINCT because it is not comparable.
	CREATE TABLE #pagingTable
	(
		article_id int,
		content_fk_id int,
		display_date datetime, 
		ID int IDENTITY PRIMARY KEY,
	)

	INSERT #pagingTable
		EXEC sp_executesql @sql, @paramlist,     
                   @languageId

	SELECT article_id, cds.title, cds.subtitle, cds.teaser, cds.html, display_date pubDate
	FROM #pagingTable pT 
	INNER JOIN [dbo].[content_data_store] cds ON cds.content_fk_id=pT.content_fk_id AND cds.language_fk_id=@languageId

	DROP TABLE #pagingTable
	DROP TABLE #regularIDs
END
GO







CREATE PROCEDURE [dbo].[ListArticleCategoriesForRss]
	@languageId int
AS
BEGIN
	SELECT r.id, cds.title FROM [dbo].[regular] r
	INNER JOIN [dbo].[content] c ON c.id=r.content_fk_id
	INNER JOIN [dbo].[content_data_store] cds ON cds.content_fk_id=c.id AND cds.language_fk_id=@languageId
	ORDER BY cds.title ASC
END	
GO




CREATE PROCEDURE [dbo].[ListEventCategoriesForRss]
	@languageId int
AS
BEGIN
	SELECT uc.id, cds.title FROM [dbo].[ucategories] uc
	INNER JOIN [dbo].[content] c ON c.id=uc.content_fk_id
	INNER JOIN [dbo].[content_data_store] cds ON cds.content_fk_id=c.id AND cds.language_fk_id=@languageId
	WHERE ucategorie_type='tree_n:m_event_categorization'
	ORDER BY cds.title ASC 
END	
GO





CREATE PROCEDURE [dbo].[ListEventsForRss]
	@languageId int,
	@categoryIds ntext = NULL
AS
BEGIN
	CREATE TABLE #catIDs (catID int NOT NULL)
	CREATE TABLE #numbers (listpos int NOT NULL, number  int NOT NULL)
	EXEC [dbo].[Acc_IntList2Table] @categoryIds
	INSERT #catIDs SELECT number FROM #numbers
	DROP TABLE #numbers

	DECLARE @sql        nvarchar(4000),
			@paramlist  nvarchar(4000)

	SELECT @sql = '	SELECT DISTINCT Top(10) e.id, e.content_fk_id, begin_date display_date '
	SET @sql = @sql + ' FROM [dbo].[event] e
		INNER JOIN [dbo].[content] c ON c.id=e.content_fk_id
		INNER JOIN [dbo].[ucategorie_belongs_to] ubt ON ubt.fkid = e.id AND ubt.table_name = ''[dbo].[events]'' and e.publish=1
		INNER JOIN [dbo].[content_data_store] cds ON cds.content_fk_id=c.id AND cds.language_fk_id=@xlanguageId '
	SELECT @sql = @sql + ' WHERE e.publish = 1 ' 

	IF (SELECT COUNT (*) FROM #catIds) > 0
		SELECT @sql = @sql + 'AND ubt.ucategories_fk_id IN (SELECT catID FROM #catIds) ' 

	SELECT @sql = @sql + ' ORDER BY e.begin_date DESC '

	SELECT @paramlist = '@xlanguageId int'

	-- temp table is required for these rss SPs because if you don't use a temp table you get the following error
	-- Msg 421, Level 16, State 1, Line 1
	-- The ntext data type cannot be selected as DISTINCT because it is not comparable.	
	CREATE TABLE #pagingTable
	(
		event_id int,
		content_fk_id int,
		display_date datetime, 
		ID int IDENTITY PRIMARY KEY,
	)

	INSERT #pagingTable
		EXEC sp_executesql @sql, @paramlist,     
                   @languageId

	SELECT event_id, cds.title, cds.subtitle, cds.teaser, cds.html, display_date
	FROM #pagingTable pT 
	INNER JOIN [dbo].[content_data_store] cds ON cds.content_fk_id=pT.content_fk_id AND cds.language_fk_id=@languageId

	DROP TABLE #pagingTable
	DROP TABLE #catIds
END	
GO

GRANT EXECUTE ON [dbo].[ListArticlesForRss] TO [One.Net.BackEnd]
GRANT EXECUTE ON [dbo].[ListArticleCategoriesForRss] TO [One.Net.BackEnd]
GRANT EXECUTE ON [dbo].[ListEventCategoriesForRss] TO [One.Net.BackEnd]
GRANT EXECUTE ON [dbo].[ListEventsForRss] TO [One.Net.BackEnd]