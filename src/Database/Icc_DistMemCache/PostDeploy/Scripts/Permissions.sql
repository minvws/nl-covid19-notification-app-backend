GRANT CONNECT TO [$(Domain)\$(Appbeheerders)];
GRANT CONNECT TO [$(Domain)\$(Funcbeheerders)];
GRANT CONNECT TO [$(Domain)\$(Ontwikkelaars)];
GRANT CONNECT TO [$(Domain)\$(ServiceAccount)];
GRANT CONNECT TO [$(Domain)\$(ServiceAccountReport)];
GRANT CONNECT TO [$(Domain)\$(Users)];

--subroles
GRANT CONNECT TO [$(Domain)\$(IccBackend)];

GRANT DELETE  ON SCHEMA::[dbo] TO [Dbr_Service];
GRANT EXECUTE ON SCHEMA::[dbo] TO [Dbr_Service];
GRANT INSERT  ON SCHEMA::[dbo] TO [Dbr_Service];
GRANT SELECT  ON SCHEMA::[dbo] TO [Dbr_Service];
GRANT UPDATE  ON SCHEMA::[dbo] TO [Dbr_Service];

GRANT SELECT  ON SCHEMA::[dbo] TO [Dbr_Funcbeheerders];

--specific permissions for [Cache]

GRANT SELECT ON [dbo].[Cache] TO [Dbr_Service_IccBackend];
GRANT INSERT ON [dbo].[Cache] TO [Dbr_Service_IccBackend];
GRANT UPDATE ON [dbo].[Cache] TO [Dbr_Service_IccBackend];
GRANT DELETE ON [dbo].[Cache] TO [Dbr_Service_IccBackend];

--end

GRANT VIEW ANY COLUMN ENCRYPTION KEY DEFINITION TO PUBLIC;
GRANT VIEW ANY COLUMN MASTER KEY DEFINITION TO PUBLIC;