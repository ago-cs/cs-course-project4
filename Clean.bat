@echo off
@echo Deleting all BIN, OBJ and TestResults folders...
for /d /r . %%d in (bin,obj) do @if exist "%%d" rd /s/q "%%d"
for /d /r . %%d in (.vs) do @if exist "%%d" rd /s/q "%%d"
for /d /r . %%d in (TestResults) do @if exist "%%d" rd /s/q "%%d"
@echo BIN and OBJ folders successfully deleted :) Close the window.
pause > nul
