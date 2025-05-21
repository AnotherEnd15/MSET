set WORKSPACE=..
set LUBAN_DLL=%WORKSPACE%\ThirdParty\Luban\net7.0\Luban.dll
set CONF_ROOT=.


dotnet %LUBAN_DLL% ^
    -t server ^
    -c cs-bin ^
    -d bin ^
    -e Dev ^
    --conf %CONF_ROOT%\luban.conf ^
    -x outputDataDir=..\Config\Excel\ ^
    -x outputCodeDir=..\Server\Model\Generate\Excel ^
    -x pathValidator.rootDir=..\Unity

dotnet %LUBAN_DLL% ^
    -t client ^
    -c cs-bin ^
    -d bin ^
    -e Dev ^
    --conf %CONF_ROOT%\luban.conf ^
    -x outputDataDir=..\Unity\Assets\Bundles\Config ^
    -x outputCodeDir=..\Unity\Assets\Scripts\Codes\Model\Generate\Excel ^
    -x pathValidator.rootDir=..\Unity

pause