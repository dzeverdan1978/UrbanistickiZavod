USE [Urban]
GO
if exists (select name from sysobjects where name='Napomena' and type='U')
	drop table Napomena
GO
CREATE TABLE [dbo].[Napomena](
	[id] [nvarchar](50) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
	[opis] [nvarchar](2000) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
	[prikazuje] [bit] NOT NULL CONSTRAINT [DF_Napomena_prikazuje]  DEFAULT ((1)),
 CONSTRAINT [PK_Napomena] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
go
insert into Napomena (id) values ('NAPOMENA I')
insert into Napomena (id) values ('NAPOMENA II')
insert into Napomena (id) values ('NAPOMENA III')
insert into Napomena (id) values ('NAPOMENA IV')
