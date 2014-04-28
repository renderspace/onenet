DECLARE @AccountID INT
DECLARE @getAccountID CURSOR
SET @getAccountID = CURSOR FOR

SELECT id FROM pages ORDER BY id DESC

OPEN @getAccountID
FETCH NEXT FROM @getAccountID INTO @AccountID
WHILE @@FETCH_STATUS = 0
BEGIN	
	DELETE FROM pages WHERE id = @AccountID
FETCH NEXT FROM @getAccountID INTO @AccountID
END
CLOSE @getAccountID
DEALLOCATE @getAccountID

-- DELETE FROM content WHERE id > 1