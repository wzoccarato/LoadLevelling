﻿CREATE TABLE [dbo].[Schema]
(
	[Id] [INT] IDENTITY(1,1)  PRIMARY KEY,
	[BlockId] NVARCHAR(5) UNIQUE NOT NULL,
	[CubeName] NVARCHAR(256) NULL,
	[Heading] NVARCHAR(256) NULL,
	[WriteBack] NVARCHAR(5)
)
