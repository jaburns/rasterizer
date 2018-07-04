Add-Type -AssemblyName System.IO.Compression.FileSystem

$scriptpath = $MyInvocation.MyCommand.Path
$dir = Split-Path $scriptpath
Push-Location $dir
Push-Location external
[Environment]::CurrentDirectory = $PWD

Write-Output "Extracting SDL 2.0.8..."
Remove-Item ".\SDL2-2.0.8" -Recurse -ErrorAction Ignore
[System.IO.Compression.ZipFile]::ExtractToDirectory("SDL2-devel-2.0.8-VC.zip", ".")

Write-Output "Copying DLLs to project root..."
Copy-Item "SDL2-2.0.8/lib/x64/SDL2.dll" ".."

Write-Output "Done!"

Pop-Location
Pop-Location
[Environment]::CurrentDirectory = $PWD
