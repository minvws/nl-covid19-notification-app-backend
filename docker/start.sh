#!/bin/sh
#until MSSQL_PASSWORD=mss-dev psql -h db -U postgres -lqt
#do
	sleep 500; # TODO remove debugging, for bashing directly into container to test
#done;

echo DB is up

# TODO Check if DB mss exists

#DB=`MSSQL_PASSWORD=mss-dev psql -h db -U sa -lqt | cut -d \| -f 1 | grep mss`

#if [ -z $DB ]; then
    echo -n Create database...
    
    /opt/mssql-tools/bin/sqlcmd -S db_mssql -P "mss-nl-covid-19-dev" -U sa -Q "CREATE DATABASE mss" 
   
#    echo CREATE DATABASE mss2 | MSSQL_PASSWORD=mss-dev psql -h db -U sa > /dev/null
    echo done.
    
    # TODO: PROVISIONDB IN CURRENT PROJECTVERSION
    
#    echo Running ProvisionDb...
#    cd /app/DataUtilities/ProvisionDb
#    dotnet run
#fi

cd /app/ServerStandAlone
dotnet run