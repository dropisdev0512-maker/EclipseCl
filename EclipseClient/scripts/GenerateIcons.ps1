# Generates module icon PNGs for Eclipse Client
$iconsDir = Join-Path $PSScriptRoot "src\EclipseClient\Assets\Icons"
New-Item -ItemType Directory -Force -Path $iconsDir | Out-Null

Add-Type -AssemblyName System.Drawing

function New-Icon($name, $r, $g, $b, $symbol) {
    $bmp = New-Object System.Drawing.Bitmap 32, 32
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.Clear([System.Drawing.Color]::FromArgb(0, 0, 0, 0))
    $brush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, $r, $g, $b))
    $g.FillEllipse($brush, 2, 2, 28, 28)
    $font = New-Object System.Drawing.Font("Segoe UI", 12, [System.Drawing.FontStyle]::Bold)
    $sf = New-Object System.Drawing.StringFormat
    $sf.Alignment = [System.Drawing.StringAlignment]::Center
    $sf.LineAlignment = [System.Drawing.StringAlignment]::Center
    $g.DrawString($symbol, $font, [System.Drawing.Brushes]::White, (New-Object System.Drawing.RectangleF(0,0,32,32)), $sf)
    $path = Join-Path $iconsDir "$name.png"
    $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $bmp.Dispose()
    Write-Host "Created $name.png"
}

New-Icon "crystal" 100 80 255 "◆"
New-Icon "totem" 255 180 60 "T"
New-Icon "lightning" 255 220 80 "⚡"
New-Icon "sword" 200 200 220 "⚔"
New-Icon "crosshair" 255 80 80 "⊕"
New-Icon "potion" 255 80 180 "P"
New-Icon "shield" 80 160 255 "S"
New-Icon "hitbox" 180 255 100 "H"
New-Icon "jump" 100 255 180 "J"
New-Icon "click" 255 140 100 "C"
New-Icon "pearl" 180 100 255 "E"
New-Icon "anchor" 140 140 200 "A"
New-Icon "stealth" 80 80 100 "👁"
New-Icon "eye" 100 200 255 "O"
New-Icon "firework" 255 100 100 "F"
New-Icon "lag" 255 200 80 "L"
New-Icon "pack" 160 120 80 "K"
New-Icon "bright" 255 255 150 "B"
New-Icon "xp" 80 255 120 "X"

# App icon
$icoDir = Join-Path $PSScriptRoot "src\EclipseClient\Assets"
New-Item -ItemType Directory -Force -Path $icoDir | Out-Null
$appBmp = New-Object System.Drawing.Bitmap 64, 64
$ag = [System.Drawing.Graphics]::FromImage($appBmp)
$ag.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$ag.Clear([System.Drawing.Color]::FromArgb(255, 8, 8, 14))
$ag.FillEllipse([System.Drawing.Brushes]::DarkSlateBlue, 4, 4, 56, 56)
$font = New-Object System.Drawing.Font("Segoe UI", 24, [System.Drawing.FontStyle]::Bold)
$ag.DrawString("E", $font, [System.Drawing.Brushes]::White, 18, 14)
$iconPath = Join-Path $icoDir "eclipse.ico"
$stream = [System.IO.File]::Create($iconPath)
$appBmp.Save($stream, [System.Drawing.Imaging.ImageFormat]::Icon)
$stream.Close()
$ag.Dispose(); $appBmp.Dispose()
Write-Host "Created eclipse.ico"
Write-Host "Done!"
