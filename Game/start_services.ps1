<#
.SYNOPSIS
    ��������Զ������ű�
.DESCRIPTION
    �Զ���鲢����MongoDB��ETCD���񣬷�����ǰ̨��������
.AUTHOR
    �Ż��汾
#>

# ������Ҫ��Ŀ¼
Write-Host "���ڼ�鲢������Ҫ��Ŀ¼..." -ForegroundColor Cyan
$directories = @("Bin\data\db", "Bin\etcd")

foreach ($dir in $directories) {
    if (-not (Test-Path $dir)) {
        Write-Host "����Ŀ¼: $dir" -ForegroundColor Yellow
        New-Item -Path $dir -ItemType Directory -Force | Out-Null
    } else {
        Write-Host "Ŀ¼�Ѵ���: $dir" -ForegroundColor Green
    }
}
Write-Host "Ŀ¼������" -ForegroundColor Green
Write-Host ""

# ������̼�����������
function Start-ServiceProcess {
    param (
        [string]$serviceName,
        [string]$executablePath,
        [string]$arguments
    )
    
    Write-Host "��� $serviceName ����״̬..." -ForegroundColor Cyan
    
    # �������Ƿ�������
    $processInfo = Get-Process | Where-Object { 
        $_.ProcessName -eq $serviceName -or 
        ($_.Path -ne $null -and $_.Path.EndsWith("$serviceName.exe")) 
    } -ErrorAction SilentlyContinue
    
    if ($processInfo) {
        Write-Host "[������] $serviceName �����������У�����ID: $($processInfo.Id)" -ForegroundColor Green
    } else {
        Write-Host "[������] �������� $serviceName ����..." -ForegroundColor Yellow
        
        try {
            # ʹ��Start-Process������ǰ̨����
            $process = Start-Process -FilePath $executablePath `
                                    -ArgumentList $arguments `
                                    -WindowStyle Normal `
                                    -PassThru `
                                    -ErrorAction Stop
            
            # �ȴ�����ʱ�䣬ȷ�Ͻ�������
            Start-Sleep -Seconds 2
            
            if (-not (Get-Process -Id $process.Id -ErrorAction SilentlyContinue)) {
                Write-Host "[����] $serviceName ����δ����������������־" -ForegroundColor Red
            } else {
                Write-Host "[������] $serviceName ���������ɹ�������ID: $($process.Id)" -ForegroundColor Green
            }
        }
        catch {
            Write-Host "[����] ���� $serviceName ����ʧ��: $_" -ForegroundColor Red
        }
    }
    Write-Host ""
}

# ������ʼ
Write-Host "===== ��������� =====" -ForegroundColor Magenta

# ���ó�ʱ�ʹ������ƫ��
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

# ���������Ϣ
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

# �������з��񲢴���
foreach ($service in $services) {
    Start-ServiceProcess -serviceName $service.Name -executablePath $service.Path -arguments $service.Args
}

Write-Host "===== ���з�������� =====" -ForegroundColor Magenta
Write-Host "�������ڸ��ԵĴ���������" -ForegroundColor Green
Write-Host "�ű�����3����Զ��ر�..." -ForegroundColor Green

# �ȴ�һ��ʱ����˳������û���ʱ�俴�����
Start-Sleep -Seconds 3
exit