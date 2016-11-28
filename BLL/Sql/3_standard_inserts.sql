
USE [sistem2] 
GO

------ WEBSITE

INSERT INTO [dbo].[language] (id) VALUES ( 1060 )
INSERT INTO [dbo].[language] (id) VALUES ( 1033 )
INSERT INTO [dbo].[language] (id) VALUES ( 1279 )	
INSERT INTO [dbo].[language] (id) VALUES ( 1050 )
INSERT INTO [dbo].[language] (id) VALUES ( 2074 )
INSERT INTO [dbo].[language] (id) VALUES ( 5146 )
INSERT INTO [dbo].[language] (id) VALUES ( 1031 )
INSERT INTO [dbo].[language] (id) VALUES ( 1040 )
GO

INSERT INTO [dbo].[sequences] (seq, sequence_id) VALUES ( 'pages', 0)
INSERT INTO [dbo].[sequences] (seq, sequence_id) VALUES ( 'moduleinstances', 0)
INSERT INTO [dbo].[sequences] (seq, sequence_id) VALUES ( 'articles', 0)
GO

INSERT INTO [dbo].[place_holder] ( place_holder_class, place_holder_type, place_holder_id)
VALUES ( 'centerContainer', 0, 'centerContainer')
INSERT INTO [dbo].[place_holder] ( place_holder_class, place_holder_type, place_holder_id)
VALUES ( 'leftContainer', 0, 'leftContainer')
INSERT INTO [dbo].[place_holder] ( place_holder_class, place_holder_type, place_holder_id)
VALUES ( 'rightContainer', 0, 'rightContainer')
GO


INSERT INTO [dbo].[template] (template_type, [name])
VALUES (3, 'Demo');
GO

INSERT INTO [dbo].[settings_list]
([type], subsystem, [name], default_value, user_visibility)
VALUES ('String', 'WebSite', 'GoogleAnalyticsWebPropertyID', 'UA-xxxx-x', 'NORMAL')

INSERT INTO [dbo].[settings_list]
([type], subsystem, [name], default_value, user_visibility)
VALUES ('Bool', 'WebSite', 'GoogleAnalyticsConsent', 'false', 'NORMAL')

INSERT INTO [dbo].[settings_list]
([type], subsystem, [name], default_value, user_visibility)
VALUES ('CSInteger', 'WebSite', 'RSSChannels', '', 'NORMAL')

INSERT INTO [dbo].[settings_list]
([type], subsystem, [name], default_value, user_visibility)
VALUES ('Url', 'WebSite', 'PreviewUrl', '', 'NORMAL')

INSERT INTO [dbo].[settings_list]
([type], subsystem, [name], default_value, user_visibility)
VALUES ('Url', 'WebSite', 'ProductionUrl', '', 'NORMAL')

INSERT INTO [dbo].[settings_list]
([type], subsystem, [name], default_value, user_visibility)
VALUES ('Int', 'WebSite', 'FacebookApplicationID', '', 'NORMAL')

INSERT INTO [dbo].[settings_list]
([type], subsystem, [name], default_value, user_visibility)
VALUES ('Int', 'WebSite', 'PrimaryLanguageId', '1060', 'SPECIAL')

INSERT INTO [dbo].[settings_list]
([type], subsystem, [name], default_value, user_visibility)
VALUES ('String', 'WebSite', 'MetaKeywords', '', 'NORMAL')

INSERT INTO [dbo].[settings_list]
([type], subsystem, [name], default_value, user_visibility)
VALUES ('String', 'WebSite', 'GoogleSiteVerification', '', 'NORMAL')

INSERT INTO [dbo].[settings_list]
([type], subsystem, [name], default_value, user_visibility)
VALUES ('Image', 'WebSite', 'Favicon', '', 'NORMAL')

INSERT INTO [dbo].[settings_list]
([type], subsystem, [name], default_value, user_visibility)
VALUES ('Bool', 'WebSite', 'AdvertisingFeatures', 'false', 'NORMAL')


INSERT INTO [dbo].[settings_list]
([type], subsystem, [name], default_value, user_visibility)
VALUES ('Bool', 'WebSite', 'IsMobile', 'false', 'NORMAL')

INSERT INTO [dbo].[settings_list]
([type], subsystem, [name], default_value, user_visibility)
VALUES ('String', 'WebSite', 'MobileMedia', 'only screen and (max-width: 640px)', 'NORMAL')

INSERT INTO [dbo].[settings_list]
([type], subsystem, [name], default_value, user_visibility)
VALUES ('Int', 'WebSite', 'WebSiteGroup', '0', 'NORMAL')

INSERT INTO [dbo].[settings_list]
([type], subsystem, [name], default_value, user_visibility)
VALUES ('String', 'WebSite', 'CustomLanguageName', '', 'NORMAL')

GO

-------- PAGE

INSERT INTO [dbo].[settings_list]
([type], subsystem, [name], default_value, user_visibility)
VALUES ('String', 'Page', 'MetaKeywords', '', 'NORMAL')

insert into [dbo].[settings_list]
([type], subsystem, [name], default_value, user_visibility)
values ('Bool', 'Page', 'RobotsIndex', 'true', 'NORMAL')
	
insert into [dbo].[settings_list]
([type], subsystem, [name], default_value, user_visibility)
values ('Bool', 'Page', 'RobotsFollow', 'true', 'NORMAL')

insert into [dbo].[settings_list]
([type], subsystem, [name], default_value, user_visibility)
values ('Url', 'Page', 'CustomCss', '', 'NORMAL')

INSERT INTO [dbo].[settings_list]
([type], subsystem, [name], default_value, user_visibility)
VALUES ('Url', 'Page', 'RelatedContentLink', '', 'NORMAL')

INSERT INTO [dbo].[settings_list]
([type], subsystem, [name], default_value, user_visibility)
VALUES ('String', 'Page', 'ExtraCssClass', '', 'NORMAL')

-------- MODULES

INSERT INTO [dbo].[template] (template_type, name, content) VALUES ('ImageTemplate',  'Image 150x150', 'O:27:One.Net.BLL.BOImageTemplate;i:width:150;i:height:100;' )
INSERT INTO [dbo].[template] (template_type, name, content) VALUES ('ImageTemplate',  'Image 710x650', 'O:27:One.Net.BLL.BOImageTemplate;i:width:710;i:height:650;b:aspectRatioSize:0;' )


INSERT INTO [dbo].[template] (template_type, name, content) VALUES ('ContentTemplate',  'ContentTemplate', '' );
-------- TEMPLATE CONTENT
