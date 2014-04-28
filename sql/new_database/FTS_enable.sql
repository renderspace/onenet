use KAD
GO
exec sp_fulltext_database 'enable'
GO
exec sp_fulltext_catalog 'KAD', 'create', 'C:\Program Files\Microsoft SQL Server\MSSQL.1\MSSQL\FTData\'
GO
exec sp_fulltext_table 'content_data_store', 'create', 'KAD', 'IX_content_data_store' 
GO
exec sp_fulltext_column 'content_data_store', 'title', 'add'
GO
exec sp_fulltext_column 'content_data_store', 'subtitle', 'add'
GO 
exec sp_fulltext_column 'content_data_store', 'teaser', 'add'
GO 
exec sp_fulltext_column 'content_data_store', 'html', 'add'
GO
exec sp_fulltext_table 'content_data_store', 'activate'
GO
exec sp_fulltext_catalog 'KAD', 'start_full'
go


exec sp_fulltext_table 'content_data_store', 'start_change_tracking'
go
exec sp_fulltext_table 'content_data_store', 'start_background_updateindex' 
go