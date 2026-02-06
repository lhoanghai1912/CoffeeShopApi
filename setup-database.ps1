# Script để setup database từ đầu (chỉ dùng lần đầu hoặc khi cần reset)

Write-Host "🔧 CoffeeShop Database Setup Script" -ForegroundColor Cyan
Write-Host "====================================`n" -ForegroundColor Cyan

# Chuyển đến thư mục CoffeeShopApi
Set-Location -Path "CoffeeShopApi"

Write-Host "⚠️  CẢNH BÁO: Script này sẽ XÓA TOÀN BỘ DATABASE hiện tại!" -ForegroundColor Yellow
$confirm = Read-Host "Bạn có chắc chắn muốn tiếp tục? (yes/no)"

if ($confirm -ne "yes") {
    Write-Host "❌ Đã hủy." -ForegroundColor Red
    exit
}

Write-Host "`n1️⃣ Đang xóa database cũ..." -ForegroundColor Yellow
dotnet ef database drop --force

if ($LASTEXITCODE -ne 0) {
    Write-Host "   ⚠️  Không tìm thấy database cũ hoặc có lỗi (bỏ qua)" -ForegroundColor Gray
}

Write-Host "`n2️⃣ Đang apply migrations..." -ForegroundColor Yellow
dotnet ef database update

if ($LASTEXITCODE -ne 0) {
    Write-Host "   ❌ Lỗi khi apply migrations!" -ForegroundColor Red
    exit 1
}

Write-Host "`n✅ Database đã được tạo thành công!" -ForegroundColor Green
Write-Host "`n📝 Bước tiếp theo:" -ForegroundColor Cyan
Write-Host "   - Chạy API: dotnet run hoặc F5 trong Visual Studio" -ForegroundColor White
Write-Host "   - Data sẽ được seed tự động khi API khởi động lần đầu" -ForegroundColor White
Write-Host "`n💡 Lưu ý:" -ForegroundColor Cyan
Write-Host "   - Lần chạy tiếp theo, data sẽ KHÔNG bị reset" -ForegroundColor White
Write-Host "   - Chỉ chạy script này khi muốn reset hoàn toàn database`n" -ForegroundColor White
