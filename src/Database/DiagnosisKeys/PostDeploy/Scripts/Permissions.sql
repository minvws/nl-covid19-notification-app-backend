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

--[Dbr_Service_EksEngine]
GRANT SELECT ON [dbo].[DiagnosisKeys] TO [Dbr_Service_EksEngine];
GRANT INSERT ON [dbo].[DiagnosisKeys] TO [Dbr_Service_EksEngine];
GRANT UPDATE ON [dbo].[DiagnosisKeys] TO [Dbr_Service_EksEngine];
GRANT DELETE ON [dbo].[DiagnosisKeys] TO [Dbr_Service_EksEngine];

GRANT SELECT ON [dbo].[DiagnosisKeysInput] TO [Dbr_Service_EksEngine];
GRANT INSERT ON [dbo].[DiagnosisKeysInput] TO [Dbr_Service_EksEngine];
GRANT UPDATE ON [dbo].[DiagnosisKeysInput] TO [Dbr_Service_EksEngine];
GRANT DELETE ON [dbo].[DiagnosisKeysInput] TO [Dbr_Service_EksEngine];
GRANT ALTER ON [dbo].[DiagnosisKeysInput] TO [Dbr_Service_EksEngine];

GRANT EXECUTE ON [dbo].[RemoveDuplicateDiagnosisKeysForIks] TO [Dbr_Service_EksEngine];

--[Dbr_Service_CleanupJob] 
GRANT INSERT ON [dbo].[DiagnosisKeys] TO [Dbr_Service_CleanupJob];
GRANT SELECT ON [dbo].[DiagnosisKeys] TO [Dbr_Service_CleanupJob];
GRANT UPDATE ON [dbo].[DiagnosisKeys] TO [Dbr_Service_CleanupJob];
GRANT DELETE ON [dbo].[DiagnosisKeys] TO [Dbr_Service_CleanupJob];

GRANT INSERT ON [dbo].[DiagnosisKeysInput] TO [Dbr_Service_CleanupJob];
GRANT SELECT ON [dbo].[DiagnosisKeysInput] TO [Dbr_Service_CleanupJob];
GRANT UPDATE ON [dbo].[DiagnosisKeysInput] TO [Dbr_Service_CleanupJob];
GRANT DELETE ON [dbo].[DiagnosisKeysInput] TO [Dbr_Service_CleanupJob];
GRANT ALTER ON [dbo].[DiagnosisKeysInput] TO [Dbr_Service_CleanupJob];

GRANT EXECUTE ON [dbo].[RemoveDuplicateDiagnosisKeysForIks] TO [Dbr_Service_CleanupJob];
--end

GRANT VIEW ANY COLUMN ENCRYPTION KEY DEFINITION TO PUBLIC;
GRANT VIEW ANY COLUMN MASTER KEY DEFINITION TO PUBLIC;