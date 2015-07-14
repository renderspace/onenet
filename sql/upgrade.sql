--- 2.x version....

ALTER PROCEDURE [dbo].[ChangeWebSiteSetting] 
	@Id int,
	@Name varchar(255),
	@Value varchar(MAX)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @SettingId int
	SELECT @SettingId = id FROM settings_list WHERE [name] = @Name AND subsystem = 'Website'

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




 update [dbo].[settings_list] set type='ImageTemplate' WHERE name = 'ImageTemplate' AND subsystem in ('TextContent', 'ImageGallery', 'Article')