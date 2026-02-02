# Script rename tất cả file ảnh về lowercase
# Giữ nguyên file placeholder.jpg và các file product_*.jpg

$imagesPath = "wwwroot\images"
$renamed = 0
$skipped = 0

Write-Host "====================================" -ForegroundColor Cyan
Write-Host "Rename Images to Lowercase Script" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

# Kiểm tra thư mục tồn tại
if (-not (Test-Path $imagesPath)) {
    Write-Host "ERROR: Thư mục $imagesPath không tồn tại!" -ForegroundColor Red
    exit 1
}

Get-ChildItem $imagesPath -Filter "*.jpg" | ForEach-Object {
    $oldName = $_.Name
    $newName = $_.Name.ToLower()
    
    # Bỏ qua file đã lowercase hoặc file placeholder/product_*
    if ($oldName -eq $newName) {
        Write-Host "SKIP: $oldName (already lowercase)" -ForegroundColor Gray
        $skipped++
    }
    elseif ($oldName -like "placeholder*" -or $oldName -like "product_*") {
        Write-Host "SKIP: $oldName (system file)" -ForegroundColor Yellow
        $skipped++
    }
    else {
        try {
            Rename-Item -Path $_.FullName -NewName $newName -Force
            Write-Host "RENAME: $oldName -> $newName" -ForegroundColor Green
            $renamed++
        }
        catch {
            Write-Host "ERROR: Cannot rename $oldName - $_" -ForegroundColor Red
        }
    }
}

Write-Host ""
Write-Host "====================================" -ForegroundColor Cyan
Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "  Renamed: $renamed files" -ForegroundColor Green
Write-Host "  Skipped: $skipped files" -ForegroundColor Yellow
Write-Host "====================================" -ForegroundColor Cyan
