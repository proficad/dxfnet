echo off

set outpath = %~dp0A2I\out\out.png

if exist outpath del outpath

A2IEXE.exe png yes 5000 "%~dp0A2I\in\in.dxf" "outpath" "c:\temp\nolog.txt"

if exist outpath (
	echo A2I passed
) else (
	echo A2I failed !!!
)

