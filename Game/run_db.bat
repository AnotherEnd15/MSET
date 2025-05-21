chcp 65001
@echo off
echo 正在启动服务管理脚本...

:: 设置工作目录
cd /d "%~dp0"
echo 当前工作目录: %CD%

:: 启动PowerShell脚本
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0start_services.ps1" -Verbose