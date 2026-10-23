[CmdletBinding()]
param(
    [string]$Sts2ApiSignatureRoot = (Join-Path (Split-Path $PSScriptRoot) 'sts2-api-signatures'),
    [string]$Configuration = 'Debug'
)
$ErrorActionPreference = 'Stop'
$projects = @(Get-ChildItem -LiteralPath $PSScriptRoot -Filter '*.csproj')
if ($projects.Count -ne 1) { throw 'Expected exactly one mod project.' }
$project = $projects[0]
$modName = $project.BaseName
$payloadRoot = Join-Path $PSScriptRoot '.godot\universal\payloads'
[xml]$local = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'local.props') -Raw
$gameDir = [string]$local.Project.PropertyGroup.Sts2Dir
if (!(Test-Path -LiteralPath $gameDir -PathType Container)) { throw "Game directory not found: $gameDir" }
$godotExe = [string]$local.Project.PropertyGroup.GodotExe
if (!(Test-Path -LiteralPath $godotExe -PathType Leaf)) { throw "MegaDot executable not found: $godotExe" }
if ($env:STS2_SKIP_PCK_EXPORT -eq '1') { throw 'PCK export must be enabled for universal packaging.' }
$modDir = Join-Path $gameDir "mods\$modName"
foreach ($version in '0.107.1', '0.111.0') {
    $buildStarted = [DateTime]::UtcNow
    & dotnet build $project.FullName --nologo -v:minimal "-p:Configuration=$Configuration" "-p:Sts2ApiCompat=$version" "-p:Sts2ApiSignatureRoot=$Sts2ApiSignatureRoot"
    if ($LASTEXITCODE -ne 0) { throw "Build failed for $version" }
    $destination = Join-Path $payloadRoot $version
    New-Item -ItemType Directory -Path $destination -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $modDir "$modName.dll") -Destination $destination
    if (!(Test-Path -LiteralPath (Join-Path $modDir "$modName.pck"))) { throw "Missing PCK for $version" }
    if ((Get-Item -LiteralPath (Join-Path $modDir "$modName.pck")).LastWriteTimeUtc -lt $buildStarted) {
        throw "PCK was not exported by the $version build."
    }
}
$output = Join-Path $PSScriptRoot '.godot\universal\bootstrap'
& dotnet build (Join-Path $PSScriptRoot 'Universal\Bootstrap.csproj') --nologo -v:minimal "-p:Configuration=$Configuration" "-p:ModAssemblyName=$modName" "-p:PayloadRoot=$payloadRoot" "-p:Sts2ApiSignatureRoot=$Sts2ApiSignatureRoot" "-o:$output"
if ($LASTEXITCODE -ne 0) { throw 'Universal bootstrap build failed' }
Copy-Item -LiteralPath (Join-Path $output "$modName.Bootstrap.dll") -Destination (Join-Path $modDir "$modName.dll")
Write-Output "Universal mod installed: $modDir"
