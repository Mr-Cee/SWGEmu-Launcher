@echo off
pushd "%~dp0"
call "C:\Program Files\Microsoft Visual Studio\18\Community\VC\Auxiliary\Build\vcvars32.bat" >nul
if errorlevel 1 exit /b 1
cl /nologo /std:c++17 /W4 /O2 /MT /EHsc /LD /DUNICODE /D_UNICODE travel.cpp /link /DEF:travel.def /OUT:dinput8.dll user32.lib
