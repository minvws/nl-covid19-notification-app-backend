ALTER ROLE [Dbr_Appbeheerders] ADD MEMBER [$(Domain)\$(Appbeheerders)];
ALTER ROLE [Dbr_Funcbeheerders] ADD MEMBER [$(Domain)\$(Funcbeheerders)];
ALTER ROLE [Dbr_Service] ADD MEMBER [$(Domain)\$(ServiceAccount)];
ALTER ROLE [Dbr_Report] ADD MEMBER [$(Domain)\$(ServiceAccountReport)];
ALTER ROLE [Dbr_Ontwikkelaars] ADD MEMBER [$(Domain)\$(Ontwikkelaars)];
ALTER ROLE [Dbr_Users] ADD MEMBER [$(Domain)\$(Users)];

ALTER ROLE [Dbr_Service_MobileAppAPI] ADD MEMBER [$(Domain)\$(MobileAppAPI)];
ALTER ROLE [Dbr_Service_IccBackend] ADD MEMBER [$(Domain)\$(IccBackend)];
ALTER ROLE [Dbr_Service_EksEngine] ADD MEMBER [$(Domain)\$(EksEngine)];
ALTER ROLE [Dbr_Service_ManifestEngine] ADD MEMBER [$(Domain)\$(ManifestEngine)];
ALTER ROLE [Dbr_Service_ContentAPI] ADD MEMBER [$(Domain)\$(ContentAPI)];
ALTER ROLE [Dbr_Service_CleanupJob] ADD MEMBER [$(Domain)\$(CleanupJob)];
ALTER ROLE [Dbr_Service_ManagementPortal] ADD MEMBER [$(Domain)\$(ManagementPortal)];
ALTER ROLE [Dbr_Service_DbProvision] ADD MEMBER [$(Domain)\$(DbProvision)];
ALTER ROLE [Dbr_Service_GenTeks] ADD MEMBER [$(Domain)\$(GenTeks)];
ALTER ROLE [Dbr_Service_EfgsDownloader] ADD MEMBER [$(Domain)\$(EfgsDownloader)];
ALTER ROLE [Dbr_Service_EfgsUploader] ADD MEMBER [$(Domain)\$(EfgsUploader)];