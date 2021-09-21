call test_A2I
if not %errorlevel%==0 (
	echo A2I failed
	goto quit
)

call test_A2P
if not %errorlevel%==0 (
	echo A2P failed
	goto quit
)

call test_P2A
if not %errorlevel%==0 (
	echo P2A failed
	goto quit
)

echo .
echo all passed :-)


:quit
