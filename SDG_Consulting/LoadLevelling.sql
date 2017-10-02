﻿CREATE TABLE [dbo].[LoadLevelling]
(
	[Id] [INT] IDENTITY(1,1)  PRIMARY KEY,
	[PRODUCTION_CATEGORY] CHAR(2) NOT NULL,
	[IND_SEASONAL_STATUS] NVARCHAR(128) NOT NULL,
	[TCH_WEEK] CHAR(6) NOT NULL,
	[PLANNING_LEVEL] CHAR(10) NOT NULL,
	[EVENT] CHAR(10),
	[WEEK_PLAN] CHAR(6) NOT NULL,
	[F1] FLOAT,
	[F2] FLOAT,
	[F3] FLOAT,
	[Ahead] FLOAT NOT NULL,
	[Late] FLOAT NOT NULL,
	[Priority] FLOAT NOT NULL,
	[Capacity] FLOAT NOT NULL,
	[Required] FLOAT NOT NULL,
	[PLAN_BU] VARCHAR(10) NULL,
	[FLAG_HR] CHAR(1) NULL,
	[ALLOCATED] FLOAT NULL
)
