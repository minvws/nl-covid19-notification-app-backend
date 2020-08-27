GRANT CONNECT TO [$(Domain)\$(Appbeheerders)];
GRANT CONNECT TO [$(Domain)\$(Funcbeheerders)];
GRANT CONNECT TO [$(Domain)\$(Ontwikkelaars)];
GRANT CONNECT TO [$(Domain)\$(ServiceAccount)];
GRANT CONNECT TO [$(Domain)\$(ServiceAccountReport)];
GRANT CONNECT TO [$(Domain)\$(Users)];

--subroles
GRANT CONNECT TO [$(Domain)\$(MobileAppAPI)];
GRANT CONNECT TO [$(Domain)\$(IccBackend)];
GRANT CONNECT TO [$(Domain)\$(EksEngine)];
GRANT CONNECT TO [$(Domain)\$(ManifestEngine)];
GRANT CONNECT TO [$(Domain)\$(ContentAPI)];
GRANT CONNECT TO [$(Domain)\$(CleanupJob)];
GRANT CONNECT TO [$(Domain)\$(ManagementPortal)];
GRANT CONNECT TO [$(Domain)\$(DbProvision)];
GRANT CONNECT TO [$(Domain)\$(GenTeks)];

GRANT DELETE  ON SCHEMA::[dbo] TO [Dbr_Service];
GRANT EXECUTE ON SCHEMA::[dbo] TO [Dbr_Service];
GRANT INSERT  ON SCHEMA::[dbo] TO [Dbr_Service];
GRANT SELECT  ON SCHEMA::[dbo] TO [Dbr_Service];
GRANT UPDATE  ON SCHEMA::[dbo] TO [Dbr_Service];

GRANT SELECT  ON SCHEMA::[dbo] TO [Dbr_Funcbeheerders];

--specific permissions for WorkFlow here..
--[Dbr_Service_MobileAppAPI]
GRANT INSERT ON [dbo].[TekReleaseWorkflowState] TO [Dbr_Service_MobileAppAPI];
GRANT SELECT ON [dbo].[TekReleaseWorkflowState] TO [Dbr_Service_MobileAppAPI];
GRANT UPDATE ON [dbo].[TekReleaseWorkflowState] TO [Dbr_Service_MobileAppAPI];

GRANT INSERT ON [dbo].[TemporaryExposureKeys] TO [Dbr_Service_MobileAppAPI];

--[Dbr_Service_IccBackend]
GRANT SELECT ON [dbo].[TekReleaseWorkflowState] TO [Dbr_Service_IccBackend];
GRANT UPDATE ON [dbo].[TekReleaseWorkflowState] TO [Dbr_Service_IccBackend];

GRANT SELECT ON [dbo].[TemporaryExposureKeys] TO [Dbr_Service_IccBackend];
GRANT UPDATE ON [dbo].[TemporaryExposureKeys] TO [Dbr_Service_IccBackend];

--[Dbr_Service_EksEngine] 
GRANT SELECT ON [dbo].[TekReleaseWorkflowState] TO [Dbr_Service_EksEngine];
GRANT UPDATE ON [dbo].[TekReleaseWorkflowState] TO [Dbr_Service_EksEngine];

GRANT SELECT ON [dbo].[TemporaryExposureKeys] TO [Dbr_Service_EksEngine];
GRANT UPDATE ON [dbo].[TemporaryExposureKeys] TO [Dbr_Service_EksEngine];

--[Dbr_Service_CleanupJob] 
GRANT SELECT ON [dbo].[TekReleaseWorkflowState] TO [Dbr_Service_CleanupJob];
GRANT DELETE ON [dbo].[TekReleaseWorkflowState] TO [Dbr_Service_CleanupJob];
GRANT UPDATE ON [dbo].[TekReleaseWorkflowState] TO [Dbr_Service_CleanupJob];

GRANT SELECT ON [dbo].[TemporaryExposureKeys] TO [Dbr_Service_CleanupJob];
GRANT DELETE ON [dbo].[TemporaryExposureKeys] TO [Dbr_Service_CleanupJob];
GRANT UPDATE ON [dbo].[TemporaryExposureKeys] TO [Dbr_Service_CleanupJob];

--[Dbr_Service_ManagementPortal] 
GRANT SELECT ON [dbo].[TekReleaseWorkflowState] TO [Dbr_Service_ManagementPortal];

GRANT SELECT ON [dbo].[TemporaryExposureKeys] TO [Dbr_Service_ManagementPortal];

--[Dbr_Service_GenTeks] 
GRANT INSERT ON [dbo].[TekReleaseWorkflowState] TO [Dbr_Service_GenTeks];
GRANT INSERT ON [dbo].[TemporaryExposureKeys] TO [Dbr_Service_GenTeks];
--end 

GRANT VIEW ANY COLUMN ENCRYPTION KEY DEFINITION TO PUBLIC;
GRANT VIEW ANY COLUMN MASTER KEY DEFINITION TO PUBLIC;