Add-Type -AssemblyName System.Drawing
try {
    $png = [System.Drawing.Image]::FromFile("icon.png")
    $bmp = New-Object System.Drawing.Bitmap($png)
    $hIcon = $bmp.GetHicon()
    $icon = [System.Drawing.Icon]::FromHandle($hIcon)
    $fs = [System.IO.File]::OpenWrite("icon.ico")
    $icon.Save($fs)
    $fs.Close()
    $icon.Dispose()
    $bmp.Dispose()
    $png.Dispose()
    Write-Host "Successfully created icon.ico"
}
catch {
    Write-Error $_.Exception.Message
}
