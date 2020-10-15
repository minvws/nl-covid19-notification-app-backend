IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = N'$(Domain)\$(Appbeheerders)')
	CREATE LOGIN [$(Domain)\$(Appbeheerders)] FROM WINDOWS;

IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = N'$(Domain)\$(Funcbeheerders)')
	CREATE LOGIN [$(Domain)\$(Funcbeheerders)] FROM WINDOWS;

IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = N'$(Domain)\$(Ontwikkelaars)')
	CREATE LOGIN [$(Domain)\$(Ontwikkelaars)] FROM WINDOWS;

IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = N'$(Domain)\$(ServiceAccount)')
	CREATE LOGIN [$(Domain)\$(ServiceAccount)] FROM WINDOWS;

IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = N'$(Domain)\$(ServiceAccountReport)')
	CREATE LOGIN [$(Domain)\$(ServiceAccountReport)] FROM WINDOWS;

IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = N'$(Domain)\$(Users)')
	CREATE LOGIN [$(Domain)\$(Users)] FROM WINDOWS;

-- Subroles

IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = N'$(Domain)\$(MobileAppAPI)')
	CREATE LOGIN [$(Domain)\$(MobileAppAPI)] FROM WINDOWS;
IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = N'$(Domain)\$(IccBackend)')
	CREATE LOGIN [$(Domain)\$(IccBackend)] FROM WINDOWS;
IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = N'$(Domain)\$(EksEngine)')
	CREATE LOGIN [$(Domain)\$(EksEngine)] FROM WINDOWS;
IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = N'$(Domain)\$(ManifestEngine)')
	CREATE LOGIN [$(Domain)\$(ManifestEngine)] FROM WINDOWS;
IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = N'$(Domain)\$(ContentAPI)')
	CREATE LOGIN [$(Domain)\$(ContentAPI)] FROM WINDOWS;
IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = N'$(Domain)\$(CleanupJob)')
	CREATE LOGIN [$(Domain)\$(CleanupJob)] FROM WINDOWS;
IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = N'$(Domain)\$(ManagementPortal)')
	CREATE LOGIN [$(Domain)\$(ManagementPortal)] FROM WINDOWS;
IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = N'$(Domain)\$(DbProvision)')
	CREATE LOGIN [$(Domain)\$(DbProvision)] FROM WINDOWS;
IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = N'$(Domain)\$(GenTeks)')
	CREATE LOGIN [$(Domain)\$(GenTeks)] FROM WINDOWS;



IF NOT EXISTS (SELECT name FROM [sys].[database_principals] WHERE name = N'$(Domain)\$(Appbeheerders)')
	CREATE USER [$(Domain)\$(Appbeheerders)];

IF NOT EXISTS (SELECT name FROM [sys].[database_principals] WHERE name = N'$(Domain)\$(Funcbeheerders)')
	CREATE USER [$(Domain)\$(Funcbeheerders)];

IF NOT EXISTS (SELECT name FROM [sys].[database_principals] WHERE name = N'$(Domain)\$(Ontwikkelaars)')
	CREATE USER [$(Domain)\$(Ontwikkelaars)]

IF NOT EXISTS (SELECT name FROM [sys].[database_principals] WHERE name = N'$(Domain)\$(ServiceAccount)')
	CREATE USER [$(Domain)\$(ServiceAccount)];

IF NOT EXISTS (SELECT name FROM [sys].[database_principals] WHERE name = N'$(Domain)\$(ServiceAccountReport)')
	CREATE USER [$(Domain)\$(ServiceAccountReport)];

IF NOT EXISTS (SELECT name FROM [sys].[database_principals] WHERE name = N'$(Domain)\$(Users)')
	CREATE USER [$(Domain)\$(Users)];

-- Subroles
IF NOT EXISTS (SELECT name FROM [sys].[database_principals] WHERE name = N'$(Domain)\$(MobileAppAPI)')
	CREATE USER [$(Domain)\$(MobileAppAPI)];
IF NOT EXISTS (SELECT name FROM [sys].[database_principals] WHERE name = N'$(Domain)\$(IccBackend)')
	CREATE USER [$(Domain)\$(IccBackend)];
IF NOT EXISTS (SELECT name FROM [sys].[database_principals] WHERE name = N'$(Domain)\$(EksEngine)')
	CREATE USER [$(Domain)\$(EksEngine)];
IF NOT EXISTS (SELECT name FROM [sys].[database_principals] WHERE name = N'$(Domain)\$(ManifestEngine)')
	CREATE USER [$(Domain)\$(ManifestEngine)];
IF NOT EXISTS (SELECT name FROM [sys].[database_principals] WHERE name = N'$(Domain)\$(ContentAPI)')
	CREATE USER [$(Domain)\$(ContentAPI)];
IF NOT EXISTS (SELECT name FROM [sys].[database_principals] WHERE name = N'$(Domain)\$(CleanupJob)')
	CREATE USER [$(Domain)\$(CleanupJob)];
IF NOT EXISTS (SELECT name FROM [sys].[database_principals] WHERE name = N'$(Domain)\$(ManagementPortal)')
	CREATE USER [$(Domain)\$(ManagementPortal)];
IF NOT EXISTS (SELECT name FROM [sys].[database_principals] WHERE name = N'$(Domain)\$(DbProvision)')
	CREATE USER [$(Domain)\$(DbProvision)];
IF NOT EXISTS (SELECT name FROM [sys].[database_principals] WHERE name = N'$(Domain)\$(GenTeks)')
	CREATE USER [$(Domain)\$(GenTeks)];