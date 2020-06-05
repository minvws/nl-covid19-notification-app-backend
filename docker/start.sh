#!/bin/sh

cd /app/ServerStandAlone
dotnet run


# README: Rest of script was to prepare Postgres SQL, partly converted to MSSQL, but unnecessary since latest API updates.
#
#until MSSQL_PASSWORD=mss-dev psql -h db -U postgres -lqt
#do
#	sleep 5; # XTODO remove debugging, for bashing directly into container to test
#done;
#
#echo DB is up
#
# XTODO Check if DB mss exists
#
#DB=`MSSQL_PASSWORD=mss-dev psql -h db -U sa -lqt | cut -d \| -f 1 | grep mss`
#
#if [ -z $DB ]; then
#    echo -n Create database...
#    
#    /opt/mssql-tools/bin/sqlcmd -S db_mssql -P "mss-nl-covid-19-dev" -U sa -Q "CREATE DATABASE mss" 
#   
#    echo CREATE DATABASE mss2 | MSSQL_PASSWORD=mss-dev psql -h db -U sa > /dev/null
#    echo done.
#    
#     XTODO: PROVISIONDB IN CURRENT PROJECTVERSION
#    
#    echo Running ProvisionDb...
#    cd /app/DataUtilities/ProvisionDb
#    dotnet run
#fi

### Add following to Dockerfile to use sqlcmd in web container scripts
## 
## install mssql tools for sqlcmd
##
#RUN apk add curl
##Download the desired package(s)
#RUN curl -O https://download.microsoft.com/download/e/4/e/e4e67866-dffd-428c-aac7-8d28ddafb39b/msodbcsql17_17.5.2.2-1_amd64.apk
#RUN curl -O https://download.microsoft.com/download/e/4/e/e4e67866-dffd-428c-aac7-8d28ddafb39b/mssql-tools_17.5.2.1-1_amd64.apk
#
##Install the package(s)
#RUN apk add --allow-untrusted msodbcsql17_17.5.2.2-1_amd64.apk
#RUN apk add --allow-untrusted mssql-tools_17.5.2.1-1_amd64.apk
#
## 


