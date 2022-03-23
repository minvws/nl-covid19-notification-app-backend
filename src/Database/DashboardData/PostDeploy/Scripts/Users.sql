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

IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = N'$(Domain)\$(DashboardData)')
	CREATE LOGIN [$(Domain)\$(DashboardData)] FROM WINDOWS;



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
IF NOT EXISTS (SELECT name FROM [sys].[database_principals] WHERE name = N'$(Domain)\$(DashboardData)')
	CREATE USER [$(Domain)\$(DashboardData)];