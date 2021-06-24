CREATE TABLE [dbo].[csDocument](
	[ID] [uniqueidentifier] NOT NULL,
	[ReferenceObject] [nvarchar](100) NULL,
	[DocType] [varchar](50) NOT NULL,
	[FileName] [nvarchar](150) NULL,
	[FileData] [varbinary](max) NULL,
	[Icon] [image] NULL,
	[FileDescription] [nvarchar](500) NULL,
	[FileCheckSum] [nvarchar](100) NULL,
	[UserCreated] [varchar](50) NULL,
	[DatetimeCreated] [smalldatetime] NULL,
	[UserModified] [varchar](50) NULL,
	[DatetimeModified] [smalldatetime] NULL,
	[Inactive] [bit] NULL,
	[InactiveOn] [smalldatetime] NULL,
	[BranchID] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_csDocument] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [U_Document] UNIQUE NONCLUSTERED 
(
	[BranchID] ASC,
	[ReferenceObject] ASC,
	[DocType] ASC,
	[FileName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
