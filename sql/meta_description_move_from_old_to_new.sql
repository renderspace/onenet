
-- select * from settings_list where subsystem = 'Page'





DECLARE @publish bit
DECLARE @page_id int
DECLARE @value nVARCHAR(500)

DECLARE @old_value nVARCHAR(500)
DECLARE @content_id int 


DECLARE db_cursor CURSOR FOR  
select pages_fk_publish, pages_fk_id, value from MladinskaKnjiga..pages_settings where settings_list_fk_id = 6 AND NOT value = ''

OPEN db_cursor   
FETCH NEXT FROM db_cursor INTO @publish, @page_id, @value

WHILE @@FETCH_STATUS = 0   
BEGIN   
       SELECT @old_value = teaser, @content_id = p.content_fk_id 
	   FROM MladinskaKnjigaNew..pages p
	   INNER JOIN MladinskaKnjigaNew..content_data_store cds ON cds.content_fk_id = p.content_fk_id
	   WHERE p.id = @page_id AND p.publish = @publish

	   if ( @old_value <> '')
	   BEGIN
		PRINT @old_value
	   END
	   ELSE
	   BEGIN
	    -- UPDATE MladinskaKnjigaNew..content_data_store SET teaser = @value WHERE content_fk_id = @content_id
		PRINT @content_id
	   END

       FETCH NEXT FROM db_cursor INTO @publish, @page_id, @value
END   

CLOSE db_cursor   
DEALLOCATE db_cursor