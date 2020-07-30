ALTER ROLE [Dbr_Appbeheerders] ADD MEMBER [$(Domain)\$(Appbeheerders)];
ALTER ROLE [Dbr_Funcbeheerders] ADD MEMBER [$(Domain)\$(Funcbeheerders)];
ALTER ROLE [Dbr_Service] ADD MEMBER [$(Domain)\$(ServiceAccount)];
ALTER ROLE [Dbr_Report] ADD MEMBER [$(Domain)\$(ServiceAccountReport)];
ALTER ROLE [Dbr_Ontwikkelaars] ADD MEMBER [$(Domain)\$(Ontwikkelaars)];
ALTER ROLE [Dbr_Users] ADD MEMBER [$(Domain)\$(Users)];