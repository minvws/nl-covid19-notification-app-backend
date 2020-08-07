@echo off

dotnet test --no-build --configuration Release --verbosity normal

@echo on

IF %ERRORLEVEL% NEQ 0 EXIT 1