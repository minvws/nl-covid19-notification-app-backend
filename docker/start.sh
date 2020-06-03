#!/bin/sh
until PGPASSWORD=mss-dev psql -h db -U postgres -lqt
do
	sleep 5;
done;

echo DB is up

DB=`PGPASSWORD=mss-dev psql -h db -U postgres -lqt | cut -d \| -f 1 | grep mss`

if [ -z $DB ]; then
    echo -n Create database...
    echo create database mss2 | PGPASSWORD=mss-dev psql -h db -U postgres > /dev/null
    echo done.
    echo Running ProvisionDb...
    cd /app/DataUtilities/ProvisionDb
    dotnet run
fi

cd /app/ServerStandAlone
dotnet run