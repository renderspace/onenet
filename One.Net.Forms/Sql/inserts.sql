
-------- FORM

INSERT INTO [dbo].[module] ( [name], module_source )
VALUES ( 'Form', 'Form.ascx')

INSERT INTO [dbo].[settings_list]
([type], subsystem, [name], default_value, user_visibility)
VALUES ('Int', 'Form', 'FormId', '-1', 'NORMAL')
INSERT INTO [dbo].[settings_list]
([type], subsystem, [name], default_value, user_visibility)
VALUES ('Int', 'Form', 'UploadFolderId', '-1', 'NORMAL')
INSERT INTO [dbo].[settings_list]
([type], subsystem, [name], default_value, user_visibility)
VALUES ('Bool', 'Form', 'UseLabels', 'false', 'NORMAL')
-------- END FORM