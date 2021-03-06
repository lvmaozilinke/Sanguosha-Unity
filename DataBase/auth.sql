USE [Sanguosha-auth]
GO
ALTER TABLE [dbo].[account] DROP CONSTRAINT [CK_account]
GO
ALTER TABLE [dbo].[profile] DROP CONSTRAINT [FK_profile_account]
GO
ALTER TABLE [dbo].[game_play] DROP CONSTRAINT [FK_game_play_account]
GO
ALTER TABLE [dbo].[achieve] DROP CONSTRAINT [FK_achieve_account]
GO
ALTER TABLE [dbo].[title] DROP CONSTRAINT [DF_title_date]
GO
ALTER TABLE [dbo].[profile] DROP CONSTRAINT [DF_profile_Title]
GO
ALTER TABLE [dbo].[profile] DROP CONSTRAINT [DF_profile_Escape]
GO
ALTER TABLE [dbo].[profile] DROP CONSTRAINT [DF_profile_Draw]
GO
ALTER TABLE [dbo].[profile] DROP CONSTRAINT [DF_profile_Lose]
GO
ALTER TABLE [dbo].[profile] DROP CONSTRAINT [DF_profile_Win]
GO
ALTER TABLE [dbo].[profile] DROP CONSTRAINT [DF_profile_GamePlay]
GO
ALTER TABLE [dbo].[profile] DROP CONSTRAINT [DF_profile_Image3]
GO
ALTER TABLE [dbo].[profile] DROP CONSTRAINT [DF_profile_Image2]
GO
ALTER TABLE [dbo].[profile] DROP CONSTRAINT [DF_profile_Image1]
GO
ALTER TABLE [dbo].[account] DROP CONSTRAINT [DF_account_roomID]
GO
ALTER TABLE [dbo].[account] DROP CONSTRAINT [DF_account_inGame]
GO
ALTER TABLE [dbo].[account] DROP CONSTRAINT [DF_account_status]
GO
ALTER TABLE [dbo].[account] DROP CONSTRAINT [DF_account_User_Right]
GO
/****** Object:  Index [IX_profile]    Script Date: 2019/9/1 20:31:18 ******/
DROP INDEX [IX_profile] ON [dbo].[profile]
GO
/****** Object:  Index [IX_account]    Script Date: 2019/9/1 20:31:18 ******/
ALTER TABLE [dbo].[account] DROP CONSTRAINT [IX_account]
GO
/****** Object:  Table [dbo].[title_mark]    Script Date: 2019/9/1 20:31:18 ******/
DROP TABLE [dbo].[title_mark]
GO
/****** Object:  Table [dbo].[title]    Script Date: 2019/9/1 20:31:18 ******/
DROP TABLE [dbo].[title]
GO
/****** Object:  Table [dbo].[profile]    Script Date: 2019/9/1 20:31:18 ******/
DROP TABLE [dbo].[profile]
GO
/****** Object:  Table [dbo].[game_play]    Script Date: 2019/9/1 20:31:18 ******/
DROP TABLE [dbo].[game_play]
GO
/****** Object:  Table [dbo].[achieve_mark]    Script Date: 2019/9/1 20:31:18 ******/
DROP TABLE [dbo].[achieve_mark]
GO
/****** Object:  Table [dbo].[achieve]    Script Date: 2019/9/1 20:31:18 ******/
DROP TABLE [dbo].[achieve]
GO
/****** Object:  Table [dbo].[account]    Script Date: 2019/9/1 20:31:18 ******/
DROP TABLE [dbo].[account]
GO
USE [master]
GO
/****** Object:  Database [Sanguosha-auth]    Script Date: 2019/9/1 20:31:18 ******/
DROP DATABASE [Sanguosha-auth]
GO
/****** Object:  Database [Sanguosha-auth]    Script Date: 2019/9/1 20:31:18 ******/
CREATE DATABASE [Sanguosha-auth]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'PlayCard2', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL14.MSSQLSERVER\MSSQL\DATA\PlayCard2.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'PlayCard2_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL14.MSSQLSERVER\MSSQL\DATA\PlayCard2_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
GO
ALTER DATABASE [Sanguosha-auth] SET COMPATIBILITY_LEVEL = 140
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [Sanguosha-auth].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [Sanguosha-auth] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [Sanguosha-auth] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [Sanguosha-auth] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [Sanguosha-auth] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [Sanguosha-auth] SET ARITHABORT OFF 
GO
ALTER DATABASE [Sanguosha-auth] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [Sanguosha-auth] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [Sanguosha-auth] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [Sanguosha-auth] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [Sanguosha-auth] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [Sanguosha-auth] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [Sanguosha-auth] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [Sanguosha-auth] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [Sanguosha-auth] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [Sanguosha-auth] SET  DISABLE_BROKER 
GO
ALTER DATABASE [Sanguosha-auth] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [Sanguosha-auth] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [Sanguosha-auth] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [Sanguosha-auth] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [Sanguosha-auth] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [Sanguosha-auth] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [Sanguosha-auth] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [Sanguosha-auth] SET RECOVERY FULL 
GO
ALTER DATABASE [Sanguosha-auth] SET  MULTI_USER 
GO
ALTER DATABASE [Sanguosha-auth] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [Sanguosha-auth] SET DB_CHAINING OFF 
GO
ALTER DATABASE [Sanguosha-auth] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [Sanguosha-auth] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [Sanguosha-auth] SET DELAYED_DURABILITY = DISABLED 
GO
EXEC sys.sp_db_vardecimal_storage_format N'Sanguosha-auth', N'ON'
GO
ALTER DATABASE [Sanguosha-auth] SET QUERY_STORE = OFF
GO
USE [Sanguosha-auth]
GO
/****** Object:  Table [dbo].[account]    Script Date: 2019/9/1 20:31:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[account](
	[uid] [int] IDENTITY(1,1) NOT NULL,
	[account] [varchar](50) NOT NULL,
	[password] [varchar](50) NOT NULL,
	[User_Right] [int] NOT NULL,
	[status] [bit] NOT NULL,
	[lastIP] [varchar](50) NULL,
	[login_date] [datetime] NULL,
	[inGame] [bit] NOT NULL,
	[roomID] [int] NULL,
 CONSTRAINT [PK_account] PRIMARY KEY CLUSTERED 
(
	[uid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[achieve]    Script Date: 2019/9/1 20:31:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[achieve](
	[uid] [int] NOT NULL,
	[achieve_id] [int] NOT NULL,
	[date] [datetime] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[achieve_mark]    Script Date: 2019/9/1 20:31:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[achieve_mark](
	[uid] [int] NOT NULL,
	[mark_id] [int] NOT NULL,
	[value] [int] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[game_play]    Script Date: 2019/9/1 20:31:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[game_play](
	[uid] [int] NOT NULL,
	[game_id] [int] NOT NULL,
	[result] [varchar](50) NOT NULL,
	[date] [date] NOT NULL,
	[generals] [varchar](50) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[profile]    Script Date: 2019/9/1 20:31:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[profile](
	[uid] [int] NOT NULL,
	[NickName] [varchar](50) NOT NULL,
	[avatar] [int] NOT NULL,
	[frame] [int] NOT NULL,
	[bg] [int] NOT NULL,
	[GamePlay] [int] NOT NULL,
	[Win] [int] NOT NULL,
	[Lose] [int] NOT NULL,
	[Draw] [int] NOT NULL,
	[Escape] [int] NOT NULL,
	[Title_id] [int] NOT NULL,
 CONSTRAINT [PK_profile] PRIMARY KEY CLUSTERED 
(
	[uid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[title]    Script Date: 2019/9/1 20:31:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[title](
	[uid] [int] NOT NULL,
	[title_id] [int] NOT NULL,
	[date] [datetime] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[title_mark]    Script Date: 2019/9/1 20:31:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[title_mark](
	[uid] [int] NOT NULL,
	[mark_id] [int] NOT NULL,
	[value] [int] NOT NULL
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_account]    Script Date: 2019/9/1 20:31:19 ******/
ALTER TABLE [dbo].[account] ADD  CONSTRAINT [IX_account] UNIQUE NONCLUSTERED 
(
	[account] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_profile]    Script Date: 2019/9/1 20:31:19 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_profile] ON [dbo].[profile]
(
	[NickName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[account] ADD  CONSTRAINT [DF_account_User_Right]  DEFAULT ((0)) FOR [User_Right]
GO
ALTER TABLE [dbo].[account] ADD  CONSTRAINT [DF_account_status]  DEFAULT ((0)) FOR [status]
GO
ALTER TABLE [dbo].[account] ADD  CONSTRAINT [DF_account_inGame]  DEFAULT ((0)) FOR [inGame]
GO
ALTER TABLE [dbo].[account] ADD  CONSTRAINT [DF_account_roomID]  DEFAULT ((-1)) FOR [roomID]
GO
ALTER TABLE [dbo].[profile] ADD  CONSTRAINT [DF_profile_Image1]  DEFAULT ((100099)) FOR [avatar]
GO
ALTER TABLE [dbo].[profile] ADD  CONSTRAINT [DF_profile_Image2]  DEFAULT ((200099)) FOR [frame]
GO
ALTER TABLE [dbo].[profile] ADD  CONSTRAINT [DF_profile_Image3]  DEFAULT ((300099)) FOR [bg]
GO
ALTER TABLE [dbo].[profile] ADD  CONSTRAINT [DF_profile_GamePlay]  DEFAULT ((0)) FOR [GamePlay]
GO
ALTER TABLE [dbo].[profile] ADD  CONSTRAINT [DF_profile_Win]  DEFAULT ((0)) FOR [Win]
GO
ALTER TABLE [dbo].[profile] ADD  CONSTRAINT [DF_profile_Lose]  DEFAULT ((0)) FOR [Lose]
GO
ALTER TABLE [dbo].[profile] ADD  CONSTRAINT [DF_profile_Draw]  DEFAULT ((0)) FOR [Draw]
GO
ALTER TABLE [dbo].[profile] ADD  CONSTRAINT [DF_profile_Escape]  DEFAULT ((0)) FOR [Escape]
GO
ALTER TABLE [dbo].[profile] ADD  CONSTRAINT [DF_profile_Title]  DEFAULT ((0)) FOR [Title_id]
GO
ALTER TABLE [dbo].[title] ADD  CONSTRAINT [DF_title_date]  DEFAULT (getdate()) FOR [date]
GO
ALTER TABLE [dbo].[achieve]  WITH CHECK ADD  CONSTRAINT [FK_achieve_account] FOREIGN KEY([uid])
REFERENCES [dbo].[account] ([uid])
GO
ALTER TABLE [dbo].[achieve] CHECK CONSTRAINT [FK_achieve_account]
GO
ALTER TABLE [dbo].[game_play]  WITH CHECK ADD  CONSTRAINT [FK_game_play_account] FOREIGN KEY([uid])
REFERENCES [dbo].[account] ([uid])
GO
ALTER TABLE [dbo].[game_play] CHECK CONSTRAINT [FK_game_play_account]
GO
ALTER TABLE [dbo].[profile]  WITH CHECK ADD  CONSTRAINT [FK_profile_account] FOREIGN KEY([uid])
REFERENCES [dbo].[account] ([uid])
GO
ALTER TABLE [dbo].[profile] CHECK CONSTRAINT [FK_profile_account]
GO
ALTER TABLE [dbo].[account]  WITH CHECK ADD  CONSTRAINT [CK_account] CHECK  (([user_right]>=(0) AND [user_right]<=(3)))
GO
ALTER TABLE [dbo].[account] CHECK CONSTRAINT [CK_account]
GO
USE [master]
GO
ALTER DATABASE [Sanguosha-auth] SET  READ_WRITE 
GO
