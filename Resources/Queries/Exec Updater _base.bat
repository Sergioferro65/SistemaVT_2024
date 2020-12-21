:do
CALL UpdateDb.bat localhost Bugs

:choice
set /P c=Again [Y] / [N]?
if /I "%c%" EQU "Y" goto :do
if /I "%c%" EQU "N" goto :exit
goto :choice
:exit