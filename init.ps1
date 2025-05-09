function AddDirToEnvPath($dir){
    if (-not ($env:PATH -split [System.IO.Path]::PathSeparator -contains $dir)) {
        $env:PATH += [System.IO.Path]::PathSeparator + $dir
    }
}

function NormalizePath {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]
        [string] $path
    )

    $separator = [IO.Path]::DirectorySeparatorChar
    $invalidSeparator = [IO.Path]::AltDirectorySeparatorChar
    $path = $path -replace $invalidSeparator, $separator

    return $path
}

function EnsurePathsExist {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory = $true)]
        [string[]]$paths
    )

    $invalidPaths = @()

    foreach ($item in $paths) {
        if (-not (Test-Path $item)) {
            $invalidPaths += $item
        }
    }

    if($invalidPaths.Count -gt 0){
        throw "Some paths that the powershell script environment needs are missing : $invalidPaths"
    }
}

$root = $PSScriptRoot
Write-Host "root: $root"
$scriptsPath = Join-Path $root "scripts"

EnsurePathsExist @(
    $scriptsPath)

# add script paths to run dev scripts
AddDirToEnvPath $scriptsPath
