<#
.SYNOPSIS
    管理服务自动启动脚本
.DESCRIPTION
    自动检查并启动MongoDB和ETCD服务，服务将在前台窗口运行
.AUTHOR
    优化版本
#>

# 创建必要的目录
Write-Host "正在检查并创建必要的目录..." -ForegroundColor Cyan
$directories = @("Bin\data\db", "Bin\etcd")

foreach ($dir in $directories) {
    if (-not (Test-Path $dir)) {
        Write-Host "创建目录: $dir" -ForegroundColor Yellow
        New-Item -Path $dir -ItemType Directory -Force | Out-Null
    } else {
        Write-Host "目录已存在: $dir" -ForegroundColor Green
    }
}
Write-Host "目录检查完成" -ForegroundColor Green
Write-Host ""

# 定义进程检查和启动函数
function Start-ServiceProcess {
    param (
        [string]$serviceName,
        [string]$executablePath,
        [string]$arguments
    )
    
    Write-Host "检查 $serviceName 服务状态..." -ForegroundColor Cyan
    
    # 检查进程是否在运行
    $processInfo = Get-Process | Where-Object { 
        $_.ProcessName -eq $serviceName -or 
        ($_.Path -ne $null -and $_.Path.EndsWith("$serviceName.exe")) 
    } -ErrorAction SilentlyContinue
    
    if ($processInfo) {
        Write-Host "[已运行] $serviceName 服务已在运行，进程ID: $($processInfo.Id)" -ForegroundColor Green
    } else {
        Write-Host "[启动中] 正在启动 $serviceName 服务..." -ForegroundColor Yellow
        
        try {
            # 使用Start-Process并设置前台窗口
            $process = Start-Process -FilePath $executablePath `
                                    -ArgumentList $arguments `
                                    -WindowStyle Normal `
                                    -PassThru `
                                    -ErrorAction Stop
            
            # 等待短暂时间，确认进程启动
            Start-Sleep -Seconds 2
            
            if (-not (Get-Process -Id $process.Id -ErrorAction SilentlyContinue)) {
                Write-Host "[警告] $serviceName 可能未正常启动，请检查日志" -ForegroundColor Red
            } else {
                Write-Host "[已启动] $serviceName 服务启动成功，进程ID: $($process.Id)" -ForegroundColor Green
            }
        }
        catch {
            Write-Host "[错误] 启动 $serviceName 服务失败: $_" -ForegroundColor Red
        }
    }
    Write-Host ""
}

# 主程序开始
Write-Host "===== 服务管理器 =====" -ForegroundColor Magenta

# 设置超时和错误操作偏好
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

# 定义服务信息
$services = @(
    @{
        Name = "mongod"
        Path = "ThirdParty\mongodb\mongod.exe"
        Args = "--dbpath Bin/data/db"
    },
    @{
        Name = "etcd"
        Path = "ThirdParty\etcd\etcd.exe"
        Args = "--data-dir Bin/etcd"
    }
)

# 遍历所有服务并处理
foreach ($service in $services) {
    Start-ServiceProcess -serviceName $service.Name -executablePath $service.Path -arguments $service.Args
}

Write-Host "===== 所有服务处理完成 =====" -ForegroundColor Magenta
Write-Host "服务已在各自的窗口中启动" -ForegroundColor Green
Write-Host "脚本将在3秒后自动关闭..." -ForegroundColor Green

# 等待一段时间后退出，让用户有时间看到结果
Start-Sleep -Seconds 3
exit