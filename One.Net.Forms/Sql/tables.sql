
CREATE TABLE [dbo].[nform](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[title] [varchar](255) NOT NULL,
	[sub_title] [varchar](255) NOT NULL,
	[description] [varchar](max) NULL,
	[thank_you_note] [varchar](max) NOT NULL,
	[form_type] [varchar](255) NOT NULL,
	[send_to] [nvarchar](1000) NULL,
	[allow_multiple_submissions] [bit] NOT NULL,
	[allow_modify_in_submission] [bit] NOT NULL,
	[completion_redirect] [nvarchar](255) NULL,
 CONSTRAINT [PK_nform] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
;

CREATE TABLE [dbo].[nform_answer](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[title] [varchar](255) NOT NULL,
	[description] [varchar](max) NULL,
	[nform_question_fk_id] [int] NOT NULL,
	[idx] [int] NOT NULL,
	[answer_type] [varchar](255) NOT NULL,
	[additional_field_type] [varchar](255) NOT NULL,
	[max_file_size] [int] NULL,
	[allowed_mime_types] [nvarchar](1000) NULL,
	[max_chars] [int] NULL,
	[number_of_rows] [int] NULL,
	[is_fake] [bit] not null
 CONSTRAINT [PK_nform_answer] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
;

CREATE TABLE [dbo].[nform_question](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[title] [varchar](255) NOT NULL,
	[description] [varchar](max) NULL,
	[nform_section_fk_id] [int] NOT NULL,
	[idx] [int] NOT NULL,
	[is_answer_required] [bit] NOT NULL,
	[validation_type] [nvarchar](255) NULL,
 CONSTRAINT [PK_nform_question] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
;

CREATE TABLE [dbo].[nform_section](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[title] [varchar](255) NOT NULL,
	[description] [varchar](max) NULL,
	[nform_fk_id] [int] NOT NULL,
	[idx] [int] NOT NULL,
	[section_type] [varchar](255) NOT NULL,
	[on_client_click] [varchar](255) NULL,
 CONSTRAINT [PK_nform_section] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
;

CREATE TABLE [dbo].[nform_submission](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[nform_fk_id] [int] NOT NULL,
	[finished] [datetime] NOT NULL,
 CONSTRAINT [PK_nform_submission] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
;

CREATE TABLE [dbo].[nform_submitted_answer](
	[nform_answer_fk_id] [int] NOT NULL,
	[nform_submission_fk_id] [int] NOT NULL,
	[submitted_text] [ntext] NULL,
	[files_fk_id] [int] NULL,
 CONSTRAINT [PK_nform_submitted_answer] PRIMARY KEY CLUSTERED 
(
	[nform_answer_fk_id] ASC,
	[nform_submission_fk_id] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
;

ALTER TABLE [dbo].[nform_submitted_answer]  WITH CHECK ADD  CONSTRAINT [FK_nform_submitted_answer_nform_answer] FOREIGN KEY([nform_answer_fk_id])
REFERENCES [dbo].[nform_answer] ([id]);
ALTER TABLE [dbo].[nform_submitted_answer] CHECK CONSTRAINT [FK_nform_submitted_answer_nform_answer];
ALTER TABLE [dbo].[nform_submitted_answer]  WITH CHECK ADD  CONSTRAINT [FK_nform_submitted_answer_nform_submission] FOREIGN KEY([nform_submission_fk_id])
REFERENCES [dbo].[nform_submission] ([id]) ON DELETE CASCADE;
ALTER TABLE [dbo].[nform_submitted_answer] CHECK CONSTRAINT [FK_nform_submitted_answer_nform_submission];
ALTER TABLE [dbo].[nform_section]  WITH CHECK ADD  CONSTRAINT [FK_nform_section_nform] FOREIGN KEY([nform_fk_id])
REFERENCES [dbo].[nform] ([id]);
ALTER TABLE [dbo].[nform_section] CHECK CONSTRAINT [FK_nform_section_nform];
ALTER TABLE [dbo].[nform_submission]  WITH CHECK ADD  CONSTRAINT [FK_nform_submission_nform] FOREIGN KEY([nform_fk_id])
REFERENCES [dbo].[nform] ([id]);
ALTER TABLE [dbo].[nform_submission] CHECK CONSTRAINT [FK_nform_submission_nform];
ALTER TABLE [dbo].[nform_question]  WITH CHECK ADD  CONSTRAINT [FK_nform_question_nform_section] FOREIGN KEY([nform_section_fk_id])
REFERENCES [dbo].[nform_section] ([id]);
ALTER TABLE [dbo].[nform_question] CHECK CONSTRAINT [FK_nform_question_nform_section];
ALTER TABLE [dbo].[nform_answer]  WITH CHECK ADD  CONSTRAINT [FK_nform_answer_nform_question] FOREIGN KEY([nform_question_fk_id])
REFERENCES [dbo].[nform_question] ([id]);
ALTER TABLE [dbo].[nform_answer] CHECK CONSTRAINT [FK_nform_answer_nform_question];