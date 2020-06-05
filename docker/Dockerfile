FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS builder

# 
# install mssql tools for sqlcmd
#
RUN apk add curl
#Download the desired package(s)
RUN curl -O https://download.microsoft.com/download/e/4/e/e4e67866-dffd-428c-aac7-8d28ddafb39b/msodbcsql17_17.5.2.2-1_amd64.apk
RUN curl -O https://download.microsoft.com/download/e/4/e/e4e67866-dffd-428c-aac7-8d28ddafb39b/mssql-tools_17.5.2.1-1_amd64.apk

#Install the package(s)
RUN apk add --allow-untrusted msodbcsql17_17.5.2.2-1_amd64.apk
RUN apk add --allow-untrusted mssql-tools_17.5.2.1-1_amd64.apk

# 

ADD Components /app/Components
ADD ServerStandAlone /app/ServerStandAlone

# TODO: Copy appsettings to correct DB Migration Project

ADD docker/appsettings.Development.json /app/ServerStandAlone/appsettings.Development.json
ADD docker/appsettings.Development.json /app/Components/appsettings.Development.json
ADD docker/start.sh /app/start.sh
WORKDIR /app
ENTRYPOINT ["./start.sh"]
