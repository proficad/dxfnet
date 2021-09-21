echo off

set outpath = %~dp0A2P\out\in.ppd


rem if exist outpath del outpath

A2P "%~dp0A2P\in\in.dxf" "%~dp0A2P\out" ppd yes 20 "c:\temp\nologA2P.txt"

if exist %~dp0A2P\out\in.ppd (
	echo A2P passed
	exit /b 0
) else (
	echo A2P failed !!!
	exit /b 1
)


