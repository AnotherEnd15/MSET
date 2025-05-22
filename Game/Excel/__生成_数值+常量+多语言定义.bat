chcp 65001
set WORKSPACE=..
set Tool_DLL=%WORKSPACE%\Share\Tools\bin\Tool.dll

dotnet %Tool_DLL% ^
    --ActionType="numeric" ^
    --InputPath="Datas\S.数值\S.数值定义表.xlsx" ^
    --OutputPath="Defines\自动生成.Numeric.xml"

dotnet %Tool_DLL% ^
    --ActionType="lang" ^
    --InputPath="Datas\D.多语言表.xlsx" ^
    --OutputPath="..\Unity\Assets\Scripts\Model\Generate\LanguageKey.cs"

dotnet %Tool_DLL% ^
    --ActionType="lang" ^
    --InputPath="Datas\D.多语言表.xlsx" ^
    --OutputPath="..\Server\Model\Generate\LanguageKey.cs"

dotnet %Tool_DLL% ^
    --ActionType="errorcode" ^
    --InputPath="Datas\C.错误码表.xlsx" ^
    --OutputPath="..\Unity\Assets\Scripts\Model\Generate\ErrorCode.cs"

dotnet %Tool_DLL% ^
    --ActionType="errorcode" ^
    --InputPath="Datas\C.错误码表.xlsx" ^
    --OutputPath="..\Server\Model\Generate\ErrorCode.cs"

dotnet %Tool_DLL% ^
    --ActionType="constvalue" ^
    --InputPath="Datas\C.常量表.xlsx" ^
    --OutputPath="..\Unity\Assets\Scripts\Model\Generate\ConstValue.cs"

dotnet %Tool_DLL% ^
    --ActionType="constvalue" ^
    --InputPath="Datas\C.常量表.xlsx" ^
    --OutputPath="..\Server\Model\Generate\ConstValue.cs"

pause