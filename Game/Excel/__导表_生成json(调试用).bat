set WORKSPACE=..
set LUBAN_DLL=%WORKSPACE%\Tools\Luban\Luban.dll
set CONF_ROOT=.

dotnet %LUBAN_DLL% ^
    -t server ^
    -c cs-bin ^
    -d bin ^
    --conf %CONF_ROOT%\luban.conf ^
    -x outputDataDir=..\Config\Excel\ ^
    -x outputCodeDir=..\DotNet\Model\Generate\Excel ^
    -x pathValidator.rootDir=..\Unity

dotnet %LUBAN_DLL% ^
    -t server ^
    -d json ^
    --conf %CONF_ROOT%\luban.conf ^
    -x outputDataDir=.\Json ^
    -x pathValidator.rootDir=..\Unity

dotnet %LUBAN_DLL% ^
    -t client ^
    -c cs-bin ^
    -d bin ^
    --conf %CONF_ROOT%\luban.conf ^
    -x outputDataDir=..\Unity\Assets\Bundles\Config ^
    -x outputCodeDir=..\Unity\Assets\Scripts\Model\Generate\Excel ^
    -x pathValidator.rootDir=..\Unity

pause