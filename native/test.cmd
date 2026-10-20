@echo off
pushd "%~dp0"
call "C:\Program Files\Microsoft Visual Studio\18\Community\VC\Auxiliary\Build\vcvars32.bat" >nul
if errorlevel 1 exit /b 1
cl /nologo /std:c++17 /W4 /O2 /MT /EHsc test.cpp /link /OUT:native-test.exe user32.lib
if errorlevel 1 exit /b 1
native-test.exe
