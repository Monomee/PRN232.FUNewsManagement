# Script khoi chay 4 service cua he thong FUNewsManagementSystem v2
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "  Khoi dong FUNewsManagementSystem v2 (PRN232 Assignment) " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$root = $PSScriptRoot

# 1. Core API (Port 5132) - Backend goc, xu ly Auth, Database, SignalR Hub
Write-Host "[1/4] Dang khoi dong Core API (Port 5132)..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "`$Host.UI.RawUI.WindowTitle='[1] Core API (:5132)'; cd '$root\FUNewsManagement_CoreAPI\FUNewsManagement_CoreAPI'; dotnet run --urls 'http://localhost:5132'"
Start-Sleep -Seconds 3

# 2. Analytics API (Port 5133) - Xu ly thong ke, trending, export Excel
Write-Host "[2/4] Dang khoi dong Analytics API (Port 5133)..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "`$Host.UI.RawUI.WindowTitle='[2] Analytics API (:5133)'; cd '$root\FUNewsManagement_AnalyticsAPI\FUNewsManagement_AnalyticsAPI'; dotnet run --urls 'http://localhost:5133'"
Start-Sleep -Seconds 2

# 3. AI API (Port 5134) - Xu ly goi y Tag bang Google AI Studio / Gemini
Write-Host "[3/4] Dang khoi dong AI API (Port 5134)..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "`$Host.UI.RawUI.WindowTitle='[3] AI API (:5134)'; cd '$root\FUNewsManagement_AIAPI\FUNewsManagement_AIAPI'; dotnet run --urls 'http://localhost:5134'"
Start-Sleep -Seconds 2

# 4. Frontend (Port 5237) - Giao dien Razor Pages nguoi dung
Write-Host "[4/4] Dang khoi dong Frontend Web App (Port 5237)..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "`$Host.UI.RawUI.WindowTitle='[4] Frontend (:5237)'; cd '$root\FUNewsManagement_FE\FUNewsManagement_FE'; dotnet run --urls 'http://localhost:5237'"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "Tat ca 4 service da duoc khoi chay tren cac terminal rieng:" -ForegroundColor Green
Write-Host " - Frontend UI:        http://localhost:5237" -ForegroundColor Yellow
Write-Host " - Core API Swagger:   http://localhost:5132/swagger" -ForegroundColor DarkCyan
Write-Host " - Analytics Swagger:  http://localhost:5133/swagger" -ForegroundColor DarkCyan
Write-Host " - AI API Swagger:     http://localhost:5134/swagger" -ForegroundColor DarkCyan
Write-Host "==========================================================" -ForegroundColor Cyan
