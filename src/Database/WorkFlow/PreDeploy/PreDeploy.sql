PRINT 'START PRE-DEPLOYMENT ON $(Domain)'

:r .\Scripts\DropColumns.sql
:r .\Scripts\SetIsOriginPortal.sql

PRINT 'FINISHED PRE-DEPLOYMENT ON $(Domain)'