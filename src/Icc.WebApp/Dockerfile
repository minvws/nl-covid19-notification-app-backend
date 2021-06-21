#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
RUN apt-get update -yq \
    && apt-get install curl gnupg -yq \
    && curl -sL https://deb.nodesource.com/setup_10.x | bash \
    && apt-get install nodejs -yq

WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
RUN apt-get update -yq \
    && apt-get install curl gnupg -yq \
    && curl -sL https://deb.nodesource.com/setup_10.x | bash \
    && apt-get install nodejs -yq

WORKDIR /src
COPY ["Icc.WebApp/Icc.WebApp.csproj", "Icc.WebApp/"]
COPY ["Icc.Commands/Icc.Commands.csproj", "Icc.Commands/"]
COPY ["MobileAppApi.EntityFramework/MobileAppApi.EntityFramework.csproj", "MobileAppApi.EntityFramework/"]
COPY ["MobileAppApi.Entities/MobileAppApi.Entities.csproj", "MobileAppApi.Entities/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Core/Core.csproj", "Core/"]
COPY ["Core.EntityFramework/Core.EntityFramework.csproj", "Core.EntityFramework/"]
COPY ["Core.AspNet/Core.AspNet.csproj", "Core.AspNet/"]
RUN dotnet restore "Icc.WebApp/Icc.WebApp.csproj"
COPY . .
WORKDIR "/src/Icc.WebApp"
RUN dotnet build "Icc.WebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Icc.WebApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Icc.WebApp"]