DECLARE @id int;
DECLARE @publish bit;
DECLARE @changed bit;
DECLARE @markedForDeletion bit;
DECLARE @contentId int;
DECLARE @newArticleId int;
DECLARE @displayDate DATETIME;
DECLARE @newFaqRegularId INT;

-- NOTE: YOU NEED TO CREATE A NEW REGULAR FIRST ... BELOW IS IT'S ID VALUE.
SET @newFaqRegularId = 34;
SET @displayDate = getdate();

DECLARE db_cursor CURSOR FOR  
select id, publish, changed, marked_for_deletion, content_fk_id  from frequent_questions WHERE publish=0

OPEN db_cursor   
FETCH NEXT FROM db_cursor INTO @id, @publish, @changed, @markedForDeletion, @contentId

WHILE @@FETCH_STATUS = 0   
BEGIN   
	
	SET @newArticleId = NULL;
	EXEC ChangeArticle @publish, @changed, @contentId, @markedForDeletion, @displayDate, @newArticleId output;

	INSERT INTO regular_has_articles
	(article_fk_id, regular_fk_id, article_fk_publish)
	VALUES
	(@newArticleId, @newFaqRegularId, @publish);

	FETCH NEXT FROM db_cursor INTO @id, @publish, @changed, @markedForDeletion, @contentId
END   

CLOSE db_cursor   
DEALLOCATE db_cursor