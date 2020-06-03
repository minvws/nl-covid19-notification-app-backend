FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS builder
RUN apk add --no-cache postgresql-client
ADD Components /app/Components
ADD DataUtilities /app/DataUtilities
ADD ServerStandAlone /app/ServerStandAlone
ADD docker/appsettings.Development.json /app/ServerStandAlone/appsettings.Development.json
ADD docker/appsettings.Development.json /app/DataUtilities/ProvisionDb/appsettings.Development.json
ADD docker/start.sh /app/start.sh
WORKDIR /app
ENTRYPOINT ["./start.sh"]
