@ECHO OFF

NET SESSION >nul 2>&1
IF %ERRORLEVEL% EQU 0 (
    %~dp0\LocalSTS.exe "%~dp0\LocalSTS.exe.config" -Start
	PAUSE
) ELSE (
    ECHO Ensure this command is run as an administrator
	PAUSE
	EXIT /B 1
)