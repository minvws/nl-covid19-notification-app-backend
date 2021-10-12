PRINT 'START POST-DEPLOYMENT ON $(Domain)'

DECLARE @Domain NVARCHAR(3) = '$(Domain)';

:r .\Scripts\Users.sql
:r .\Scripts\RoleMemberships.sql
:r .\Scripts\Permissions.sql

PRINT 'FINISHED POST-DEPLOYMENT ON $(Domain)'