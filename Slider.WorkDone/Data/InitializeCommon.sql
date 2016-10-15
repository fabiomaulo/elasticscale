IF OBJECT_ID('Tenants', 'U') IS NULL 
BEGIN
	CREATE TABLE Tenants(
		Id uniqueidentifier NOT NULL,
		[Owner] nvarchar(100) NOT NULL,
		Name nvarchar(140) NOT NULL,
		[Level] varchar(20) NOT NULL,
	 CONSTRAINT PK_Tenants PRIMARY KEY CLUSTERED (Id)
	)
	CREATE NONCLUSTERED INDEX IX_Tenants_Owner ON Tenants([Owner] ASC)
END
GO
