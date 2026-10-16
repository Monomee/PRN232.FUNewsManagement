# Script dung nhanh cac service dang chay tren cac cong 5132, 5133, 5134, 5237
Write-Host "Dang dung cac service FUNewsManagement..." -ForegroundColor Yellow

$ports = @(5132, 5133, 5134, 5237)
foreach ($port in $ports) {
    $processes = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue | Select-Object -ExpandProperty OwningProcess -Unique
    foreach ($pidNum in $processes) {
        if ($pidNum -gt 0) {
            Stop-Process -Id $pidNum -Force -ErrorAction SilentlyContinue
            Write-Host "Da dung process PID $pidNum tren port $port" -ForegroundColor Green
        }
    }
}

Write-Host "Hoan tat dung cac service." -ForegroundColor Cyan
