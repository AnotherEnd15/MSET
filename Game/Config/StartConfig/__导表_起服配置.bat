chcp 65001
set WORKSPACE=..\..
set Tool_DLL=%WORKSPACE%\Share\Tools\bin\Tool.dll
set CONF_ROOT=.

dotnet %Tool_DLL% ^
    --ActionType="start_scene" ^
    --InputPath="Localhost\start_scenes.xlsx" ^
    --OutputPath="Localhost\start_scenes.yaml"



pause