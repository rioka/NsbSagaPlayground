IF (db_id(N'NsbSagaPlayground') IS NULL)
  CREATE DATABASE [NsbSagaPlayground]

GO

USE [NsbSagaPlayground]

GO

IF NOT EXISTS
(
  SELECT 1 
  FROM [INFORMATION_SCHEMA].[SCHEMATA]
  WHERE [SCHEMA_NAME] = N'nsb'
)
BEGIN

  /* The schema must be run in its own batch! */
  EXEC(N'CREATE SCHEMA [nsb] AUTHORIZATION [dbo]');

END    

IF NOT EXISTS
(
  SELECT 1 
  FROM [INFORMATION_SCHEMA].[TABLES]
  WHERE [TABLE_SCHEMA] = N'dbo'
    AND [TABLE_NAME] = N'Orders'
)
BEGIN

  CREATE TABLE [dbo].[Orders]
  (
    [Id]          INT               NOT NULL IDENTITY(1, 1),
    [UId]         UNIQUEIDENTIFIER  NOT NULL,
    [CreatedAt]   DATETIME2         NOT NULL,
    [ConfirmedAt] DATETIME2         NULL,
    [CancelledAt] DATETIME2         NULL
    
    CONSTRAINT [PK_Orders] PRIMARY KEY CLUSTERED ([Id])
  );

  CREATE UNIQUE NONCLUSTERED INDEX [UQ_UId] 
    ON [dbo].[Orders] ([UId] ASC);
  
END

GO  
