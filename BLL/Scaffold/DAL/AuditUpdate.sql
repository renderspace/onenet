ALTER TABLE [TABLE_NAME] ADD principal_created [nvarchar](255) not null default '';
ALTER TABLE [TABLE_NAME] ADD principal_modified [nvarchar](255) not null default '';
ALTER TABLE [TABLE_NAME] ADD date_modified datetime null;
ALTER TABLE [TABLE_NAME] ADD date_created datetime not null default getdate();