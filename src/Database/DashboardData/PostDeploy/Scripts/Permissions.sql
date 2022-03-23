GRANT CONNECT TO [$(Domain)\$(Appbeheerders)];
GRANT CONNECT TO [$(Domain)\$(Funcbeheerders)];
GRANT CONNECT TO [$(Domain)\$(Ontwikkelaars)];
GRANT CONNECT TO [$(Domain)\$(ServiceAccount)];
GRANT CONNECT TO [$(Domain)\$(ServiceAccountReport)];
GRANT CONNECT TO [$(Domain)\$(Users)];

--subroles
GRANT CONNECT TO [$(Domain)\$(DashboardData)];

GRANT DELETE  ON SCHEMA::[dbo] TO [Dbr_Service];
GRANT EXECUTE ON SCHEMA::[dbo] TO [Dbr_Service];
GRANT INSERT  ON SCHEMA::[dbo] TO [Dbr_Service];
GRANT SELECT  ON SCHEMA::[dbo] TO [Dbr_Service];
GRANT UPDATE  ON SCHEMA::[dbo] TO [Dbr_Service];

GRANT SELECT  ON SCHEMA::[dbo] TO [Dbr_Funcbeheerders];

--[Dbr_Service_DashboardData]

GRANT INSERT ON [dbo].[DashboardOutputJson] TO [Dbr_Service_DashboardData];
GRANT SELECT ON [dbo].[DashboardOutputJson] TO [Dbr_Service_DashboardData];
GRANT UPDATE ON [dbo].[DashboardOutputJson] TO [Dbr_Service_DashboardData];
GRANT DELETE ON [dbo].[DashboardOutputJson] TO [Dbr_Service_DashboardData];

GRANT INSERT ON [dbo].[DashboardInputJson] TO [Dbr_Service_DashboardData];
GRANT SELECT ON [dbo].[DashboardInputJson] TO [Dbr_Service_DashboardData];
GRANT UPDATE ON [dbo].[DashboardInputJson] TO [Dbr_Service_DashboardData];
GRANT DELETE ON [dbo].[DashboardInputJson] TO [Dbr_Service_DashboardData];

GRANT INSERT ON [dbo].[CdnStats] TO [Dbr_Service_DashboardData];
GRANT SELECT ON [dbo].[CdnStats] TO [Dbr_Service_DashboardData];
GRANT UPDATE ON [dbo].[CdnStats] TO [Dbr_Service_DashboardData];
GRANT DELETE ON [dbo].[CdnStats] TO [Dbr_Service_DashboardData];
--end

GRANT VIEW ANY COLUMN ENCRYPTION KEY DEFINITION TO PUBLIC;
GRANT VIEW ANY COLUMN MASTER KEY DEFINITION TO PUBLIC;