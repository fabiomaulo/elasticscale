CREATE TABLE [dbo].[Companies](
	[Id] [uniqueidentifier] NOT NULL,
	[TenantId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_Companies] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


CREATE TABLE [dbo].[Professionals](
	[Id] [uniqueidentifier] NOT NULL,
	[TenantId] [uniqueidentifier] NOT NULL,
	[EMail] [nvarchar](60) NOT NULL,
	[First] [nvarchar](50) NULL,
	[Last] [nvarchar](50) NULL,
	[Middle] [nvarchar](50) NULL,
 CONSTRAINT [PK_Professionals] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [dbo].[Projects](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[CompanyId] [uniqueidentifier] NULL,
	[Active] [bit] NOT NULL DEFAULT ((1)),
 CONSTRAINT [PK_Projects] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
CREATE TABLE [dbo].[WorksDone](
	[Id] [uniqueidentifier] NOT NULL,
	[TenantId] [uniqueidentifier] NOT NULL,
	[ProjectId] [uniqueidentifier] NOT NULL,
	[ProfessionalId] [uniqueidentifier] NOT NULL,
	[Description] [nvarchar](2048) NULL,
	[Day] [date] NOT NULL,
	[Duration] [bigint] NOT NULL,
	[Invoiced] [bit] NOT NULL DEFAULT ((0)),
 CONSTRAINT [PK_WorksDone] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
CREATE TABLE [dbo].[WorksDoneTags](
	[WorkDoneId] [uniqueidentifier] NOT NULL,
	[Tag] [nvarchar](128) NOT NULL
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[Projects]  WITH CHECK ADD  CONSTRAINT [FK_ProjectsCompany] FOREIGN KEY([CompanyId])
REFERENCES [dbo].[Companies] ([Id])
GO

ALTER TABLE [dbo].[Projects] CHECK CONSTRAINT [FK_ProjectsCompany]
GO
ALTER TABLE [dbo].[WorksDone]  WITH CHECK ADD  CONSTRAINT [FK_WorksDoneProfessional] FOREIGN KEY([ProfessionalId])
REFERENCES [dbo].[Professionals] ([Id])
GO

ALTER TABLE [dbo].[WorksDone] CHECK CONSTRAINT [FK_WorksDoneProfessional]
GO

ALTER TABLE [dbo].[WorksDone]  WITH CHECK ADD  CONSTRAINT [FK_WorksDoneProjects] FOREIGN KEY([ProjectId])
REFERENCES [dbo].[Projects] ([Id])
GO

ALTER TABLE [dbo].[WorksDone] CHECK CONSTRAINT [FK_WorksDoneProjects]
GO

ALTER TABLE [dbo].[WorksDoneTags]  WITH CHECK ADD  CONSTRAINT [FK_WorksDoneTagsWorksDone] FOREIGN KEY([WorkDoneId])
REFERENCES [dbo].[WorksDone] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[WorksDoneTags] CHECK CONSTRAINT [FK_WorksDoneTagsWorksDone]
GO

CREATE NONCLUSTERED INDEX [IX_CompaniesTenant] ON [dbo].[Companies] ([TenantId] ASC)
GO
CREATE NONCLUSTERED INDEX [IX_ProfessionalsTenant] ON [dbo].[Professionals] ([TenantId] ASC)
GO
CREATE NONCLUSTERED INDEX [IX_WorksDoneTenant] ON [dbo].[WorksDone] ([TenantId] ASC)
GO
