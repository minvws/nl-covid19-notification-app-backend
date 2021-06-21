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

--specific permissions for Content here..
--[Dbr_Service_EksEngine]
GRANT INSERT ON [dbo].[Content] TO [Dbr_Service_EksEngine];
GRANT SELECT ON [dbo].[Content] TO [Dbr_Service_EksEngine];
GRANT DELETE ON [dbo].[Content] TO [Dbr_Service_EksEngine];

--[Dbr_Service_ManifestEngine] 
GRANT INSERT ON [dbo].[Content] TO [Dbr_Service_ManifestEngine];
GRANT SELECT ON [dbo].[Content] TO [Dbr_Service_ManifestEngine];
GRANT DELETE ON [dbo].[Content] TO [Dbr_Service_ManifestEngine];

--[Dbr_Service_ContentAPI] 
GRANT SELECT ON [dbo].[Content] TO [Dbr_Service_ContentAPI];

--[Dbr_Service_CleanupJob] 
GRANT SELECT ON [dbo].[Content] TO [Dbr_Service_CleanupJob];
GRANT DELETE ON [dbo].[Content] TO [Dbr_Service_CleanupJob];
GRANT INSERT ON [dbo].[Content] TO [Dbr_Service_CleanupJob];

--[Dbr_Service_ManagementPortal] 
GRANT SELECT ON [dbo].[Content] TO [Dbr_Service_ManagementPortal];
GRANT INSERT ON [dbo].[Content] TO [Dbr_Service_ManagementPortal];
GRANT UPDATE ON [dbo].[Content] TO [Dbr_Service_ManagementPortal];
--end 

GRANT VIEW ANY COLUMN ENCRYPTION KEY DEFINITION TO PUBLIC;
GRANT VIEW ANY COLUMN MASTER KEY DEFINITION TO PUBLIC;