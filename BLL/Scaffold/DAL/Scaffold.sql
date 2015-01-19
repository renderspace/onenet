

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[_virtual_relation]') AND type in (N'U'))
DROP TABLE [dbo].[_virtual_relation]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[_virtual_col]') AND type in (N'U'))
DROP TABLE [dbo].[_virtual_col]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[_virtual_table]') AND type in (N'U'))
DROP TABLE [dbo].[_virtual_table]
GO

CREATE TABLE [dbo].[_virtual_table](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[starting_table] [varchar](255) NOT NULL,
	[order_col] [varchar](50) NOT NULL,
	[show_on_menu] [bit] NOT NULL,
	[condition] [varchar](255) NOT NULL DEFAULT (''),
	[friendly_name] [nvarchar](100) NOT NULL DEFAULT (''),
 CONSTRAINT [PK_virtual_table] PRIMARY KEY CLUSTERED  ([id] ASC)
	WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

CREATE TABLE [dbo].[_virtual_col](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[virtual_table_id] [int] NOT NULL,
	[col_name] [varchar](255) NOT NULL,
	[form_type] [varchar](50) NOT NULL,
	[friendly_name] [nvarchar](100) NOT NULL DEFAULT (''),
	[is_multilanguage_content] bit NOT NULL DEFAULT ((0)),
	[show_on_list] [bit] NOT NULL DEFAULT ((1)),
	[is_wysiwyg] [bit] NOT NULL DEFAULT ((0)),
 CONSTRAINT [PK_virtual_col] PRIMARY KEY CLUSTERED  ( 	[id] ASC )
	WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[_virtual_col]  WITH CHECK ADD  CONSTRAINT [FK_virtual_col_virtual_table] FOREIGN KEY([virtual_table_id])
REFERENCES [dbo].[_virtual_table] ([id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO


CREATE TABLE [dbo].[_virtual_relation](
	[id] [int] IDENTITY(1000,1) NOT NULL,
	[pk_virtual_table_id] [int] NOT NULL,
	[fk_virtual_table_id] [int] NOT NULL,
	[pk_display_col] [varchar](255) NOT NULL,
	[pk_xref_virtual_table_id] [int] NULL,
	[is_multilanguage_content] bit NOT NULL DEFAULT ((0)),
 CONSTRAINT [PK_virtual_relation] PRIMARY KEY CLUSTERED  ( 	[id] ASC )
 WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[_virtual_relation]  WITH CHECK ADD  CONSTRAINT [FK_virtual_relation_virtual_table_xref] FOREIGN KEY([pk_xref_virtual_table_id])
REFERENCES [dbo].[_virtual_table] ([id])
GO

ALTER TABLE [dbo].[_virtual_relation]  WITH CHECK ADD  CONSTRAINT [FK_virtual_relation_virtual_table] FOREIGN KEY([fk_virtual_table_id])
REFERENCES [dbo].[_virtual_table] ([id])
ON UPDATE CASCADE
ON DELETE CASCADE
GO


ALTER TABLE [dbo].[_virtual_relation]  WITH CHECK ADD  CONSTRAINT [FK_virtual_relation_virtual_table_pk] FOREIGN KEY([pk_virtual_table_id])
REFERENCES [dbo].[_virtual_table] ([id])
GO

ALTER TABLE [dbo].[_virtual_relation] CHECK CONSTRAINT [FK_virtual_relation_virtual_table_pk]
GO


CREATE TABLE [dbo].[_sequence](
	[name] [varchar](100) NOT NULL,
	[sequence_id] [int] NOT NULL,
 CONSTRAINT [PK__sequences] PRIMARY KEY CLUSTERED 
(
	[name] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE PROCEDURE [dbo].[_new_in_sequence]
                 @name varchar(100),
                 @sequence_id INT OUTPUT
               AS

    set @sequence_id = -1

	UPDATE _sequence
	SET    @sequence_id = sequence_id = sequence_id + 1
	WHERE  [name] = @name

	IF (@sequence_id = -1 )
	RAISERROR (50005,  10, 1, N'Sequence doesn''t exist');

	RETURN @sequence_id
	
GO

INSERT INTO [dbo].[_sequence] ([name],[sequence_id]) VALUES ('_content',  1000)

GO



