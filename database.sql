USE [UserProjectDB]
GO
/****** Object:  Table [dbo].[Project]    Script Date: 5/3/2021 15:56:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Project](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TimeLog]    Script Date: 5/3/2021 15:56:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TimeLog](
	[ProjectId] [int] NULL,
	[UserId] [int] NULL,
	[DH] [datetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[User]    Script Date: 5/3/2021 15:56:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[User](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[GetUPTL]    Script Date: 5/3/2021 15:56:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetUPTL]
	 @PageNumber AS INT,
	 @RowsOfPage AS INT,
	 @CheckCount AS BIT 

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	--DECLARE @SortingCol AS VARCHAR(100) ='FruitName'
	--DECLARE @SortType AS VARCHAR(100) = 'DESC'
	--SET @PageNumber=1
	--SET @RowsOfPage=10

	IF (@CheckCount = 1)
	BEGIN
		SELECT Count(Id) as RowCountTotal from [User]
	END

	SELECT
    u.Id as UserId,
	u.Name as UserName,
	u.Name as UserSurName,
	p.Id as ProjectId,
	p.Name as ProjectName,
	tl.DH 
	FROM [User] u
	LEFT JOIN TimeLog tl 
	ON tl.UserId = u.Id
	LEFT JOIN Project p
	ON tl.ProjectId = p.Id
	ORDER BY u.Id
	OFFSET (@PageNumber-1)*@RowsOfPage ROWS
	FETCH NEXT @RowsOfPage ROWS ONLY
	--ORDER BY 
	--CASE WHEN @SortingCol = 'Price' AND @SortType ='ASC' THEN Price END ,
	--CASE WHEN @SortingCol = 'Price' AND @SortType ='DESC' THEN Price END DESC,
	--CASE WHEN @SortingCol = 'FruitName' AND @SortType ='ASC' THEN FruitName END ,
	--CASE WHEN @SortingCol = 'FruitName' AND @SortType ='DESC' THEN FruitName END DESC
	
END
GO
/****** Object:  StoredProcedure [dbo].[InsertProject]    Script Date: 5/3/2021 15:56:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[InsertProject]
	@Id INTEGER,  
	@Name VARCHAR(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	SET IDENTITY_INSERT dbo.[Project] ON;  

   insert into [Project] (Id,Name) values(@Id,@Name)
END
GO
/****** Object:  StoredProcedure [dbo].[InsertTimeLog]    Script Date: 5/3/2021 15:56:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[InsertTimeLog]
	@UserId INT,  
	@ProjectId INT,
	@DH VARCHAR(50) = '2020-07-05T23:59:59Z',
	@outputParam INT OUTPUT
AS
BEGIN

 DECLARE @DH_ DATETIME;
 SET @DH_  = CONVERT(DATETIME, CONVERT(DATETIMEOFFSET,@DH))
     INSERT INTO dbo.[TimeLog](UserId,ProjectId,DH)
	 SELECT u.Id, p.Id, @DH_
     FROM (SELECT Id from [User] WHERE Id= @UserId) as U
	 CROSS JOIN
	 (SELECT Id from [Project] WHERE Id= @ProjectId) as P;

	 SELECT @outputParam = @@ROWCOUNT
END
GO
/****** Object:  StoredProcedure [dbo].[InsertUser]    Script Date: 5/3/2021 15:56:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[InsertUser]
	@Id INTEGER,  
	@Name VARCHAR(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	SET IDENTITY_INSERT dbo.[User] ON;  
   insert into [User] (Id,Name) values(@Id,@Name)
END
GO
/****** Object:  StoredProcedure [dbo].[TruncateTable]    Script Date: 5/3/2021 15:56:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[TruncateTable]
	@Name varchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	If @Name= 'All'
	BEGIN
		TRUNCATE TABLE [TimeLog]
		TRUNCATE TABLE [User]
		TRUNCATE TABLE [Project]
	END
END
GO
