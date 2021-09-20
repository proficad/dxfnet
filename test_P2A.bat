echo off

set outpath = %~dp0P2A\out\out.dxf


P2A "%~dp0P2A\in\in.sxe" "outpath" "c:\temp\nologP2A.txt"

if exist outpath (
	echo P2A passed
) else (
	echo P2A failed !!!
)


