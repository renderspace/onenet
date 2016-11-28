CREATE LOGIN [sistem2] WITH PASSWORD = N'sistem2', DEFAULT_DATABASE=[sistem2], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF
GO

USE sistem2
GO

IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'sistem2')
BEGIN
    CREATE USER [sistem2] FOR LOGIN [sistem2] WITH DEFAULT_SCHEMA=[dbo];
    EXEC sp_addrolemember N'db_owner', N'sistem2'
END
GO