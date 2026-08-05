param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("win-x64", "linux-x64", "linux-arm64", "osx-x64", "osx-arm64")]
    [string] $RuntimeIdentifier,

    [string] $OutputRoot = (Join-Path $PSScriptRoot "..\artifacts\packages")
)

$ErrorActionPreference = "Stop"
$repositoryRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$outputRoot = [IO.Path]::GetFullPath($OutputRoot)
$allowedRoot = [IO.Path]::GetFullPath((Join-Path $repositoryRoot "artifacts"))
$allowedPrefix = $allowedRoot.TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
if ($outputRoot -ne $allowedRoot -and -not $outputRoot.StartsWith($allowedPrefix, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Package output must stay under $allowedRoot."
}

$packageName = "espada-$RuntimeIdentifier"
$packageRoot = Join-Path $outputRoot $packageName
$publishRoot = Join-Path $outputRoot ".publish-$RuntimeIdentifier"
if (Test-Path -LiteralPath $packageRoot) {
    Remove-Item -LiteralPath $packageRoot -Recurse -Force
}
if (Test-Path -LiteralPath $publishRoot) {
    Remove-Item -LiteralPath $publishRoot -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $packageRoot, $publishRoot | Out-Null

Push-Location (Join-Path $repositoryRoot "src\Espada.Web")
try {
    & npm ci
    if ($LASTEXITCODE -ne 0) {
        throw "npm ci failed."
    }
    & npm run build
    if ($LASTEXITCODE -ne 0) {
        throw "The production web build failed."
    }
}
finally {
    Pop-Location
}

$projects = [ordered]@{
    "cli" = "src\Espada.Cli\Espada.Cli.csproj"
    "daemon" = "src\Espada.Daemon\Espada.Daemon.csproj"
    "api" = "src\Espada.Api\Espada.Api.csproj"
    "mcp" = "src\Espada.Mcp\Espada.Mcp.csproj"
    "worker" = "src\Espada.Worker\Espada.Worker.csproj"
    "db" = "src\Espada.Db\Espada.Db.csproj"
}
foreach ($entry in $projects.GetEnumerator()) {
    $destination = Join-Path $publishRoot $entry.Key
    $publishArguments = @(
        "publish",
        (Join-Path $repositoryRoot $entry.Value),
        "--configuration", "Release",
        "--runtime", $RuntimeIdentifier,
        "--self-contained", "true",
        "-p:PublishSingleFile=true",
        "-p:PublishTrimmed=false",
        "-p:IncludeNativeLibrariesForSelfExtract=true",
        "-p:DebugType=None",
        "--output", $destination
    )
    & dotnet @publishArguments
    if ($LASTEXITCODE -ne 0) {
        throw "Publishing $($entry.Key) failed."
    }
}

$extension = if ($RuntimeIdentifier.StartsWith("win-")) { ".exe" } else { "" }
Copy-Item -LiteralPath (Join-Path $publishRoot "cli\Espada.Cli$extension") -Destination (Join-Path $packageRoot "espada$extension")
Copy-Item -LiteralPath (Join-Path $publishRoot "daemon\Espada.Daemon$extension") -Destination (Join-Path $packageRoot "Espada.Daemon$extension")

$components = @("api", "mcp", "worker", "db")
foreach ($component in $components) {
    $destination = Join-Path $packageRoot "components\$component"
    New-Item -ItemType Directory -Force -Path $destination | Out-Null
    Copy-Item -Path (Join-Path $publishRoot "$component\*") -Destination $destination -Recurse -Force
}

$localRuntime = [ordered]@{
    Espada = [ordered]@{
        LocalRuntime = [ordered]@{
            ApiExecutable = "components/api/Espada.Api$extension"
            McpExecutable = "components/mcp/Espada.Mcp$extension"
            WorkerExecutable = "components/worker/Espada.Worker$extension"
            DbExecutable = "components/db/Espada.Db$extension"
        }
    }
}
$localRuntime | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $packageRoot "appsettings.json") -Encoding utf8NoBOM
Copy-Item -LiteralPath (Join-Path $repositoryRoot "LICENSE") -Destination $packageRoot
Copy-Item -LiteralPath (Join-Path $repositoryRoot "README.md") -Destination $packageRoot

$manifest = [ordered]@{
    product = "Espada"
    runtimeIdentifier = $RuntimeIdentifier
    framework = "net10.0"
    selfContained = $true
    nativeAot = $false
    dockerPrerequisite = $true
    components = @("espada", "Espada.Daemon", "Espada.Api", "Espada.Mcp", "Espada.Worker", "Espada.Db")
}
$manifest | ConvertTo-Json -Depth 3 | Set-Content -LiteralPath (Join-Path $packageRoot "manifest.json") -Encoding utf8NoBOM

$archive = if ($RuntimeIdentifier.StartsWith("win-")) {
    $path = Join-Path $outputRoot "$packageName.zip"
    if (Test-Path -LiteralPath $path) {
        Remove-Item -LiteralPath $path -Force
    }
    Compress-Archive -Path (Join-Path $packageRoot "*") -DestinationPath $path
    $path
}
else {
    $path = Join-Path $outputRoot "$packageName.tar.gz"
    if (Test-Path -LiteralPath $path) {
        Remove-Item -LiteralPath $path -Force
    }
    Push-Location $outputRoot
    try {
        & tar -czf $path $packageName
        if ($LASTEXITCODE -ne 0) {
            throw "Creating the package archive failed."
        }
    }
    finally {
        Pop-Location
    }
    $path
}

Remove-Item -LiteralPath $publishRoot -Recurse -Force
Write-Output $archive
