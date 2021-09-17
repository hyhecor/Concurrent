@ECHO OFF

REM set date & time
set year=%date:~-10,4%
set month=%date:~-5,2%
set day=%date:~-2,2%
set hour=%time:~0,2%
if "%hour:~0,1%" == " " set hour=0%hour:~1,1%
set min=%time:~3,2%
if "%min:~0,1%" == " " set min=0%min:~1,1%
set secs=%time:~6,2%
if "%secs:~0,1%" == " " set secs=0%secs:~1,1%

REM set directory, file and linkfile names
SET logdir=.\TestResult
SET logfile=TestResult_%year%%month%%day%%hour%%min%%secs%.log
SET linklast=TestResult_last.log

REM set dotnet test command
SET command=dotnet test --verbosity quiet --configuration Release Concurrent.sln

REM __CREATE DIRECTORY__
REM create directory if not exist
IF NOT EXIST %logdir% (
	MKDIR %logdir%
)

REM __CREATE LOGFILE AND SET UPDATE LINK FILE__
REM create file with echo 
ECHO %%%%LOGFILE(%logfile%) > %logdir%\%logfile%
ECHO %%%%SYSTEMINFO >> %logdir%\%logfile%
ECHO %%%%( >> %logdir%\%logfile%
systeminfo /FO LIST >> %logdir%\%logfile%
ECHO %%%%) >> %logdir%\%logfile%
ECHO %%%%USERPROFILE(%USERPROFILE%) >> %logdir%\%logfile%

REM remove the link file if exist 
IF EXIST %logdir%\%linklast% (
	DEL %logdir%\%linklast% 
)

REM make this file to the last link file
mklink /h %logdir%\%linklast% %logdir%\%logfile%

REM __RUN TEST__
ECHO RUN CMD(%command%)
REM logging command 
ECHO %%%%RUN CMD(%command%) >> %logdir%\%logfile%
REM run test command
CALL %command% >> %logdir%\%logfile%

REM write return code 
ECHO FIN(%ERRORLEVEL%)
ECHO %%%%FIN(%ERRORLEVEL%) >> %logdir%\%logfile%
REM __TEST FINISHED__

REM __REPORT__
REM report option
SET report="~notepad"
REM if report true then report with notepad
if report=="notepad" (
	START notepad %logdir%\%logfile%
)
