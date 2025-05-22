set WORKSPACE=..
set Tool_DLL=%WORKSPACE%\Share\Tools\bin\Tool.dll

dotnet %Tool_DLL% ^
    --ActionType="proto" ^
    --Proto.ServerOutputPath=..\Server\Model\Generate\Proto ^
    --Proto.ClientOutputPath=..\Unity\Assets\Scripts\Model\Generate\Proto

pause