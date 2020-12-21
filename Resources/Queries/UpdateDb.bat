@echo OFF
SETLOCAL
SET server=%1
SET dbName=%2
SET mode=%3
SET path=%~dp0
SET ignoreFakeData=1

IF %server%.==. GOTO NoServer
IF %dbName%.==. GOTO NoDbName
IF %mode%.==. SET mode=update 
GOTO Main

:Main
IF %ignoreFakeData%==1 (
..\..\..\database-updater\DatabaseUpdater.Core\bin\Debug\DatabaseUpdater.Core.exe -s %server% -d %dbName% -m %mode% -i -p "%path%"
) ELSE (
..\..\..\database-updater\DatabaseUpdater.Core\bin\Debug\DatabaseUpdater.Core.exe -s %server% -d %dbName% -m %mode% -p "%path%"
)
GOTO End



:NoServer
ECHO Must include param 1: server
GOTO End

:NoDbName
ECHO Must include param 2: dbName
GOTO End

:End
ENDLOCAL
exit /B 1