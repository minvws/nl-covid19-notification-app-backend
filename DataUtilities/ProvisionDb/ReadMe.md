# Provision Database

This adds the tables and some example data for RIVM Advice.

1. Download dotnet SDK: https://dotnet.microsoft.com/download/dotnet-core/3.1
1. Clone this repo
1. Choose your database
1. Add an `appsettings.Development.json` file. This overrides the settings in appsettings.json. And add a value for the MSS connection string.
1. Run provisiondb from the command line without building the project : dotnet run nameofdatabase

