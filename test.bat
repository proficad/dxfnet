echo off

A2IEXE.exe png yes 5000 "%~dp0A2I\in\in.dxf" "%~dp0A2I\out\out.png" "c:\temp\nolog.txt"

if exist "%~dp0A2I\out\out.png" (
	echo A2I passed
) else (
	echo A2I failed !!!
)

